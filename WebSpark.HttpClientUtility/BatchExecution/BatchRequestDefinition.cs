namespace WebSpark.HttpClientUtility.BatchExecution;

/// <summary>
/// Parameterized request template used by batch execution.
/// </summary>
public sealed class BatchRequestDefinition
{
    /// <summary>
    /// Friendly request name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// HTTP method name.
    /// </summary>
    public string Method { get; set; } = "GET";

    /// <summary>
    /// Relative or absolute path template.
    /// </summary>
    public string PathTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Optional request body template.
    /// </summary>
    public string? BodyTemplate { get; set; }

    /// <summary>
    /// Indicates whether the request should include a body.
    /// </summary>
    public bool IsBodyCapable { get; set; }

    /// <summary>
    /// Request-specific headers.
    /// </summary>
    public IReadOnlyDictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Content type used when a body is attached.
    /// </summary>
    public string? ContentType { get; set; }
}
