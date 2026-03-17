using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using WebSpark.HttpClientUtility.RequestResult;

namespace WebSpark.HttpClientUtility.BatchExecution;

/// <summary>
/// Expands and executes batch request combinations through <see cref="IHttpRequestResultService"/>.
/// </summary>
public sealed class BatchExecutionService : IBatchExecutionService
{
    private readonly IHttpRequestResultService _requestResultService;
    private readonly ITemplateSubstitutionService _templateSubstitutionService;
    private readonly ILogger<BatchExecutionService> _logger;

    /// <summary>
    /// Creates a new orchestrator.
    /// </summary>
    public BatchExecutionService(
        IHttpRequestResultService requestResultService,
        ITemplateSubstitutionService templateSubstitutionService,
        ILogger<BatchExecutionService> logger)
    {
        _requestResultService = requestResultService;
        _templateSubstitutionService = templateSubstitutionService;
        _logger = logger;
    }

    /// <inheritdoc />
    [RequiresUnreferencedCode("Batch execution uses IHttpRequestResultService JSON serialization which relies on reflection.")]
    [RequiresDynamicCode("Batch execution delegates to IHttpRequestResultService which may require runtime code generation.")]
    public async Task<BatchExecutionResult> ExecuteAsync(
        BatchExecutionConfiguration configuration,
        IBatchExecutionResultSink? resultSink = null,
        IProgress<BatchProgress>? progress = null,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ValidateConfiguration(configuration);

        var runId = string.IsNullOrWhiteSpace(configuration.RunId)
            ? $"batch-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}"
            : configuration.RunId.Trim();

        var workItems = Expand(configuration, runId);
        var totalPlanned = workItems.Count;

        if (totalPlanned == 0)
        {
            return new BatchExecutionResult
            {
                RunId = runId,
                TotalPlannedCount = 0,
                CompletedCount = 0,
                WasCancelled = ct.IsCancellationRequested,
                Statistics = new BatchExecutionStatistics(),
                Results = []
            };
        }

        var collector = new BatchStatisticsCollector();
        var results = new ConcurrentBag<BatchExecutionItemResult>();
        var semaphore = new SemaphoreSlim(configuration.MaxConcurrency, configuration.MaxConcurrency);
        var tasks = new List<Task>(totalPlanned);
        var completed = 0;
        var dispatchCancelled = false;

        foreach (var item in workItems)
        {
            if (ct.IsCancellationRequested)
            {
                dispatchCancelled = true;
                break;
            }

            try
            {
                await semaphore.WaitAsync(ct).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                dispatchCancelled = true;
                break;
            }

            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    var itemResult = await ExecuteOneAsync(item, ct).ConfigureAwait(false);
                    results.Add(itemResult);
                    collector.Record(itemResult);

                    if (resultSink is not null)
                    {
                        await resultSink.OnResultAsync(itemResult, ct).ConfigureAwait(false);
                    }

                    var currentCompleted = Interlocked.Increment(ref completed);
                    progress?.Report(new BatchProgress
                    {
                        RunId = runId,
                        CompletedCount = currentCompleted,
                        TotalPlannedCount = totalPlanned,
                        StatisticsSnapshot = collector.Snapshot(),
                        LastUpdatedUtc = DateTimeOffset.UtcNow
                    });
                }
                finally
                {
                    semaphore.Release();
                }
            }, CancellationToken.None));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        var finalStatistics = collector.Snapshot();
        var finalResults = results
            .OrderBy(x => x.EnvironmentName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.UserId, StringComparer.Ordinal)
            .ThenBy(x => x.RequestName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.Iteration)
            .ToArray();

        return new BatchExecutionResult
        {
            RunId = runId,
            TotalPlannedCount = totalPlanned,
            CompletedCount = completed,
            WasCancelled = dispatchCancelled || ct.IsCancellationRequested,
            Statistics = finalStatistics,
            Results = finalResults
        };
    }

    private static void ValidateConfiguration(BatchExecutionConfiguration configuration)
    {
        if (configuration.Iterations < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(configuration.Iterations), configuration.Iterations, "Iterations must be non-negative.");
        }

        if (configuration.MaxConcurrency < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(configuration.MaxConcurrency), configuration.MaxConcurrency, "MaxConcurrency must be at least 1.");
        }

        foreach (var environment in configuration.Environments)
        {
            ArgumentNullException.ThrowIfNull(environment);

            if (!Uri.TryCreate(environment.BaseUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException($"Environment '{environment.Name}' has an invalid absolute HTTP/HTTPS BaseUrl.", nameof(configuration));
            }
        }
    }

    private List<WorkItem> Expand(BatchExecutionConfiguration configuration, string runId)
    {
        if (configuration.Environments.Count == 0 ||
            configuration.Users.Count == 0 ||
            configuration.Requests.Count == 0 ||
            configuration.Iterations == 0)
        {
            return [];
        }

        var items = new List<WorkItem>(
            configuration.Environments.Count * configuration.Users.Count * configuration.Requests.Count * configuration.Iterations);

        foreach (var environment in configuration.Environments)
        {
            foreach (var user in configuration.Users)
            {
                foreach (var request in configuration.Requests)
                {
                    for (var iteration = 1; iteration <= configuration.Iterations; iteration++)
                    {
                        items.Add(new WorkItem(runId, environment, user, request, iteration));
                    }
                }
            }
        }

        return items;
    }

    [RequiresUnreferencedCode("Uses IHttpRequestResultService JSON serialization that relies on reflection.")]
    [RequiresDynamicCode("Uses IHttpRequestResultService which may require runtime code generation.")]
    private async Task<BatchExecutionItemResult> ExecuteOneAsync(WorkItem item, CancellationToken ct)
    {
        var renderedPath = _templateSubstitutionService.Render(item.Request.PathTemplate, item.User);
        var requestUri = BuildRequestUri(item.Environment.BaseUrl, renderedPath);
        var method = new HttpMethod(item.Request.Method);
        var startedAt = DateTimeOffset.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        var request = new HttpRequestResult<string>
        {
            RequestPath = requestUri,
            RequestMethod = method,
            RequestHeaders = BuildHeaders(item.Environment.DefaultHeaders, item.Request.Headers),
            CorrelationId = Guid.NewGuid().ToString(),
            Iteration = item.Iteration,
            RequestContext = new Dictionary<string, object>
            {
                ["BatchRunId"] = item.RunId,
                ["BatchEnvironment"] = item.Environment.Name,
                ["BatchUser"] = item.User.UserId,
                ["BatchRequest"] = item.Request.Name
            }
        };

        if (ShouldAttachBody(item.Request))
        {
            var renderedBody = _templateSubstitutionService.Render(item.Request.BodyTemplate ?? string.Empty, item.User);
            request.RequestBody = new StringContent(
                renderedBody,
                Encoding.UTF8,
                string.IsNullOrWhiteSpace(item.Request.ContentType) ? "application/json" : item.Request.ContentType);
        }

        try
        {
            var response = await _requestResultService.HttpSendRequestResultAsync(request, ct: ct).ConfigureAwait(false);
            stopwatch.Stop();

            return new BatchExecutionItemResult
            {
                RunId = item.RunId,
                EnvironmentName = item.Environment.Name,
                UserId = item.User.UserId,
                RequestName = item.Request.Name,
                Iteration = item.Iteration,
                HttpMethod = method.Method,
                RequestPath = requestUri,
                CorrelationId = response.CorrelationId,
                IsSuccess = response.IsSuccessStatusCode,
                StatusCode = (int)response.StatusCode,
                ResponseBodyHash = ComputeHash(response.ResponseResults),
                DurationMilliseconds = stopwatch.ElapsedMilliseconds,
                TimestampUtc = startedAt,
                ErrorMessages = response.ErrorList.ToArray()
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogWarning(ex, "Batch execution item failed for run {RunId} {Method} {Path}", item.RunId, method.Method, requestUri);

            return new BatchExecutionItemResult
            {
                RunId = item.RunId,
                EnvironmentName = item.Environment.Name,
                UserId = item.User.UserId,
                RequestName = item.Request.Name,
                Iteration = item.Iteration,
                HttpMethod = method.Method,
                RequestPath = requestUri,
                CorrelationId = request.CorrelationId,
                IsSuccess = false,
                StatusCode = null,
                ResponseBodyHash = null,
                DurationMilliseconds = stopwatch.ElapsedMilliseconds,
                TimestampUtc = startedAt,
                ErrorMessages = [ex.Message]
            };
        }
    }

    private static Dictionary<string, string> BuildHeaders(
        IReadOnlyDictionary<string, string> environmentHeaders,
        IReadOnlyDictionary<string, string> requestHeaders)
    {
        var merged = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var pair in environmentHeaders)
        {
            merged[pair.Key] = pair.Value;
        }

        foreach (var pair in requestHeaders)
        {
            merged[pair.Key] = pair.Value;
        }

        return merged;
    }

    private static bool ShouldAttachBody(BatchRequestDefinition request)
    {
        if (string.IsNullOrWhiteSpace(request.BodyTemplate))
        {
            return false;
        }

        if (request.IsBodyCapable)
        {
            return true;
        }

        var method = request.Method?.Trim() ?? string.Empty;
        return method.Equals("POST", StringComparison.OrdinalIgnoreCase)
            || method.Equals("PUT", StringComparison.OrdinalIgnoreCase)
            || method.Equals("PATCH", StringComparison.OrdinalIgnoreCase);
    }

    private static string BuildRequestUri(string baseUrl, string path)
    {
        if (Uri.TryCreate(path, UriKind.Absolute, out var absolute))
        {
            return absolute.ToString();
        }

        var baseUri = new Uri(baseUrl.EndsWith("/", StringComparison.Ordinal) ? baseUrl : baseUrl + "/");
        var relative = path.StartsWith("/", StringComparison.Ordinal) ? path[1..] : path;
        return new Uri(baseUri, relative).ToString();
    }

    private static string? ComputeHash(string? responseBody)
    {
        if (string.IsNullOrEmpty(responseBody))
        {
            return null;
        }

        var data = Encoding.UTF8.GetBytes(responseBody);
        var hash = SHA256.HashData(data);
        return Convert.ToHexString(hash);
    }

    private sealed record WorkItem(
        string RunId,
        BatchEnvironment Environment,
        BatchUserContext User,
        BatchRequestDefinition Request,
        int Iteration);
}
