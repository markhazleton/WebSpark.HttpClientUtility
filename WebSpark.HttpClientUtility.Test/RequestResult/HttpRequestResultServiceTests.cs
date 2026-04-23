using System.Net;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WebSpark.HttpClientUtility.RequestResult;

namespace WebSpark.HttpClientUtility.Test.RequestResult;

[TestClass]
public class HttpRequestResultServiceTests
{
    private Mock<ILogger<HttpRequestResultService>>? _logger;
    private IConfiguration? _configuration;

    [TestInitialize]
    public void Setup()
    {
        _logger = new Mock<ILogger<HttpRequestResultService>>();
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();
    }

    [TestMethod]
    public async Task HttpSendRequestResultAsync_WithStringResponse_ReturnsSuccess()
    {
        // Arrange
        var service = CreateService((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("hello", Encoding.UTF8, "text/plain")
        }));

        var request = new HttpRequestResult<string>
        {
            RequestPath = "https://example.test/ok",
            RequestMethod = HttpMethod.Get
        };

        // Act
        var result = await service.HttpSendRequestResultAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        Assert.AreEqual("hello", result.ResponseResults);
        Assert.IsNotNull(result.CompletionDate);
        Assert.IsTrue(result.ElapsedMilliseconds >= 0);
        Assert.IsTrue(result.RequestContext.ContainsKey("CallerMemberName"));
    }

    [TestMethod]
    public async Task HttpSendRequestResultAsync_WithInvalidPath_MapsToBadRequest()
    {
        // Arrange
        var service = CreateService((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

        var request = new HttpRequestResult<string>
        {
            RequestPath = string.Empty,
            RequestMethod = HttpMethod.Get
        };

        // Act
        var result = await service.HttpSendRequestResultAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.IsTrue(result.ErrorList.Any(e => e.Contains("Error during HTTP GET")));
    }

    [TestMethod]
    public async Task HttpSendRequestResultAsync_WithHttpRequestException_MapsStatusCode()
    {
        // Arrange
        var service = CreateService((_, _) =>
            throw new HttpRequestException("failed", null, HttpStatusCode.BadGateway));

        var request = new HttpRequestResult<string>
        {
            RequestPath = "https://example.test/fail",
            RequestMethod = HttpMethod.Get
        };

        // Act
        var result = await service.HttpSendRequestResultAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadGateway, result.StatusCode);
        Assert.IsTrue(result.ErrorList.Any(e => e.Contains("Error during HTTP GET")));
    }

    [TestMethod]
    public async Task HttpSendRequestResultAsync_WithCanceledRequest_MapsToRequestTimeout()
    {
        // Arrange
        var service = CreateService((_, ct) => Task.FromCanceled<HttpResponseMessage>(ct));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var request = new HttpRequestResult<string>
        {
            RequestPath = "https://example.test/timeout",
            RequestMethod = HttpMethod.Get
        };

        // Act
        var result = await service.HttpSendRequestResultAsync(request, ct: cts.Token);

        // Assert
        Assert.AreEqual(HttpStatusCode.RequestTimeout, result.StatusCode);
        Assert.IsTrue(result.ErrorList.Any(e => e.Contains("Error during HTTP GET")));
    }

    [TestMethod]
    public async Task HttpSendRequestResultAsync_WithInvalidJson_AddsDeserializationError()
    {
        // Arrange
        var service = CreateService((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("not-json", Encoding.UTF8, "application/json")
        }));

        var request = new HttpRequestResult<ComplexResponse>
        {
            RequestPath = "https://example.test/json",
            RequestMethod = HttpMethod.Get
        };

        // Act
        var result = await service.HttpSendRequestResultAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
        Assert.IsNull(result.ResponseResults);
        Assert.IsTrue(result.ErrorList.Any(e => e.Contains("Failed to deserialize response to expected type")));
    }

    [TestMethod]
    public void CreateHttpRequest_AddsDefaultHeadersAndCorrelationId()
    {
        // Arrange
        var service = CreateService((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        var request = new HttpRequestResult<string>
        {
            RequestPath = "https://example.test/request",
            RequestMethod = HttpMethod.Post,
            RequestBody = new StringContent("{}", Encoding.UTF8, "application/json"),
            RequestHeaders = new Dictionary<string, string>
            {
                ["X-Test"] = "value"
            }
        };

        // Act
        var httpRequest = service.CreateHttpRequest(request);

        // Assert
        Assert.AreEqual(HttpMethod.Post, httpRequest.Method);
        Assert.AreEqual("https://example.test/request", httpRequest.RequestUri!.ToString());
        Assert.IsTrue(httpRequest.Headers.Contains("X-Correlation-ID"));
        Assert.IsTrue(httpRequest.Headers.Contains("X-Test"));
        Assert.IsTrue(httpRequest.Headers.Contains("Accept"));
        Assert.IsTrue(httpRequest.Headers.Contains("Accept-Language"));
        Assert.IsNotNull(httpRequest.Content);
    }

    private HttpRequestResultService CreateService(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync)
    {
        var client = new HttpClient(new DelegateHandler(sendAsync));
        return new HttpRequestResultService(_logger!.Object, _configuration!, client);
    }

    private sealed class DelegateHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsync) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return sendAsync(request, cancellationToken);
        }
    }

    private sealed class ComplexResponse
    {
        public string Name { get; set; } = string.Empty;
    }
}
