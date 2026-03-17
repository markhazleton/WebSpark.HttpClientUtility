namespace WebSpark.HttpClientUtility.BatchExecution;

/// <summary>
/// Target API environment for batch execution.
/// </summary>
public sealed class BatchEnvironment
{
    /// <summary>
    /// Display name for the environment.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Absolute base URL for requests in this environment.
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Headers applied to every request for this environment.
    /// </summary>
    public IReadOnlyDictionary<string, string> DefaultHeaders { get; set; } = new Dictionary<string, string>();
}
