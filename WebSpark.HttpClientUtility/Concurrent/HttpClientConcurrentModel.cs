using WebSpark.HttpClientUtility.RequestResult;

namespace WebSpark.HttpClientUtility.Concurrent;

/// <summary>
/// Represents a data model for concurrent HTTP client operations.
/// </summary>
/// <remarks>
/// This model encapsulates all the data needed for processing HTTP requests concurrently,
/// including request details, task tracking information, and performance metrics.
/// It extends the base ConcurrentProcessorModel with HTTP-specific functionality.
/// </remarks>
public class HttpClientConcurrentModel : ConcurrentProcessorModel
{
    /// <summary>
    /// Initializes a new instance of the HttpClientConcurrentModel class with the specified task ID and request URL.
    /// </summary>
    /// <param name="taskId">The task identifier within the concurrent operation batch.</param>
    /// <param name="requestUrl">The URL endpoint for the HTTP request.</param>
    /// <remarks>
    /// This constructor creates a new concurrent model with a fresh HttpRequestResult object
    /// initialized with the given task ID and request URL.
    /// </remarks>
    public HttpClientConcurrentModel(int taskId, string requestUrl) : base(taskId)
    {
        StatusCall = new HttpRequestResult<SiteStatus>(taskId, requestUrl);
        TaskId = taskId;
    }

    /// <summary>
    /// Initializes a new instance of the HttpClientConcurrentModel class by cloning an existing model.
    /// </summary>
    /// <param name="model">The source model to clone properties from.</param>
    /// <param name="endPoint">The endpoint URL (used for contextual information).</param>
    /// <remarks>
    /// This copy constructor is useful when creating a new model instance that preserves
    /// the state and metrics of an existing model, such as when passing results between
    /// concurrent processing stages.
    /// </remarks>
    public HttpClientConcurrentModel(HttpClientConcurrentModel model, string endPoint) : base(model.TaskId)
    {
        StatusCall = model.StatusCall;
        TaskId = model.TaskId;
        TaskCount = model.TaskCount;
        DurationMS = model.DurationMS;
        SemaphoreCount = model.SemaphoreCount;
        SemaphoreWaitTicks = model.SemaphoreWaitTicks;
    }

    /// <summary>
    /// Gets or sets the HTTP request result object associated with this concurrent task.
    /// </summary>
    /// <remarks>
    /// This property holds both the request details and, after processing, the response data.
    /// It maintains the complete state of the HTTP operation, including status codes,
    /// timing information, error details, and the typed response content.
    /// </remarks>
    public HttpRequestResult<SiteStatus> StatusCall { get; set; } = default!;
}
