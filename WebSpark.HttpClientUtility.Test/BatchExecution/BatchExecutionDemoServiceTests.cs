using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
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
        var service = CreateService(mockBatch.Object);

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

        var service = CreateService(mockBatch.Object);
        var request = DemoStartRunRequest.CreateDefault();

        var result = await service.StartRunAsync(request);

        Assert.IsTrue(result.Accepted);
        Assert.IsNotNull(result.Response);
        Assert.AreEqual("Queued", result.Response.Status);
    }

    private static BatchExecutionDemoService CreateService(IBatchExecutionService batchExecutionService)
    {
        var services = new ServiceCollection();
        services.AddScoped(_ => batchExecutionService);
        var provider = services.BuildServiceProvider();
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        return new BatchExecutionDemoService(scopeFactory, Mock.Of<ILogger<BatchExecutionDemoService>>());
    }
}
