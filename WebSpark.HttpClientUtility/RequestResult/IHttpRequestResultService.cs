using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace WebSpark.HttpClientUtility.RequestResult;
/// <summary>
/// HttpClientService interface to send HTTP requests.
/// </summary>
/// <remarks>
/// This service uses reflection-based JSON serialization and is not compatible with Native AOT.
/// For AOT scenarios, consumers should use System.Text.Json source generators.
/// </remarks>
public interface IHttpRequestResultService
{
    /// <summary>
    /// Sends an HTTP request asynchronously and returns the result.
    /// </summary>
    /// <typeparam name="T">The type of the expected response data</typeparam>
    /// <param name="httpSendResults">The HTTP request result object containing request details</param>
    /// <param name="memberName">Name of the calling member (automatically populated)</param>
    /// <param name="filePath">File path of the calling code (automatically populated)</param>
    /// <param name="lineNumber">Line number of the calling code (automatically populated)</param>
    /// <param name="ct">Cancellation token to cancel the operation</param>
    /// <returns>A task representing the asynchronous operation with the HTTP request result</returns>
    [RequiresUnreferencedCode("JSON serialization uses reflection. For AOT scenarios, use System.Text.Json source generators.")]
    [RequiresDynamicCode("JSON serialization may require runtime code generation. For AOT scenarios, use System.Text.Json source generators.")]
    Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(HttpRequestResult<T> httpSendResults,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        CancellationToken ct = default);
}
