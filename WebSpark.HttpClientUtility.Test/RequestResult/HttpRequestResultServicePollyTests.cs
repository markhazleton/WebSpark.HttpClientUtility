using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using WebSpark.HttpClientUtility.RequestResult;

namespace WebSpark.HttpClientUtility.Test.RequestResult;

[TestClass]
public class HttpRequestResultServicePollyTests
{
    private Mock<IHttpRequestResultService>? _inner;
    private Mock<ILogger<HttpRequestResultServicePolly>>? _logger;

    [TestInitialize]
    public void Setup()
    {
        _inner = new Mock<IHttpRequestResultService>(MockBehavior.Strict);
        _logger = new Mock<ILogger<HttpRequestResultServicePolly>>();
    }

    [TestMethod]
    public async Task HttpSendRequestResultAsync_TransientHttpException_RetriesThenSucceeds()
    {
        // Arrange
        var options = NewOptions(maxRetries: 2, threshold: 5);
        var request = NewRequest("https://example.test/retry");

        _inner!
            .SetupSequence(s => s.HttpSendRequestResultAsync(request, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("transient", null, HttpStatusCode.ServiceUnavailable))
            .ReturnsAsync(NewResponseFrom(request, HttpStatusCode.OK));

        var sut = new HttpRequestResultServicePolly(_logger!.Object, _inner.Object, options);

        // Act
        var result = await sut.HttpSendRequestResultAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        Assert.IsTrue(result.RequestContext.ContainsKey("ResilienceLayerDurationMs"));
        Assert.AreEqual(options.MaxRetryAttempts, result.RequestContext["MaxRetries"]);
        Assert.IsTrue(result.ErrorList.Any(e => e.Contains("Retry 1/2")));
        _inner.Verify(s => s.HttpSendRequestResultAsync(request, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [TestMethod]
    public async Task HttpSendRequestResultAsync_NonTransientHttpException_DoesNotRetry()
    {
        // Arrange
        var options = NewOptions(maxRetries: 3, threshold: 5);
        var request = NewRequest("https://example.test/non-retry");

        _inner!
            .Setup(s => s.HttpSendRequestResultAsync(request, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("bad request", null, HttpStatusCode.BadRequest));

        var sut = new HttpRequestResultServicePolly(_logger!.Object, _inner.Object, options);

        // Act
        var result = await sut.HttpSendRequestResultAsync(request);

        // Assert
        Assert.IsTrue(result.ErrorList.Any(e => e.Contains("Resilience policy error")));
        _inner.Verify(s => s.HttpSendRequestResultAsync(request, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task HttpSendRequestResultAsync_RepeatedTransientFailures_OpensCircuit()
    {
        // Arrange
        var options = NewOptions(maxRetries: 1, threshold: 2, breakDurationMs: 500);
        var request1 = NewRequest("https://example.test/circuit-1");
        var request2 = NewRequest("https://example.test/circuit-2");
        var request3 = NewRequest("https://example.test/circuit-3");

        _inner!
            .Setup(s => s.HttpSendRequestResultAsync(It.IsAny<HttpRequestResult<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("server down", null, HttpStatusCode.ServiceUnavailable));

        var sut = new HttpRequestResultServicePolly(_logger!.Object, _inner.Object, options);

        // Act
        var result1 = await sut.HttpSendRequestResultAsync(request1);
        var result2 = await sut.HttpSendRequestResultAsync(request2);
        var result3 = await sut.HttpSendRequestResultAsync(request3);

        // Assert
        Assert.IsTrue(result1.ErrorList.Count > 0);
        Assert.IsTrue(result2.ErrorList.Count > 0);
        Assert.IsTrue(result3.ErrorList.Count > 0);
        Assert.AreEqual("Open", result3.RequestContext["FinalCircuitState"]);
    }

    [TestMethod]
    public async Task HttpSendRequestResultAsync_AfterBreakDuration_AllowsExecutionAgain()
    {
        // Arrange
        var options = NewOptions(maxRetries: 0, threshold: 2, breakDurationMs: 200);
        var requestA = NewRequest("https://example.test/reset-a");
        var requestB = NewRequest("https://example.test/reset-b");
        var requestC = NewRequest("https://example.test/reset-c");

        _inner!
            .SetupSequence(s => s.HttpSendRequestResultAsync(It.IsAny<HttpRequestResult<string>>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("down", null, HttpStatusCode.ServiceUnavailable))
            .ThrowsAsync(new HttpRequestException("down", null, HttpStatusCode.ServiceUnavailable))
            .ReturnsAsync(NewResponseFrom(requestC, HttpStatusCode.OK));

        var sut = new HttpRequestResultServicePolly(_logger!.Object, _inner.Object, options);

        // Act
        await sut.HttpSendRequestResultAsync(requestA);
        await sut.HttpSendRequestResultAsync(requestB);
        await Task.Delay(350);
        var result = await sut.HttpSendRequestResultAsync(requestC);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        Assert.AreNotEqual("Open", result.RequestContext["FinalCircuitState"]);
    }

    private static HttpRequestResultPollyOptions NewOptions(int maxRetries, int threshold, int breakDurationMs = 250)
    {
        return new HttpRequestResultPollyOptions
        {
            MaxRetryAttempts = maxRetries,
            RetryDelay = TimeSpan.FromMilliseconds(10),
            CircuitBreakerThreshold = threshold,
            CircuitBreakerDuration = TimeSpan.FromMilliseconds(breakDurationMs)
        };
    }

    private static HttpRequestResult<string> NewRequest(string path)
    {
        return new HttpRequestResult<string>
        {
            RequestPath = path,
            RequestMethod = HttpMethod.Get,
            CacheDurationMinutes = 0
        };
    }

    private static HttpRequestResult<string> NewResponseFrom(HttpRequestResult<string> request, HttpStatusCode code)
    {
        return new HttpRequestResult<string>(request)
        {
            StatusCode = code,
            ResponseResults = "ok",
            CompletionDate = DateTime.UtcNow
        };
    }
}
