using Microsoft.Extensions.Logging;
using Moq;
using WebSpark.HttpClientUtility.BatchExecution;
using WebSpark.HttpClientUtility.RequestResult;

namespace WebSpark.HttpClientUtility.Test.BatchExecution;

[TestClass]
public class BatchExecutionExecutionTests
{
    [TestMethod]
    public async Task ExecuteAsync_RespectsMaxConcurrency_AndReportsProgress()
    {
        var maxObserved = 0;
        var inFlight = 0;
        var progressCount = 0;

        var mock = new Mock<IHttpRequestResultService>(MockBehavior.Strict);
        mock.Setup(x => x.HttpSendRequestResultAsync(
                It.IsAny<HttpRequestResult<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns<HttpRequestResult<string>, string, string, int, CancellationToken>(async (req, _, _, _, _) =>
            {
                var current = Interlocked.Increment(ref inFlight);
                maxObserved = Math.Max(maxObserved, current);
                await Task.Delay(15);
                Interlocked.Decrement(ref inFlight);

                return new HttpRequestResult<string>
                {
                    CorrelationId = req.CorrelationId,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    ResponseResults = "ok"
                };
            });

        var service = new BatchExecutionService(mock.Object, new TemplateSubstitutionService(), Mock.Of<ILogger<BatchExecutionService>>());
        var progress = new Progress<BatchProgress>(_ => Interlocked.Increment(ref progressCount));

        var result = await service.ExecuteAsync(CreateConfig(20, 3), progress: progress);

        Assert.AreEqual(20, result.CompletedCount);
        Assert.IsTrue(maxObserved <= 3, $"Observed concurrency {maxObserved} should be <= 3.");
        Assert.IsTrue(progressCount > 0);
    }

    [TestMethod]
    public async Task ExecuteAsync_WhenCancelled_StopsDispatchingNewWork()
    {
        var mock = new Mock<IHttpRequestResultService>(MockBehavior.Strict);
        mock.Setup(x => x.HttpSendRequestResultAsync(
                It.IsAny<HttpRequestResult<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns<HttpRequestResult<string>, string, string, int, CancellationToken>(async (req, _, _, _, _) =>
            {
                await Task.Delay(40);
                return new HttpRequestResult<string> { CorrelationId = req.CorrelationId, StatusCode = System.Net.HttpStatusCode.OK, ResponseResults = "ok" };
            });

        var service = new BatchExecutionService(mock.Object, new TemplateSubstitutionService(), Mock.Of<ILogger<BatchExecutionService>>());
        using var cts = new CancellationTokenSource(60);

        var result = await service.ExecuteAsync(CreateConfig(30, 2), ct: cts.Token);

        Assert.IsTrue(result.WasCancelled);
        Assert.IsTrue(result.CompletedCount < result.TotalPlannedCount);
    }

    [TestMethod]
    public async Task ExecuteAsync_ContinuesWhenOneEnvironmentFails()
    {
        var mock = new Mock<IHttpRequestResultService>(MockBehavior.Strict);
        mock.Setup(x => x.HttpSendRequestResultAsync(
                It.IsAny<HttpRequestResult<string>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .Returns<HttpRequestResult<string>, string, string, int, CancellationToken>((req, _, _, _, _) =>
            {
                var environmentName = req.RequestContext.TryGetValue("BatchEnvironment", out var value)
                    ? value?.ToString()
                    : null;

                if (string.Equals(environmentName, "Broken", StringComparison.OrdinalIgnoreCase))
                {
                    throw new HttpRequestException("Environment unreachable");
                }

                return Task.FromResult(new HttpRequestResult<string>
                {
                    CorrelationId = req.CorrelationId,
                    StatusCode = System.Net.HttpStatusCode.OK,
                    ResponseResults = "ok"
                });
            });

        var service = new BatchExecutionService(mock.Object, new TemplateSubstitutionService(), Mock.Of<ILogger<BatchExecutionService>>());

        var config = new BatchExecutionConfiguration
        {
            Environments =
            [
                new BatchEnvironment { Name = "Healthy", BaseUrl = "https://healthy.test" },
                new BatchEnvironment { Name = "Broken", BaseUrl = "https://broken.test" }
            ],
            Users = [new BatchUserContext { UserId = "u1", Properties = new Dictionary<string, string>() }],
            Requests = [new BatchRequestDefinition { Name = "R1", Method = "GET", PathTemplate = "/api" }],
            Iterations = 3,
            MaxConcurrency = 2
        };

        var result = await service.ExecuteAsync(config);

        Assert.AreEqual(6, result.TotalPlannedCount);
        Assert.AreEqual(6, result.CompletedCount);
        Assert.AreEqual(3, result.Statistics.FailureCount);
        Assert.AreEqual(3, result.Statistics.SuccessCount);

        var brokenResults = result.Results.Where(x => x.EnvironmentName == "Broken").ToArray();
        Assert.AreEqual(3, brokenResults.Length);
        Assert.IsTrue(brokenResults.All(x => !x.IsSuccess));

        var healthyResults = result.Results.Where(x => x.EnvironmentName == "Healthy").ToArray();
        Assert.AreEqual(3, healthyResults.Length);
        Assert.IsTrue(healthyResults.All(x => x.IsSuccess));
    }

    private static BatchExecutionConfiguration CreateConfig(int count, int maxConcurrency)
    {
        return new BatchExecutionConfiguration
        {
            Environments = [new BatchEnvironment { Name = "A", BaseUrl = "https://a.test" }],
            Users = [new BatchUserContext { UserId = "u", Properties = new Dictionary<string, string>() }],
            Requests = Enumerable.Range(0, count)
                .Select(i => new BatchRequestDefinition { Name = $"R{i}", Method = "GET", PathTemplate = "/" + i })
                .ToArray(),
            Iterations = 1,
            MaxConcurrency = maxConcurrency
        };
    }
}
