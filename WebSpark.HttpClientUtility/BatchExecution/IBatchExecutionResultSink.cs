namespace WebSpark.HttpClientUtility.BatchExecution;

/// <summary>
/// Receives incremental batch execution item results.
/// </summary>
public interface IBatchExecutionResultSink
{
    /// <summary>
    /// Invoked when a single batch execution item completes.
    /// </summary>
    /// <param name="result">The completed item result.</param>
    /// <param name="ct">Cancellation token.</param>
    Task OnResultAsync(BatchExecutionItemResult result, CancellationToken ct = default);
}
