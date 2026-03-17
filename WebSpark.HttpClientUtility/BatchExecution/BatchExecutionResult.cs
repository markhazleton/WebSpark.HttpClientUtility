namespace WebSpark.HttpClientUtility.BatchExecution;

/// <summary>
/// Final batch run outcome.
/// </summary>
public sealed class BatchExecutionResult
{
    /// <summary>
    /// Parent run identifier.
    /// </summary>
    public string RunId { get; set; } = string.Empty;

    /// <summary>
    /// Number of items planned at execution start.
    /// </summary>
    public int TotalPlannedCount { get; set; }

    /// <summary>
    /// Number of items that completed.
    /// </summary>
    public int CompletedCount { get; set; }

    /// <summary>
    /// True when cancellation interrupted dispatch.
    /// </summary>
    public bool WasCancelled { get; set; }

    /// <summary>
    /// Aggregate execution statistics.
    /// </summary>
    public BatchExecutionStatistics Statistics { get; set; } = new();

    /// <summary>
    /// Completed item results.
    /// </summary>
    public IReadOnlyList<BatchExecutionItemResult> Results { get; set; } = [];
}
