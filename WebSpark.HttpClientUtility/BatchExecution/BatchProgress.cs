namespace WebSpark.HttpClientUtility.BatchExecution;

/// <summary>
/// Incremental progress payload emitted during batch execution.
/// </summary>
public sealed class BatchProgress
{
    /// <summary>
    /// Parent run identifier.
    /// </summary>
    public string RunId { get; set; } = string.Empty;

    /// <summary>
    /// Number of completed items at the time of reporting.
    /// </summary>
    public int CompletedCount { get; set; }

    /// <summary>
    /// Total number of planned work items.
    /// </summary>
    public int TotalPlannedCount { get; set; }

    /// <summary>
    /// Snapshot of aggregate statistics at report time.
    /// </summary>
    public BatchExecutionStatistics StatisticsSnapshot { get; set; } = new();

    /// <summary>
    /// UTC timestamp for this progress update.
    /// </summary>
    public DateTimeOffset LastUpdatedUtc { get; set; }
}
