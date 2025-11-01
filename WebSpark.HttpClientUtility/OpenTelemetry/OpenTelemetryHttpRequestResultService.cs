using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using WebSpark.HttpClientUtility.RequestResult;

namespace WebSpark.HttpClientUtility.OpenTelemetry;

/// <summary>
/// OpenTelemetry-instrumented HTTP request result service that provides standardized telemetry and tracing.
/// This service replaces custom telemetry implementations with industry-standard OpenTelemetry.
/// </summary>
/// <remarks>
/// Features provided by this OpenTelemetry service:
/// - Distributed tracing with automatic span creation
/// - Activity tagging with standardized HTTP semantic conventions
/// - Performance metrics collection
/// - Error tracking and correlation
/// - Integration with observability platforms (Jaeger, Zipkin, etc.)
/// </remarks>
public class OpenTelemetryHttpRequestResultService : IHttpRequestResultService
{
    private readonly IHttpRequestResultService _innerService;
    private readonly ILogger<OpenTelemetryHttpRequestResultService> _logger;
    private readonly ActivitySource _activitySource;

    /// <summary>
    /// Activity source name for HTTP request telemetry.
    /// </summary>
    public const string ActivitySourceName = "WebSpark.HttpClientUtility";

    /// <summary>
    /// Initializes a new instance of the OpenTelemetryHttpRequestResultService.
    /// </summary>
    /// <param name="innerService">The underlying HTTP request service to instrument.</param>
    /// <param name="logger">Logger for diagnostic messages.</param>
    public OpenTelemetryHttpRequestResultService(
        IHttpRequestResultService innerService,
        ILogger<OpenTelemetryHttpRequestResultService> logger)
    {
        _innerService = innerService ?? throw new ArgumentNullException(nameof(innerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _activitySource = new ActivitySource(ActivitySourceName);
    }

    /// <summary>
    /// Sends an HTTP request with comprehensive OpenTelemetry instrumentation.
    /// </summary>
    /// <typeparam name="T">The type of the expected response data.</typeparam>
    /// <param name="httpSendResults">The HTTP request result object containing request details.</param>
    /// <param name="memberName">Name of the calling member (automatically populated).</param>
    /// <param name="filePath">File path of the calling code (automatically populated).</param>
    /// <param name="lineNumber">Line number of the calling code (automatically populated).</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation with the HTTP request result.</returns>
    public async Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(
        HttpRequestResult<T> httpSendResults,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        CancellationToken ct = default)
    {
        // Create a span for this HTTP request operation
        using var activity = _activitySource.StartActivity("http.request");

        if (activity != null)
        {
            // Add standard HTTP semantic convention tags
            activity.SetTag("http.method", httpSendResults.RequestMethod.Method);
            activity.SetTag("http.url", httpSendResults.RequestPath);
            activity.SetTag("http.user_agent", "WebSpark.HttpClientUtility/1.0.9");
            activity.SetTag("correlation.id", httpSendResults.CorrelationId);

            // Add caller context tags
            activity.SetTag("code.function", memberName);
            activity.SetTag("code.filepath", filePath);
            activity.SetTag("code.lineno", lineNumber);

            // Add custom business tags
            activity.SetTag("service.name", "WebSpark.HttpClientUtility");
            activity.SetTag("service.version", "1.0.9");

            _logger.LogDebug(
                "Starting HTTP request with OpenTelemetry trace [TraceId: {TraceId}, SpanId: {SpanId}, CorrelationId: {CorrelationId}]",
                activity.TraceId, activity.SpanId, httpSendResults.CorrelationId);
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Execute the actual HTTP request
            var result = await _innerService.HttpSendRequestResultAsync(
                httpSendResults, memberName, filePath, lineNumber, ct).ConfigureAwait(false);

            stopwatch.Stop();

            // Add response information to the activity
            if (activity != null)
            {
                activity.SetTag("http.status_code", (int)result.StatusCode);
                activity.SetTag("http.response.size", result.ResponseResults?.ToString()?.Length ?? 0);
                activity.SetTag("duration.milliseconds", stopwatch.ElapsedMilliseconds);

                // Set activity status based on response
                if ((int)result.StatusCode >= 400)
                {
                    activity.SetStatus(ActivityStatusCode.Error, $"HTTP {result.StatusCode}");

                    // Add error details if available
                    if (result.ErrorList.Any())
                    {
                        activity.SetTag("error.message", string.Join("; ", result.ErrorList));
                    }
                }
                else
                {
                    activity.SetStatus(ActivityStatusCode.Ok);
                }

                _logger.LogDebug(
                    "Completed HTTP request [TraceId: {TraceId}, SpanId: {SpanId}, StatusCode: {StatusCode}, Duration: {Duration}ms, CorrelationId: {CorrelationId}]",
                    activity.TraceId, activity.SpanId, result.StatusCode, stopwatch.ElapsedMilliseconds, httpSendResults.CorrelationId);
            }

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Record exception in the activity
            if (activity != null)
            {
                activity.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity.SetTag("exception.type", ex.GetType().FullName);
                activity.SetTag("exception.message", ex.Message);
                activity.SetTag("exception.stacktrace", ex.StackTrace);
                activity.SetTag("duration.milliseconds", stopwatch.ElapsedMilliseconds);

                _logger.LogError(ex,
                    "HTTP request failed with exception [TraceId: {TraceId}, SpanId: {SpanId}, Duration: {Duration}ms, CorrelationId: {CorrelationId}]",
                    activity.TraceId, activity.SpanId, stopwatch.ElapsedMilliseconds, httpSendResults.CorrelationId);
            }

            // Let the inner service handle the exception processing
            throw;
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
