using System.Collections.Concurrent;

namespace WebSpark.HttpClientUtility.BatchExecution;

/// <summary>
/// Thread-safe statistics collector for batch execution item results.
/// </summary>
public sealed class BatchStatisticsCollector
{
    private readonly ConcurrentBag<long> _durations = new();
    private readonly ConcurrentDictionary<string, int> _byMethod = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, int> _byEnvironment = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, int> _byUser = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<int, int> _byStatusCode = new();

    private int _totalCount;
    private int _successCount;
    private int _failureCount;

    /// <summary>
    /// Records one item result into aggregate counters.
    /// </summary>
    /// <param name="item">The item result to record.</param>
    public void Record(BatchExecutionItemResult item)
    {
        ArgumentNullException.ThrowIfNull(item);

        Interlocked.Increment(ref _totalCount);
        if (item.IsSuccess)
        {
            Interlocked.Increment(ref _successCount);
        }
        else
        {
            Interlocked.Increment(ref _failureCount);
        }

        _durations.Add(item.DurationMilliseconds);

        _byMethod.AddOrUpdate(item.HttpMethod, 1, static (_, current) => current + 1);
        _byEnvironment.AddOrUpdate(item.EnvironmentName, 1, static (_, current) => current + 1);

        var userKey = string.IsNullOrWhiteSpace(item.UserId) ? "(none)" : item.UserId;
        _byUser.AddOrUpdate(userKey, 1, static (_, current) => current + 1);

        if (item.StatusCode.HasValue)
        {
            _byStatusCode.AddOrUpdate(item.StatusCode.Value, 1, static (_, current) => current + 1);
        }
    }

    /// <summary>
    /// Returns a point-in-time immutable statistics snapshot.
    /// </summary>
    /// <returns>Statistics snapshot.</returns>
    public BatchExecutionStatistics Snapshot()
    {
        var durations = _durations.ToArray();
        Array.Sort(durations);

        return new BatchExecutionStatistics
        {
            TotalCount = _totalCount,
            SuccessCount = _successCount,
            FailureCount = _failureCount,
            P50Milliseconds = Percentile(durations, 0.50),
            P95Milliseconds = Percentile(durations, 0.95),
            P99Milliseconds = Percentile(durations, 0.99),
            ByMethod = new Dictionary<string, int>(_byMethod, StringComparer.OrdinalIgnoreCase),
            ByEnvironment = new Dictionary<string, int>(_byEnvironment, StringComparer.OrdinalIgnoreCase),
            ByUser = new Dictionary<string, int>(_byUser, StringComparer.Ordinal),
            ByStatusCode = new Dictionary<int, int>(_byStatusCode)
        };
    }

    private static double Percentile(long[] sortedDurations, double percentile)
    {
        if (sortedDurations.Length == 0)
        {
            return 0;
        }

        var index = (int)Math.Ceiling(percentile * sortedDurations.Length) - 1;
        index = Math.Clamp(index, 0, sortedDurations.Length - 1);
        return sortedDurations[index];
    }
}
