using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using WebSpark.HttpClientUtility.Authentication;
using WebSpark.HttpClientUtility.RequestResult;

namespace WebSpark.HttpClientUtility.Test.RequestResult;

[TestClass]
public class HttpRequestResultServiceCacheTests
{
    private Mock<IHttpRequestResultService>? _inner;
    private Mock<ILogger<HttpRequestResultServiceCache>>? _logger;
    private IMemoryCache? _cache;

    [TestInitialize]
    public void Setup()
    {
        _inner = new Mock<IHttpRequestResultService>(MockBehavior.Strict);
        _logger = new Mock<ILogger<HttpRequestResultServiceCache>>();
        _cache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new MemoryCacheOptions());
    }

    [TestMethod]
    public async Task HttpSendRequestResultAsync_CacheMiss_CallsInnerAndStoresResult()
    {
        // Arrange
        var request = NewRequest("https://example.test/a", cacheMinutes: 5);
        var expected = NewResponseFrom(request, "first");

        _inner!
            .Setup(s => s.HttpSendRequestResultAsync(request, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var sut = new HttpRequestResultServiceCache(_inner.Object, _logger!.Object, _cache!);

        // Act
        var first = await sut.HttpSendRequestResultAsync(request);
        var second = await sut.HttpSendRequestResultAsync(request);

        // Assert
        Assert.AreEqual("first", first.ResponseResults);
        Assert.AreEqual("first", second.ResponseResults);
        Assert.IsTrue(second.RequestContext.ContainsKey("CacheHit"));
        _inner.Verify(s => s.HttpSendRequestResultAsync(request, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task HttpSendRequestResultAsync_WhenCacheDurationZero_BypassesCache()
    {
        // Arrange
        var request = NewRequest("https://example.test/no-cache", cacheMinutes: 0);
        var response1 = NewResponseFrom(request, "one");
        var response2 = NewResponseFrom(request, "two");

        _inner!
            .SetupSequence(s => s.HttpSendRequestResultAsync(request, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response1)
            .ReturnsAsync(response2);

        var sut = new HttpRequestResultServiceCache(_inner.Object, _logger!.Object, _cache!);

        // Act
        var first = await sut.HttpSendRequestResultAsync(request);
        var second = await sut.HttpSendRequestResultAsync(request);

        // Assert
        Assert.AreEqual("one", first.ResponseResults);
        Assert.AreEqual("two", second.ResponseResults);
        _inner.Verify(s => s.HttpSendRequestResultAsync(request, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [TestMethod]
    public async Task HttpSendRequestResultAsync_DifferentRequestMethod_UsesDifferentCacheEntries()
    {
        // Arrange
        var getRequest = NewRequest("https://example.test/same", cacheMinutes: 5);
        getRequest.RequestMethod = HttpMethod.Get;
        var postRequest = NewRequest("https://example.test/same", cacheMinutes: 5);
        postRequest.RequestMethod = HttpMethod.Post;

        _inner!
            .Setup(s => s.HttpSendRequestResultAsync(getRequest, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewResponseFrom(getRequest, "get"));
        _inner!
            .Setup(s => s.HttpSendRequestResultAsync(postRequest, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewResponseFrom(postRequest, "post"));

        var sut = new HttpRequestResultServiceCache(_inner.Object, _logger!.Object, _cache!);

        // Act
        var getResult = await sut.HttpSendRequestResultAsync(getRequest);
        var postResult = await sut.HttpSendRequestResultAsync(postRequest);

        // Assert
        Assert.AreEqual("get", getResult.ResponseResults);
        Assert.AreEqual("post", postResult.ResponseResults);
        _inner.VerifyAll();
    }

    [TestMethod]
    public async Task HttpSendRequestResultAsync_AuthorizationHeaderExcludedFromCacheKey()
    {
        // Arrange
        var req1 = NewRequest("https://example.test/key", cacheMinutes: 5);
        req1.RequestHeaders = new Dictionary<string, string> { ["Authorization"] = "Bearer A" };

        var req2 = NewRequest("https://example.test/key", cacheMinutes: 5);
        req2.RequestHeaders = new Dictionary<string, string> { ["Authorization"] = "Bearer B" };

        _inner!
            .Setup(s => s.HttpSendRequestResultAsync(req1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewResponseFrom(req1, "shared"));

        var sut = new HttpRequestResultServiceCache(_inner.Object, _logger!.Object, _cache!);

        // Act
        var first = await sut.HttpSendRequestResultAsync(req1);
        var second = await sut.HttpSendRequestResultAsync(req2);

        // Assert
        Assert.AreEqual("shared", first.ResponseResults);
        Assert.AreEqual("shared", second.ResponseResults);
        Assert.IsTrue(second.RequestContext.ContainsKey("CacheHit"));
        _inner.Verify(s => s.HttpSendRequestResultAsync(req1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task HttpSendRequestResultAsync_AuthenticationProviderAffectsCacheKey()
    {
        // Arrange
        var req1 = NewRequest("https://example.test/auth", cacheMinutes: 5);
        req1.AuthenticationProvider = new FakeAuthProvider("Provider-A");

        var req2 = NewRequest("https://example.test/auth", cacheMinutes: 5);
        req2.AuthenticationProvider = new FakeAuthProvider("Provider-B");

        _inner!
            .Setup(s => s.HttpSendRequestResultAsync(req1, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewResponseFrom(req1, "A"));
        _inner!
            .Setup(s => s.HttpSendRequestResultAsync(req2, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(NewResponseFrom(req2, "B"));

        var sut = new HttpRequestResultServiceCache(_inner.Object, _logger!.Object, _cache!);

        // Act
        var first = await sut.HttpSendRequestResultAsync(req1);
        var second = await sut.HttpSendRequestResultAsync(req2);

        // Assert
        Assert.AreEqual("A", first.ResponseResults);
        Assert.AreEqual("B", second.ResponseResults);
        _inner.VerifyAll();
    }

    private static HttpRequestResult<string> NewRequest(string path, int cacheMinutes)
    {
        return new HttpRequestResult<string>
        {
            RequestPath = path,
            RequestMethod = HttpMethod.Get,
            CacheDurationMinutes = cacheMinutes,
            RequestHeaders = new Dictionary<string, string>()
        };
    }

    private static HttpRequestResult<string> NewResponseFrom(HttpRequestResult<string> request, string value)
    {
        return new HttpRequestResult<string>(request)
        {
            ResponseResults = value,
            StatusCode = HttpStatusCode.OK,
            CompletionDate = DateTime.UtcNow
        };
    }

    private sealed class FakeAuthProvider(string providerName) : IAuthenticationProvider
    {
        public bool IsValid => true;
        public string ProviderName => providerName;
        public Task AddAuthenticationAsync(Dictionary<string, string> headers, CancellationToken ct = default) => Task.CompletedTask;
        public Task ValidateAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task RefreshAsync(CancellationToken ct = default) => Task.CompletedTask;
    }
}
