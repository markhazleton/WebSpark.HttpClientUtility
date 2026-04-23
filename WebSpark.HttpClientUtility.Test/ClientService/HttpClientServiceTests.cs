using System.Net;
using Microsoft.Extensions.Logging;
using Moq;
using WebSpark.HttpClientUtility.ClientService;
using WebSpark.HttpClientUtility.StringConverter;

namespace WebSpark.HttpClientUtility.Test.ClientService;

[TestClass]
public class HttpClientServiceTests
{
    private Mock<IHttpClientFactory>? _httpClientFactory;
    private Mock<IStringConverter>? _stringConverter;
    private Mock<ILogger<HttpClientService>>? _logger;

    [TestInitialize]
    public void Setup()
    {
        _httpClientFactory = new Mock<IHttpClientFactory>(MockBehavior.Strict);
        _stringConverter = new Mock<IStringConverter>(MockBehavior.Strict);
        _logger = new Mock<ILogger<HttpClientService>>();
    }

    [TestMethod]
    public async Task GetAsync_WithSuccessfulResponse_ReturnsSuccessContent()
    {
        // Arrange
        var expected = new SampleResponse { Message = "ok" };
        var handler = new DelegateHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"message\":\"ok\"}")
        }));
        var client = new HttpClient(handler);

        _httpClientFactory!
            .Setup(f => f.CreateClient("HttpClientService"))
            .Returns(client);
        _stringConverter!
            .Setup(c => c.ConvertFromString<SampleResponse>("{\"message\":\"ok\"}"))
            .Returns(expected);

        var sut = new HttpClientService(_httpClientFactory.Object, _stringConverter.Object, _logger!.Object);

        // Act
        var result = await sut.GetAsync<SampleResponse>(new Uri("https://example.test/resource"));

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        Assert.IsNotNull(result.Content);
        Assert.AreEqual("ok", result.Content.Message);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.CorrelationId));
        Assert.IsTrue(result.ElapsedMilliseconds >= 0);
    }

    [TestMethod]
    public async Task GetAsync_WithNonSuccessStatus_ReturnsFailure()
    {
        // Arrange
        var handler = new DelegateHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            ReasonPhrase = "Input invalid"
        }));
        var client = new HttpClient(handler);

        _httpClientFactory!
            .Setup(f => f.CreateClient("HttpClientService"))
            .Returns(client);

        var sut = new HttpClientService(_httpClientFactory.Object, _stringConverter!.Object, _logger!.Object);

        // Act
        var result = await sut.GetAsync<SampleResponse>(new Uri("https://example.test/resource"));

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.IsNull(result.Content);
        StringAssert.Contains(result.ErrorMessage!, "Input invalid");
        StringAssert.Contains(result.ErrorMessage!, "Ref:");
    }

    [TestMethod]
    public async Task GetAsync_WithDeserializationError_ReturnsUnprocessableEntity()
    {
        // Arrange
        var handler = new DelegateHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"message\":\"ok\"}")
        }));
        var client = new HttpClient(handler);

        _httpClientFactory!
            .Setup(f => f.CreateClient("HttpClientService"))
            .Returns(client);
        _stringConverter!
            .Setup(c => c.ConvertFromString<SampleResponse>("{\"message\":\"ok\"}"))
            .Throws(new InvalidOperationException("Cannot deserialize"));

        var sut = new HttpClientService(_httpClientFactory.Object, _stringConverter.Object, _logger!.Object);

        // Act
        var result = await sut.GetAsync<SampleResponse>(new Uri("https://example.test/resource"));

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(HttpStatusCode.UnprocessableEntity, result.StatusCode);
        StringAssert.Contains(result.ErrorMessage!, "Deserialization Error");
    }

    [TestMethod]
    public async Task GetAsync_WithTaskCanceledException_ReturnsRequestTimeout()
    {
        // Arrange
        var handler = new DelegateHandler((_, ct) => Task.FromCanceled<HttpResponseMessage>(ct));
        var client = new HttpClient(handler);
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _httpClientFactory!
            .Setup(f => f.CreateClient("HttpClientService"))
            .Returns(client);

        var sut = new HttpClientService(_httpClientFactory.Object, _stringConverter!.Object, _logger!.Object);

        // Act
        var result = await sut.GetAsync<SampleResponse>(new Uri("https://example.test/resource"), cancellationTokenSource.Token);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(HttpStatusCode.RequestTimeout, result.StatusCode);
        StringAssert.Contains(result.ErrorMessage!, "Request Timeout");
    }

    [TestMethod]
    public async Task PostAsync_WithSerializationError_ReturnsBadRequest()
    {
        // Arrange
        var handler = new DelegateHandler((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{\"message\":\"ok\"}")
        }));
        var client = new HttpClient(handler);

        _httpClientFactory!
            .Setup(f => f.CreateClient("HttpClientService"))
            .Returns(client);
        _stringConverter!
            .Setup(c => c.ConvertFromModel(It.IsAny<SamplePayload>()))
            .Throws(new ArgumentException("payload invalid"));

        var sut = new HttpClientService(_httpClientFactory.Object, _stringConverter.Object, _logger!.Object);

        // Act
        var result = await sut.PostAsync<SamplePayload, SampleResponse>(
            new Uri("https://example.test/resource"),
            new SamplePayload { Name = "payload" });

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        StringAssert.Contains(result.ErrorMessage!, "Request Preparation Error");
    }

    private sealed class DelegateHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return handler(request, cancellationToken);
        }
    }

    private sealed class SamplePayload
    {
        public string Name { get; set; } = string.Empty;
    }

    private sealed class SampleResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}
