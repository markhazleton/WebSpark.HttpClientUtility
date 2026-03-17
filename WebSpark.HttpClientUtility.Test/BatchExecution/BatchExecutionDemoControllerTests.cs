using Microsoft.AspNetCore.Mvc;
using Moq;
using WebSpark.HttpClientUtility.BatchExecution;
using WebSpark.HttpClientUtility.Web.Controllers;
using WebSpark.HttpClientUtility.Web.Models;
using WebSpark.HttpClientUtility.Web.Services;

namespace WebSpark.HttpClientUtility.Test.BatchExecution;

[TestClass]
public class BatchExecutionDemoControllerTests
{
    [TestMethod]
    public void Index_ReturnsView()
    {
        var controller = new BatchExecutionController(CreateService());

        var result = controller.Index();

        Assert.IsInstanceOfType<ViewResult>(result);
    }

    [TestMethod]
    public async Task StartRun_ReturnsAccepted_ForValidRequest()
    {
        var controller = new BatchExecutionController(CreateService());

        var actionResult = await controller.StartRun(DemoStartRunRequest.CreateDefault(), CancellationToken.None);

        Assert.IsInstanceOfType<AcceptedResult>(actionResult);
    }

    [TestMethod]
    public void GetRunStatus_ReturnsNotFound_WhenRunMissing()
    {
        var controller = new BatchExecutionController(CreateService());

        var result = controller.GetRunStatus("missing-run");

        Assert.IsInstanceOfType<NotFoundObjectResult>(result);
    }

    private static BatchExecutionDemoService CreateService()
    {
        var mockBatch = new Mock<IBatchExecutionService>(MockBehavior.Strict);
        mockBatch.Setup(x => x.ExecuteAsync(
                It.IsAny<BatchExecutionConfiguration>(),
                It.IsAny<IBatchExecutionResultSink?>(),
                It.IsAny<IProgress<BatchProgress>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((BatchExecutionConfiguration config, IBatchExecutionResultSink? _, IProgress<BatchProgress>? _, CancellationToken _) =>
                new BatchExecutionResult
                {
                    RunId = config.RunId ?? "run-1",
                    TotalPlannedCount = 0,
                    CompletedCount = 0,
                    Statistics = new BatchExecutionStatistics()
                });

        return new BatchExecutionDemoService(mockBatch.Object, Mock.Of<Microsoft.Extensions.Logging.ILogger<BatchExecutionDemoService>>());
    }
}
