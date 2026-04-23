using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebSpark.HttpClientUtility.Crawler;
using WebSpark.HttpClientUtility.RequestResult;
using WebSpark.HttpClientUtility.StringConverter;

namespace WebSpark.HttpClientUtility.Test.Crawler;

[TestClass]
public class CrawlerServiceCollectionExtensionsTests
{
    [TestMethod]
    public void AddHttpClientCrawler_RegistersCoreCrawlerServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddHttpClientCrawler();
        using var provider = services.BuildServiceProvider();

        // Assert
        Assert.IsNotNull(provider.GetService<IHttpClientFactory>());
        Assert.IsNotNull(provider.GetService<IConfiguration>());
        Assert.IsNotNull(provider.GetService<IStringConverter>());
        Assert.IsNotNull(provider.GetService<IHttpRequestResultService>());
        Assert.IsNotNull(provider.GetService<ISiteCrawler>());
        Assert.IsNotNull(provider.GetService<SimpleSiteCrawler>());
    }

    [TestMethod]
    public void AddHttpClientCrawler_WithOptions_RegistersConfiguredOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
        services.AddHttpClientCrawler(options =>
        {
            options.RespectRobotsTxt = false;
            options.MaxDepth = 5;
        });

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<CrawlerOptions>();

        // Assert
        Assert.IsFalse(options.RespectRobotsTxt);
        Assert.AreEqual(5, options.MaxDepth);
        Assert.IsNotNull(provider.GetService<ISiteCrawler>());
    }

    [TestMethod]
    public void AddHttpClientCrawler_WithNullConfigureOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Action<CrawlerOptions>? configure = null;
        Assert.ThrowsExactly<ArgumentNullException>(() =>
            services.AddHttpClientCrawler(configure!));
    }

    [TestMethod]
    public void AddHttpClientCrawler_RemovesExistingIHttpRequestResultServiceRegistration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<IHttpRequestResultService, FakeHttpRequestResultService>();

        // Act
        services.AddHttpClientCrawler();
        using var provider = services.BuildServiceProvider();
        var registered = provider.GetRequiredService<IHttpRequestResultService>();

        // Assert
        Assert.IsFalse(registered is FakeHttpRequestResultService);
    }

    private sealed class FakeHttpRequestResultService : IHttpRequestResultService
    {
        public Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(
            HttpRequestResult<T> httpSendResults,
            string memberName = "",
            string filePath = "",
            int lineNumber = 0,
            CancellationToken ct = default)
        {
            return Task.FromResult(httpSendResults);
        }
    }
}
