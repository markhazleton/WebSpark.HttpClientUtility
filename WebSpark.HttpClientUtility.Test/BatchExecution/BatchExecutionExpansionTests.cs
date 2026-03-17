using Microsoft.Extensions.Logging;
using Moq;
using WebSpark.HttpClientUtility.BatchExecution;
using WebSpark.HttpClientUtility.RequestResult;

namespace WebSpark.HttpClientUtility.Test.BatchExecution;

[TestClass]
public class BatchExecutionExpansionTests
{
    [TestMethod]
    public async Task ExecuteAsync_ExpandsCartesianProduct_AndReportsPlannedCount()
    {
        var requestService = CreateMockRequestService();
        var service = new BatchExecutionService(requestService.Object, new TemplateSubstitutionService(), Mock.Of<ILogger<BatchExecutionService>>());

        var config = new BatchExecutionConfiguration
        {
            Environments =
            [
                new BatchEnvironment { Name = "A", BaseUrl = "https://a.test" },
                new BatchEnvironment { Name = "B", BaseUrl = "https://b.test" }
            ],
            Users =
            [
                new BatchUserContext { UserId = "u1", Properties = new Dictionary<string, string>() },
                new BatchUserContext { UserId = "u2", Properties = new Dictionary<string, string>() }
            ],
            Requests =
            [
                new BatchRequestDefinition { Name = "R1", Method = "GET", PathTemplate = "/one" },
                new BatchRequestDefinition { Name = "R2", Method = "GET", PathTemplate = "/two" }
            ],
            Iterations = 2,
            MaxConcurrency = 4
        };

        var result = await service.ExecuteAsync(config);

        Assert.AreEqual(16, result.TotalPlannedCount);
        Assert.AreEqual(16, result.CompletedCount);
    }

    [TestMethod]
    public async Task ExecuteAsync_WithZeroWork_ReturnsEmptyResults()
    {
        var service = new BatchExecutionService(CreateMockRequestService().Object, new TemplateSubstitutionService(), Mock.Of<ILogger<BatchExecutionService>>());

        var result = await service.ExecuteAsync(new BatchExecutionConfiguration
        {
            Environments = [new BatchEnvironment { Name = "A", BaseUrl = "https://a.test" }],
            Users = [],
            Requests = [new BatchRequestDefinition { Name = "R", Method = "GET", PathTemplate = "/" }],
            Iterations = 1,
            MaxConcurrency = 1
        });

        Assert.AreEqual(0, result.TotalPlannedCount);
        Assert.AreEqual(0, result.Results.Count);
    }

    [TestMethod]
    public async Task ExecuteAsync_CustomMethodWithBodyCapability_AttachesBody()
    {
        HttpRequestResult<string>? captured = null;
        var mock = new Mock<IHttpRequestResultService>(MockBehavior.Strict);
        mock.Setup(x => x.HttpSendRequestResultAsync(
                It.IsAny<HttpRequestResult<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Callback<HttpRequestResult<string>, string, string, int, CancellationToken>((req, _, _, _, _) => captured = req)
            .ReturnsAsync(new HttpRequestResult<string> { StatusCode = System.Net.HttpStatusCode.OK, ResponseResults = "ok" });

        var service = new BatchExecutionService(mock.Object, new TemplateSubstitutionService(), Mock.Of<ILogger<BatchExecutionService>>());
        await service.ExecuteAsync(new BatchExecutionConfiguration
        {
            Environments = [new BatchEnvironment { Name = "A", BaseUrl = "https://a.test" }],
            Users = [new BatchUserContext { UserId = "u", Properties = new Dictionary<string, string>() }],
            Requests = [new BatchRequestDefinition { Name = "Custom", Method = "MERGE", PathTemplate = "/api", BodyTemplate = "{}", IsBodyCapable = true }],
            Iterations = 1,
            MaxConcurrency = 1
        });

        Assert.IsNotNull(captured);
        Assert.IsNotNull(captured.RequestBody);
    }

    private static Mock<IHttpRequestResultService> CreateMockRequestService()
    {
        var mock = new Mock<IHttpRequestResultService>(MockBehavior.Strict);
        mock.Setup(x => x.HttpSendRequestResultAsync(
                It.IsAny<HttpRequestResult<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpRequestResult<string>
            {
                StatusCode = System.Net.HttpStatusCode.OK,
                ResponseResults = "ok"
            });
        return mock;
    }
}
