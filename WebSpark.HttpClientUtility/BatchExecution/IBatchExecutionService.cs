using System.Diagnostics.CodeAnalysis;

namespace WebSpark.HttpClientUtility.BatchExecution;

/// <summary>
/// Executes expanded batch HTTP requests through the existing request pipeline.
/// </summary>
public interface IBatchExecutionService
{
    /// <summary>
    /// Executes a batch configuration and returns final aggregated results.
    /// </summary>
    /// <param name="configuration">The batch execution configuration.</param>
    /// <param name="resultSink">Optional sink to receive item results as they complete.</param>
    /// <param name="progress">Optional progress callback for incremental snapshots.</param>
    /// <param name="ct">Cancellation token for cooperative cancellation.</param>
    /// <returns>The final batch execution result.</returns>
    [RequiresUnreferencedCode("Batch execution delegates request execution to IHttpRequestResultService JSON serialization.")]
    [RequiresDynamicCode("Batch execution delegates request execution to IHttpRequestResultService which may require dynamic code.")]
    Task<BatchExecutionResult> ExecuteAsync(
        BatchExecutionConfiguration configuration,
        IBatchExecutionResultSink? resultSink = null,
        IProgress<BatchProgress>? progress = null,
        CancellationToken ct = default);
}
