using Microsoft.Extensions.Logging;
using Moq;
using WebSpark.HttpClientUtility.BatchExecution;
using WebSpark.HttpClientUtility.RequestResult;

namespace WebSpark.HttpClientUtility.Test.BatchExecution;

[TestClass]
public class BatchExecutionStreamingTests
{
    [TestMethod]
    public async Task ExecuteAsync_StreamsResultsToSink_ForSuccessAndFailureItems()
    {
        var sinkItems = new List<BatchExecutionItemResult>();
        var sink = new CollectingSink(sinkItems);

        var mock = new Mock<IHttpRequestResultService>(MockBehavior.Strict);
        mock.Setup(x => x.HttpSendRequestResultAsync(
                It.IsAny<HttpRequestResult<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns<HttpRequestResult<string>, string, string, int, CancellationToken>((req, _, _, _, _) =>
            {
                if (req.RequestPath.Contains("fail", StringComparison.OrdinalIgnoreCase))
                {
                    return Task.FromResult(new HttpRequestResult<string>
                    {
                        CorrelationId = req.CorrelationId,
                        StatusCode = System.Net.HttpStatusCode.InternalServerError,
                        ResponseResults = "error"
                    });
                }

                return Task.FromResult(new HttpRequestResult<string>
                {
                    CorrelationId = req.CorrelationId,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    ResponseResults = "ok"
                });
            });

        var service = new BatchExecutionService(mock.Object, new TemplateSubstitutionService(), Mock.Of<ILogger<BatchExecutionService>>());
        var result = await service.ExecuteAsync(new BatchExecutionConfiguration
        {
            Environments = [new BatchEnvironment { Name = "A", BaseUrl = "https://a.test" }],
            Users = [new BatchUserContext { UserId = "u1", Properties = new Dictionary<string, string>() }],
            Requests =
            [
                new BatchRequestDefinition { Name = "ok", Method = "GET", PathTemplate = "/ok" },
                new BatchRequestDefinition { Name = "fail", Method = "GET", PathTemplate = "/fail" }
            ],
            Iterations = 1,
            MaxConcurrency = 1
        }, resultSink: sink);

        Assert.AreEqual(2, sinkItems.Count);
        Assert.AreEqual(result.CompletedCount, sinkItems.Count);
        Assert.IsTrue(sinkItems.All(x => !string.IsNullOrWhiteSpace(x.EnvironmentName)));
        Assert.IsTrue(sinkItems.All(x => !string.IsNullOrWhiteSpace(x.RequestPath)));
        Assert.IsTrue(sinkItems.All(x => !string.IsNullOrWhiteSpace(x.HttpMethod)));
        Assert.IsTrue(sinkItems.All(x => !string.IsNullOrWhiteSpace(x.CorrelationId)));
        Assert.IsTrue(sinkItems.All(x => x.DurationMilliseconds >= 0));
    }

    private sealed class CollectingSink(List<BatchExecutionItemResult> sinkItems) : IBatchExecutionResultSink
    {
        public Task OnResultAsync(BatchExecutionItemResult result, CancellationToken ct = default)
        {
            lock (sinkItems)
            {
                sinkItems.Add(result);
            }

            return Task.CompletedTask;
        }
    }
}
