namespace WebSpark.HttpClientUtility.BatchExecution;

/// <summary>
/// Configuration for a batch orchestration run.
/// </summary>
public sealed class BatchExecutionConfiguration
{
    /// <summary>
    /// Optional caller-provided run identifier.
    /// </summary>
    public string? RunId { get; set; }

    /// <summary>
    /// Target environments for execution.
    /// </summary>
    public IReadOnlyList<BatchEnvironment> Environments { get; set; } = [];

    /// <summary>
    /// User contexts for template substitution.
    /// </summary>
    public IReadOnlyList<BatchUserContext> Users { get; set; } = [];

    /// <summary>
    /// Request definitions to execute.
    /// </summary>
    public IReadOnlyList<BatchRequestDefinition> Requests { get; set; } = [];

    /// <summary>
    /// Number of times each expanded request should execute.
    /// </summary>
    public int Iterations { get; set; } = 1;

    /// <summary>
    /// Maximum in-flight requests.
    /// </summary>
    public int MaxConcurrency { get; set; } = 10;
}
