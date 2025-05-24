using WebSpark.HttpClientUtility.Authentication;

namespace WebSpark.HttpClientUtility.RequestResult;

/// <summary>
/// Concrete class for HTTP requests with specific response type.
/// </summary>
/// <typeparam name="T">The type of the response data.</typeparam>
/// <remarks>
/// This class extends the base HttpRequestResultBase by adding strong typing for the response data.
/// It encapsulates all information about an HTTP request and its response, including status codes,
/// timing information, error details, authentication providers, and the typed response content.
/// </remarks>
public class HttpRequestResult<T> : HttpRequestResultBase
{
    /// <summary>
    /// Gets or sets the strongly-typed response results.
    /// </summary>
    /// <remarks>
    /// This property will be populated with the deserialized response content
    /// after a successful request. If the request fails, this will be the default value for type T.
    /// </remarks>
    public T? ResponseResults { get; set; }

    /// <summary>
    /// Gets or sets the authentication provider to use for this request.
    /// </summary>
    /// <remarks>
    /// If set, this provider will be used to add authentication headers to the request.
    /// This allows for per-request authentication configuration while maintaining backward compatibility.
    /// If not set, any existing RequestHeaders will be used as-is.
    /// </remarks>
    public IAuthenticationProvider? AuthenticationProvider { get; set; }

    /// <summary>
    /// Initializes a new instance of the HttpRequestResult class with default values.
    /// </summary>
    /// <remarks>
    /// Creates a new HttpRequestResult with a unique correlation ID and initialized collections.
    /// </remarks>
    public HttpRequestResult() : base() { }

    /// <summary>
    /// Initializes a new instance of the HttpRequestResult class with the specified iteration and path.
    /// </summary>
    /// <param name="it">The iteration number for this request.</param>
    /// <param name="path">The URL path for this request.</param>
    /// <remarks>
    /// This constructor is useful when tracking multiple attempts of the same request
    /// or when executing batches of similar requests.
    /// </remarks>
    public HttpRequestResult(int it, string path) : base()
    {
        Iteration = it;
        RequestPath = path;
    }

    /// <summary>
    /// Initializes a new instance of the HttpRequestResult class by copying properties from an existing base request.
    /// </summary>
    /// <param name="request">The base request to copy properties from.</param>
    /// <remarks>
    /// This constructor is useful when you need to convert a non-generic request to a typed one,
    /// or when implementing decorators that wrap existing request objects.
    /// </remarks>
    public HttpRequestResult(HttpRequestResultBase request) : base()
    {
        Iteration = request.Iteration;
        RequestPath = request.RequestPath;
        RequestMethod = request.RequestMethod;
        RequestBody = request.RequestBody;
        RequestHeaders = request.RequestHeaders;
        CacheDurationMinutes = request.CacheDurationMinutes;
        Retries = request.Retries;
        CorrelationId = request.CorrelationId;
        RequestContext = request.RequestContext;
        IsDebugEnabled = request.IsDebugEnabled;
    }

    /// <summary>
    /// Gets a value indicating whether the HTTP response status code indicates success.
    /// </summary>
    /// <remarks>
    /// Returns true if the status code is in the 2xx range (200-299).
    /// </remarks>
    public bool IsSuccessStatusCode => (int)StatusCode >= 200 && (int)StatusCode <= 299;

    /// <summary>
    /// Gets the elapsed time in milliseconds from when the request started to when it completed.
    /// </summary>
    /// <remarks>
    /// This property can be used for performance monitoring and SLA tracking.
    /// </remarks>
    public long RequestDurationMilliseconds => ElapsedMilliseconds;

    /// <summary>
    /// Returns a string representation of this HttpRequestResult.
    /// </summary>
    /// <returns>A string containing key information about this request and its result.</returns>
    public override string ToString()
    {
        var statusDescription = IsSuccessStatusCode ? "Success" : "Failed";
        return $"{statusDescription} | {RequestMethod.Method} {SafeRequestPath} | Status: {StatusCode} | Duration: {RequestDurationMilliseconds}ms | CorrelationId: {CorrelationId}";
    }
}
