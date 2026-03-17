using System.Collections.Concurrent;
using WebSpark.HttpClientUtility.BatchExecution;
using WebSpark.HttpClientUtility.Web.Models;

namespace WebSpark.HttpClientUtility.Web.Services;

public sealed class BatchExecutionDemoService
{
    private readonly IBatchExecutionService _batchExecutionService;
    private readonly ILogger<BatchExecutionDemoService> _logger;
    private readonly ConcurrentDictionary<string, DemoRunRecord> _runs = new(StringComparer.Ordinal);

    public const int DemoMaxPlannedRequests = 50;

    public BatchExecutionDemoService(
        IBatchExecutionService batchExecutionService,
        ILogger<BatchExecutionDemoService> logger)
    {
        _batchExecutionService = batchExecutionService;
        _logger = logger;
    }

    public async Task<(bool Accepted, string? Error, DemoRunAcceptedResponse? Response)> StartRunAsync(DemoStartRunRequest request, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        request.Environments ??= [];
        request.Users ??= [];
        request.Requests ??= [];

        var plannedCount = CalculatePlannedCount(request);
        if (plannedCount > DemoMaxPlannedRequests)
        {
            return (false, $"The demo is limited to {DemoMaxPlannedRequests} planned requests per run.", null);
        }

        var runId = $"batch-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}";
        var record = new DemoRunRecord
        {
            RunId = runId,
            TotalPlannedCount = plannedCount,
            State = DemoRunState.Queued,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(30)
        };

        _runs[runId] = record;

        _ = Task.Run(async () =>
        {
            record.State = DemoRunState.Running;
            try
            {
                var configuration = new BatchExecutionConfiguration
                {
                    RunId = runId,
                    Environments = request.Environments,
                    Users = request.Users,
                    Requests = request.Requests,
                    Iterations = request.Iterations,
                    MaxConcurrency = request.MaxConcurrency
                };

                var progress = new Progress<BatchProgress>(p => record.LatestProgress = p);
                record.FinalResult = await _batchExecutionService.ExecuteAsync(configuration, progress: progress, ct: ct).ConfigureAwait(false);
                record.State = record.FinalResult.WasCancelled ? DemoRunState.Cancelled : DemoRunState.Completed;
            }
            catch (OperationCanceledException)
            {
                record.State = DemoRunState.Cancelled;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Demo run {RunId} failed", runId);
                record.State = DemoRunState.Failed;
            }
        }, CancellationToken.None);

        return (true, null, new DemoRunAcceptedResponse
        {
            RunId = runId,
            TotalPlannedCount = plannedCount,
            Status = DemoRunState.Queued.ToString()
        });
    }

    public DemoRunStatusResponse? GetRunStatus(string runId)
    {
        if (!_runs.TryGetValue(runId, out var record) || record.ExpiresAtUtc < DateTimeOffset.UtcNow)
        {
            return null;
        }

        var statistics = record.FinalResult?.Statistics ?? record.LatestProgress?.StatisticsSnapshot ?? new BatchExecutionStatistics();
        var completed = record.FinalResult?.CompletedCount ?? record.LatestProgress?.CompletedCount ?? 0;

        return new DemoRunStatusResponse
        {
            RunId = record.RunId,
            Status = record.State.ToString(),
            CompletedCount = completed,
            TotalPlannedCount = record.TotalPlannedCount,
            Statistics = statistics,
            Results = record.FinalResult?.Results
        };
    }

    public static int CalculatePlannedCount(DemoStartRunRequest request)
    {
        if (request.Iterations <= 0 || request.Environments.Count == 0 || request.Users.Count == 0 || request.Requests.Count == 0)
        {
            return 0;
        }

        return request.Environments.Count * request.Users.Count * request.Requests.Count * request.Iterations;
    }

    private sealed class DemoRunRecord
    {
        public string RunId { get; set; } = string.Empty;
        public int TotalPlannedCount { get; set; }
        public DemoRunState State { get; set; }
        public BatchProgress? LatestProgress { get; set; }
        public BatchExecutionResult? FinalResult { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset ExpiresAtUtc { get; set; }
    }
}
