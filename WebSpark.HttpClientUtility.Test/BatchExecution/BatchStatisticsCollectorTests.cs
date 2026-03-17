using WebSpark.HttpClientUtility.BatchExecution;

namespace WebSpark.HttpClientUtility.Test.BatchExecution;

[TestClass]
public class BatchStatisticsCollectorTests
{
    [TestMethod]
    public void Snapshot_ComputesCountsPercentilesAndBreakdowns()
    {
        var collector = new BatchStatisticsCollector();

        collector.Record(new BatchExecutionItemResult { HttpMethod = "GET", EnvironmentName = "A", UserId = "u1", StatusCode = 200, IsSuccess = true, DurationMilliseconds = 50 });
        collector.Record(new BatchExecutionItemResult { HttpMethod = "GET", EnvironmentName = "A", UserId = "u1", StatusCode = 200, IsSuccess = true, DurationMilliseconds = 100 });
        collector.Record(new BatchExecutionItemResult { HttpMethod = "POST", EnvironmentName = "B", UserId = "u2", StatusCode = 500, IsSuccess = false, DurationMilliseconds = 300 });

        var snapshot = collector.Snapshot();

        Assert.AreEqual(3, snapshot.TotalCount);
        Assert.AreEqual(2, snapshot.SuccessCount);
        Assert.AreEqual(1, snapshot.FailureCount);
        Assert.AreEqual(100, snapshot.P50Milliseconds);
        Assert.AreEqual(300, snapshot.P95Milliseconds);
        Assert.AreEqual(300, snapshot.P99Milliseconds);
        Assert.AreEqual(2, snapshot.ByEnvironment["A"]);
        Assert.AreEqual(1, snapshot.ByEnvironment["B"]);
    }
}
