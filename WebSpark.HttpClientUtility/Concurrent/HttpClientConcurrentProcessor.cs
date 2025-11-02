using System.Diagnostics;
using WebSpark.HttpClientUtility.RequestResult;

namespace WebSpark.HttpClientUtility.Concurrent;

/// <summary>
/// Provides functionality for processing multiple HTTP requests concurrently with throttling and progress tracking.
/// </summary>
/// <remarks>
/// The HttpClientConcurrentProcessor allows batch processing of HTTP requests in parallel while:
/// - Controlling the maximum degree of parallelism
/// - Tracking progress and duration of individual requests
/// - Preserving request metadata through the concurrent execution
/// - Managing cancellation and error handling across all concurrent operations
/// 
/// This processor inherits from the generic ConcurrentProcessor&lt;T&gt; and specializes it
/// for HTTP client operations using HttpRequestResult for tracking request/response data.
/// </remarks>
/// <param name="taskDataFactory">Factory function to create task data models with specific request details.</param>
/// <param name="service">The HTTP request service used to execute individual requests.</param>
public class HttpClientConcurrentProcessor(Func<int, HttpClientConcurrentModel> taskDataFactory, IHttpRequestResultService service) : ConcurrentProcessor<HttpClientConcurrentModel>(taskDataFactory)
{
    private readonly IHttpRequestResultService service = service ?? throw new ArgumentNullException(nameof(service));

    /// <summary>
    /// Gets the next task data based on the current task data for request chaining.
    /// </summary>
    /// <param name="taskData">The current task data containing request information.</param>
    /// <returns>
    /// A new HttpClientConcurrentModel with an incremented TaskId if the maximum task count hasn't been reached,
    /// or null if there are no more tasks to process.
    /// </returns>
    /// <remarks>
    /// This method enables sequential task enumeration for batch processing. It creates
    /// the next task in the sequence until MaxTaskCount is reached, allowing controlled
    /// parallel processing of a finite set of tasks.
    /// </remarks>
    protected override HttpClientConcurrentModel? GetNextTaskData(HttpClientConcurrentModel taskData)
    {
        ArgumentNullException.ThrowIfNull(taskData);

        if (taskData.TaskId < MaxTaskCount)
        {
            return new HttpClientConcurrentModel(taskData.TaskId + 1, taskData.StatusCall.RequestPath);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Processes a single HTTP request asynchronously as part of the concurrent operation.
    /// </summary>
    /// <param name="taskData">The task data containing the HTTP request to process.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    /// <returns>
    /// A task representing the asynchronous operation, with the result containing
    /// the processed HttpClientConcurrentModel with request results and timing information.
    /// </returns>
    /// <remarks>
    /// This method:
    /// 1. Measures the duration of the HTTP request
    /// 2. Executes the HTTP request using the provided service
    /// 3. Captures the response in the task data model
    /// 4. Records performance metrics
    /// 
    /// The processed task data includes the complete HTTP response and timing information,
    /// which can be used for reporting, logging, or further processing.
    /// </remarks>
    protected override async Task<HttpClientConcurrentModel> ProcessAsync(
        HttpClientConcurrentModel taskData,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(taskData);

        Stopwatch sw = Stopwatch.StartNew();
        var result = await service.HttpSendRequestResultAsync(taskData.StatusCall, ct: ct).ConfigureAwait(false);
        taskData.StatusCall = result;
        sw.Stop();
        taskData.DurationMS = sw.ElapsedMilliseconds;
        return new HttpClientConcurrentModel(taskData, taskData.StatusCall.RequestPath);
    }
}
