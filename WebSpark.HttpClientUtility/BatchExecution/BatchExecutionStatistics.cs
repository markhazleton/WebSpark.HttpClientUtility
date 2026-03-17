namespace WebSpark.HttpClientUtility.BatchExecution;

/// <summary>
/// Aggregate metrics for a batch execution run.
/// </summary>
public sealed class BatchExecutionStatistics
{
    /// <summary>
    /// Number of completed items.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Number of successful completed items.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Number of failed completed items.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// P50 latency in milliseconds.
    /// </summary>
    public double P50Milliseconds { get; set; }

    /// <summary>
    /// P95 latency in milliseconds.
    /// </summary>
    public double P95Milliseconds { get; set; }

    /// <summary>
    /// P99 latency in milliseconds.
    /// </summary>
    public double P99Milliseconds { get; set; }

    /// <summary>
    /// Completed item counts grouped by HTTP method.
    /// </summary>
    public IReadOnlyDictionary<string, int> ByMethod { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// Completed item counts grouped by environment.
    /// </summary>
    public IReadOnlyDictionary<string, int> ByEnvironment { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// Completed item counts grouped by user.
    /// </summary>
    public IReadOnlyDictionary<string, int> ByUser { get; set; } = new Dictionary<string, int>();

    /// <summary>
    /// Completed item counts grouped by HTTP status code.
    /// </summary>
    public IReadOnlyDictionary<int, int> ByStatusCode { get; set; } = new Dictionary<int, int>();
}
