using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Net;
using WebSpark.HttpClientUtility.ClientService;

namespace WebSpark.HttpClientUtility.OpenTelemetry;

/// <summary>
/// OpenTelemetry-instrumented HTTP client service that provides standardized telemetry and tracing.
/// This service replaces the custom HttpClientServiceTelemetry with industry-standard OpenTelemetry.
/// </summary>
/// <remarks>
/// Features provided by this OpenTelemetry service:
/// - Distributed tracing with automatic span creation
/// - Activity tagging with standardized HTTP semantic conventions
/// - Performance metrics collection
/// - Error tracking and correlation
/// - Integration with observability platforms
/// </remarks>
public class OpenTelemetryHttpClientService : IHttpClientService
{
    private readonly IHttpClientService _innerService;
    private readonly ILogger<OpenTelemetryHttpClientService> _logger;
    private readonly ActivitySource _activitySource;

    /// <summary>
    /// Activity source name for HTTP client telemetry.
    /// </summary>
    public const string ActivitySourceName = "WebSpark.HttpClientUtility.HttpClient";

    /// <summary>
    /// Initializes a new instance of the OpenTelemetryHttpClientService.
    /// </summary>
    /// <param name="innerService">The underlying HTTP client service to instrument.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    public OpenTelemetryHttpClientService(
        IHttpClientService innerService,
        ILogger<OpenTelemetryHttpClientService> logger)
    {
        _innerService = innerService ?? throw new ArgumentNullException(nameof(innerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _activitySource = new ActivitySource(ActivitySourceName);
    }

    /// <inheritdoc/>
    public HttpClient CreateConfiguredClient()
    {
        return _innerService.CreateConfiguredClient();
    }

    /// <inheritdoc/>
    public async Task<HttpResponseContent<TResult>> DeleteAsync<TResult>(Uri requestUri, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithTelemetryAsync(
            "DELETE",
            requestUri,
            () => _innerService.DeleteAsync<TResult>(requestUri, cancellationToken),
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<HttpResponseContent<T>> GetAsync<T>(Uri requestUri, CancellationToken cancellationToken)
    {
        return await ExecuteWithTelemetryAsync(
            "GET",
            requestUri,
            () => _innerService.GetAsync<T>(requestUri, cancellationToken),
            cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<HttpResponseContent<TResult>> PostAsync<T, TResult>(Uri requestUri, T payload, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithTelemetryAsync(
            "POST",
            requestUri,
            () => _innerService.PostAsync<T, TResult>(requestUri, payload, cancellationToken),
            cancellationToken,
            payload).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<HttpResponseContent<TResult>> PostAsync<T, TResult>(Uri requestUri, T payload, Dictionary<string, string> headers, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithTelemetryAsync(
            "POST",
            requestUri,
            () => _innerService.PostAsync<T, TResult>(requestUri, payload, headers, cancellationToken),
            cancellationToken,
            payload,
            headers).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<HttpResponseContent<TResult>> PutAsync<T, TResult>(Uri requestUri, T payload, CancellationToken cancellationToken = default)
    {
        return await ExecuteWithTelemetryAsync(
            "PUT",
            requestUri,
            () => _innerService.PutAsync<T, TResult>(requestUri, payload, cancellationToken),
            cancellationToken,
            payload).ConfigureAwait(false);
    }

    /// <summary>
    /// Executes an HTTP operation with comprehensive OpenTelemetry instrumentation.
    /// </summary>
    /// <typeparam name="TResult">The type of the HTTP response content.</typeparam>
    /// <param name="method">The HTTP method being executed.</param>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="payload">Optional request payload for tagging.</param>
    /// <param name="headers">Optional headers for tagging.</param>
    /// <returns>The result of the HTTP operation.</returns>
    private async Task<HttpResponseContent<TResult>> ExecuteWithTelemetryAsync<TResult>(
        string method,
        Uri requestUri,
        Func<Task<HttpResponseContent<TResult>>> operation,
        CancellationToken cancellationToken,
        object? payload = null,
        Dictionary<string, string>? headers = null)
    {
        // Create a span for this HTTP client operation
        using var activity = _activitySource.StartActivity($"http.client.{method.ToLowerInvariant()}");

        if (activity != null)
        {
            // Add standard HTTP semantic convention tags
            activity.SetTag("http.method", method);
            activity.SetTag("http.url", requestUri.ToString());
            activity.SetTag("http.scheme", requestUri.Scheme);
            activity.SetTag("http.host", requestUri.Host);
            activity.SetTag("http.target", requestUri.PathAndQuery);

            if (requestUri.Port != 80 && requestUri.Port != 443)
            {
                activity.SetTag("http.server.port", requestUri.Port);
            }

            // Add payload information if available
            if (payload != null)
            {
                activity.SetTag("http.request.body.size", payload.ToString()?.Length ?? 0);
                activity.SetTag("http.request.content_type", "application/json");
            }

            // Add header information if available
            if (headers?.Any() == true)
            {
                activity.SetTag("http.request.headers.count", headers.Count);

                // Add specific header values that are safe to log
                if (headers.ContainsKey("Authorization"))
                {
                    activity.SetTag("http.request.headers.authorization", "***");
                }
            }

            // Add custom business tags
            activity.SetTag("service.name", "WebSpark.HttpClientUtility");
            activity.SetTag("service.version", "1.0.9");
            activity.SetTag("component", "HttpClientService");

            _logger.LogDebug(
                "Starting HTTP {Method} request to {Uri} [TraceId: {TraceId}, SpanId: {SpanId}]",
                method, requestUri, activity.TraceId, activity.SpanId);
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Execute the actual HTTP operation
            var result = await operation().ConfigureAwait(false);

            stopwatch.Stop();

            // Add response information to the activity
            if (activity != null)
            {
                activity.SetTag("http.status_code", (int)result.StatusCode);
                activity.SetTag("duration.milliseconds", stopwatch.ElapsedMilliseconds);

                if (result.Content != null)
                {
                    activity.SetTag("http.response.body.size", result.Content.ToString()?.Length ?? 0);
                }

                // Set activity status based on response
                if ((int)result.StatusCode >= 400)
                {
                    activity.SetStatus(ActivityStatusCode.Error, $"HTTP {result.StatusCode}");

                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        activity.SetTag("error.message", result.ErrorMessage);
                    }
                }
                else
                {
                    activity.SetStatus(ActivityStatusCode.Ok);
                }

                _logger.LogDebug(
                    "Completed HTTP {Method} request to {Uri} [TraceId: {TraceId}, SpanId: {SpanId}, StatusCode: {StatusCode}, Duration: {Duration}ms]",
                    method, requestUri, activity.TraceId, activity.SpanId, result.StatusCode, stopwatch.ElapsedMilliseconds);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Create a failure result with telemetry
            var failureResult = HttpResponseContent<TResult>.Failure(
                $"HTTP Request Exception: {ex.Message}",
                HttpStatusCode.ServiceUnavailable);

            failureResult.ElapsedMilliseconds = stopwatch.ElapsedMilliseconds;
            failureResult.CompletionDate = DateTime.UtcNow;

            // Record exception in the activity
            if (activity != null)
            {
                activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity.SetTag("exception.type", ex.GetType().FullName);
                activity.SetTag("exception.message", ex.Message);
                activity.SetTag("exception.stacktrace", ex.StackTrace);
                activity.SetTag("duration.milliseconds", stopwatch.ElapsedMilliseconds);

                _logger.LogError(ex,
                    "HTTP {Method} request to {Uri} failed [TraceId: {TraceId}, SpanId: {SpanId}, Duration: {Duration}ms]",
                    method, requestUri, activity.TraceId, activity.SpanId, stopwatch.ElapsedMilliseconds);
            }

            return failureResult;
        }
    }

    /// <summary>
    /// Disposes of the ActivitySource when the service is disposed.
    /// </summary>
    public void Dispose()
    {
        _activitySource?.Dispose();

        if (_innerService is IDisposable disposableService)
        {
            disposableService.Dispose();
        }
    }
}
