using System.Net;
using Moq;
using WebSpark.HttpClientUtility.ClientService;

namespace WebSpark.HttpClientUtility.Test.ClientService;

[TestClass]
public class HttpGetCallServiceTelemetryTests
{
    private Mock<IHttpClientService>? _innerService;
    private HttpClientServiceTelemetry? _sut;

    [TestInitialize]
    public void Setup()
    {
        _innerService = new Mock<IHttpClientService>(MockBehavior.Strict);
        _sut = new HttpClientServiceTelemetry(_innerService.Object);
    }

    [TestMethod]
    public async Task GetAsync_WhenInnerServiceSucceeds_PreservesSuccessAndSetsTelemetryValues()
    {
        // Arrange
        var expected = HttpResponseContent<string>.Success("data", HttpStatusCode.OK);
        _innerService!
            .Setup(s => s.GetAsync<string>(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut!.GetAsync<string>(new Uri("https://example.test"), CancellationToken.None);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("data", result.Content);
        Assert.IsNotNull(result.CompletionDate);
        Assert.IsTrue(result.ElapsedMilliseconds >= 0);
    }

    [TestMethod]
    public async Task GetAsync_WhenInnerServiceThrows_ReturnsServiceUnavailableFailure()
    {
        // Arrange
        _innerService!
            .Setup(s => s.GetAsync<string>(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("network error"));

        // Act
        var result = await _sut!.GetAsync<string>(new Uri("https://example.test"), CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, result.StatusCode);
        StringAssert.Contains(result.ErrorMessage!, "HTTP Request Exception");
        Assert.IsNotNull(result.CompletionDate);
    }

    [TestMethod]
    public async Task PostWithHeaders_WhenInnerServiceThrows_ReturnsFailureWithTelemetry()
    {
        // Arrange
        _innerService!
            .Setup(s => s.PostAsync<object, string>(It.IsAny<Uri>(), It.IsAny<object>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        // Act
        var result = await _sut!.PostAsync<object, string>(
            new Uri("https://example.test"),
            new { Name = "payload" },
            new Dictionary<string, string> { ["X-Test"] = "1" },
            CancellationToken.None);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, result.StatusCode);
        Assert.IsNotNull(result.CompletionDate);
        Assert.IsTrue(result.ElapsedMilliseconds >= 0);
    }

    [TestMethod]
    public async Task DeleteAsync_WhenInnerServiceSucceeds_SetsCompletionDate()
    {
        // Arrange
        _innerService!
            .Setup(s => s.DeleteAsync<string>(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(HttpResponseContent<string>.Success("deleted", HttpStatusCode.OK));

        // Act
        var result = await _sut!.DeleteAsync<string>(new Uri("https://example.test/resource"));

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("deleted", result.Content);
        Assert.IsNotNull(result.CompletionDate);
    }
}
