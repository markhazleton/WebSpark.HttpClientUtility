using Microsoft.Extensions.Logging;
using Moq;
using WebSpark.HttpClientUtility.BatchExecution;
using WebSpark.HttpClientUtility.Web.Models;
using WebSpark.HttpClientUtility.Web.Services;

namespace WebSpark.HttpClientUtility.Test.BatchExecution;

[TestClass]
public class BatchExecutionDemoServiceTests
{
    [TestMethod]
    public async Task StartRunAsync_WhenPlannedCountExceedsCap_ReturnsError()
    {
        var mockBatch = new Mock<IBatchExecutionService>(MockBehavior.Strict);
        var service = new BatchExecutionDemoService(mockBatch.Object, Mock.Of<ILogger<BatchExecutionDemoService>>());

        var request = DemoStartRunRequest.CreateDefault();
        request.Iterations = 30;

        var result = await service.StartRunAsync(request);

        Assert.IsFalse(result.Accepted);
        Assert.IsNotNull(result.Error);
    }

    [TestMethod]
    public async Task StartRunAsync_AcceptsValidRequest_AndReturnsQueuedStatus()
    {
        var mockBatch = new Mock<IBatchExecutionService>(MockBehavior.Strict);
        mockBatch.Setup(x => x.ExecuteAsync(
                It.IsAny<BatchExecutionConfiguration>(),
                It.IsAny<IBatchExecutionResultSink?>(),
                It.IsAny<IProgress<BatchProgress>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BatchExecutionResult
            {
                RunId = "run-1",
                TotalPlannedCount = 2,
                CompletedCount = 2,
                Statistics = new BatchExecutionStatistics { TotalCount = 2, SuccessCount = 2 }
            });

        var service = new BatchExecutionDemoService(mockBatch.Object, Mock.Of<ILogger<BatchExecutionDemoService>>());
        var request = DemoStartRunRequest.CreateDefault();

        var result = await service.StartRunAsync(request);

        Assert.IsTrue(result.Accepted);
        Assert.IsNotNull(result.Response);
        Assert.AreEqual("Queued", result.Response.Status);
    }
}
