namespace WebSpark.HttpClientUtility.BatchExecution;

/// <summary>
/// Result for one expanded execution work item.
/// </summary>
public sealed class BatchExecutionItemResult
{
    /// <summary>
    /// Parent run identifier.
    /// </summary>
    public string RunId { get; set; } = string.Empty;

    /// <summary>
    /// Environment name for this item.
    /// </summary>
    public string EnvironmentName { get; set; } = string.Empty;

    /// <summary>
    /// User identifier for this item when provided.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Request template name.
    /// </summary>
    public string RequestName { get; set; } = string.Empty;

    /// <summary>
    /// One-based iteration number.
    /// </summary>
    public int Iteration { get; set; }

    /// <summary>
    /// Executed HTTP method.
    /// </summary>
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>
    /// Final request URI.
    /// </summary>
    public string RequestPath { get; set; } = string.Empty;

    /// <summary>
    /// Per-request correlation identifier.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// True when status code indicates success.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Numeric status code when available.
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>
    /// Deterministic SHA-256 hash of response body when available.
    /// </summary>
    public string? ResponseBodyHash { get; set; }

    /// <summary>
    /// Request duration in milliseconds.
    /// </summary>
    public long DurationMilliseconds { get; set; }

    /// <summary>
    /// UTC timestamp for item completion.
    /// </summary>
    public DateTimeOffset TimestampUtc { get; set; }

    /// <summary>
    /// Captured errors for failed executions.
    /// </summary>
    public IReadOnlyList<string> ErrorMessages { get; set; } = [];
}
