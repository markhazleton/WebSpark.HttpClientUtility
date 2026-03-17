using WebSpark.HttpClientUtility.BatchExecution;

namespace WebSpark.HttpClientUtility.Test.BatchExecution;

[TestClass]
public class BatchProgressTests
{
    [TestMethod]
    public void BatchProgress_CanCarryStatisticsSnapshot()
    {
        var progress = new BatchProgress
        {
            RunId = "run-1",
            CompletedCount = 2,
            TotalPlannedCount = 5,
            LastUpdatedUtc = DateTimeOffset.UtcNow,
            StatisticsSnapshot = new BatchExecutionStatistics
            {
                TotalCount = 2,
                SuccessCount = 2,
                FailureCount = 0
            }
        };

        Assert.AreEqual("run-1", progress.RunId);
        Assert.AreEqual(2, progress.StatisticsSnapshot.TotalCount);
    }

    [TestMethod]
    public void StatisticsSnapshot_WithNoItems_ReturnsZeroPercentiles()
    {
        var collector = new BatchStatisticsCollector();
        var snapshot = collector.Snapshot();

        Assert.AreEqual(0, snapshot.P50Milliseconds);
        Assert.AreEqual(0, snapshot.P95Milliseconds);
        Assert.AreEqual(0, snapshot.P99Milliseconds);
    }
}
