using Microsoft.Extensions.Logging;
using Moq;
using WebSpark.HttpClientUtility.BatchExecution;
using WebSpark.HttpClientUtility.RequestResult;

namespace WebSpark.HttpClientUtility.Test.BatchExecution;

[TestClass]
public class BatchExecutionHashingTests
{
    [TestMethod]
    public async Task ExecuteAsync_ProducesDeterministicHash_ForIdenticalResponseBodies()
    {
        var mock = new Mock<IHttpRequestResultService>(MockBehavior.Strict);
        mock.Setup(x => x.HttpSendRequestResultAsync(
                It.IsAny<HttpRequestResult<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((HttpRequestResult<string> req, string _, string _, int _, CancellationToken _) =>
                new HttpRequestResult<string> { CorrelationId = req.CorrelationId, StatusCode = System.Net.HttpStatusCode.OK, ResponseResults = "same" });

        var service = new BatchExecutionService(mock.Object, new TemplateSubstitutionService(), Mock.Of<ILogger<BatchExecutionService>>());
        var result = await service.ExecuteAsync(CreateConfig());

        Assert.AreEqual(2, result.Results.Count);
        Assert.AreEqual(result.Results[0].ResponseBodyHash, result.Results[1].ResponseBodyHash);
    }

    [TestMethod]
    public async Task ExecuteAsync_ProducesDifferentHashes_ForDifferentResponseBodies()
    {
        var calls = 0;
        var mock = new Mock<IHttpRequestResultService>(MockBehavior.Strict);
        mock.Setup(x => x.HttpSendRequestResultAsync(
                It.IsAny<HttpRequestResult<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((HttpRequestResult<string> req, string _, string _, int _, CancellationToken _) =>
            {
                calls++;
                return new HttpRequestResult<string>
                {
                    CorrelationId = req.CorrelationId,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    ResponseResults = calls == 1 ? "one" : "two"
                };
            });

        var service = new BatchExecutionService(mock.Object, new TemplateSubstitutionService(), Mock.Of<ILogger<BatchExecutionService>>());
        var result = await service.ExecuteAsync(CreateConfig());

        Assert.AreNotEqual(result.Results[0].ResponseBodyHash, result.Results[1].ResponseBodyHash);
    }

    private static BatchExecutionConfiguration CreateConfig()
    {
        return new BatchExecutionConfiguration
        {
            Environments = [new BatchEnvironment { Name = "A", BaseUrl = "https://a.test" }],
            Users = [new BatchUserContext { UserId = "u1", Properties = new Dictionary<string, string>() }],
            Requests =
            [
                new BatchRequestDefinition { Name = "R1", Method = "GET", PathTemplate = "/one" },
                new BatchRequestDefinition { Name = "R2", Method = "GET", PathTemplate = "/two" }
            ],
            Iterations = 1,
            MaxConcurrency = 1
        };
    }
}
