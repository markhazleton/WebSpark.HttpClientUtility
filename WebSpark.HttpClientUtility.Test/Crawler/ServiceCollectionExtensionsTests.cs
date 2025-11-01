using Microsoft.Extensions.DependencyInjection;
using WebSpark.HttpClientUtility.Crawler;

namespace WebSpark.HttpClientUtility.Test.Crawler;

[TestClass]
public class ServiceCollectionExtensionsTests
{
    private IServiceCollection _services;

    [TestInitialize]
    public void Setup()
    {
        _services = new ServiceCollection();
    }

    [TestMethod]
    public void AddHttpClientCrawler_BasicOverload_RegistersServices()
    {
        // Act
        var result = _services.AddHttpClientCrawler();

        // Assert
        Assert.AreSame(_services, result, "Should return the same service collection for chaining");

        var serviceProvider = _services.BuildServiceProvider();

        // Verify SignalR is registered (this will throw if not registered properly)
        var signalRServices = serviceProvider.GetServices<Microsoft.AspNetCore.SignalR.HubConnectionContext>();
        Assert.IsNotNull(signalRServices, "SignalR services should be registered");

        // Verify crawler services are registered
        var siteCrawler = serviceProvider.GetService<ISiteCrawler>();
        Assert.IsNotNull(siteCrawler, "ISiteCrawler should be registered");
        Assert.IsInstanceOfType<SiteCrawler>(siteCrawler, "Should be instance of SiteCrawler");

        var simpleCrawler = serviceProvider.GetService<SimpleSiteCrawler>();
        Assert.IsNotNull(simpleCrawler, "SimpleSiteCrawler should be registered");
    }

    [TestMethod]
    public void AddHttpClientCrawler_WithOptions_RegistersServicesAndOptions()
    {
        // Arrange
        var configureActionCalled = false;
        Action<CrawlerOptions> configureOptions = options =>
        {
            configureActionCalled = true;
            options.MaxPages = 100;
            options.MaxDepth = 5;
            options.RequestDelayMs = 500;
        };

        // Act
        var result = _services.AddHttpClientCrawler(configureOptions);

        // Assert
        Assert.AreSame(_services, result, "Should return the same service collection for chaining");
        Assert.IsTrue(configureActionCalled, "Configure action should have been called");

        var serviceProvider = _services.BuildServiceProvider();

        // Verify options are registered
        var options = serviceProvider.GetService<CrawlerOptions>();
        Assert.IsNotNull(options, "CrawlerOptions should be registered");
        Assert.AreEqual(100, options.MaxPages);
        Assert.AreEqual(5, options.MaxDepth);
        Assert.AreEqual(500, options.RequestDelayMs);

        // Verify crawler services are still registered
        var siteCrawler = serviceProvider.GetService<ISiteCrawler>();
        Assert.IsNotNull(siteCrawler, "ISiteCrawler should be registered");

        var simpleCrawler = serviceProvider.GetService<SimpleSiteCrawler>();
        Assert.IsNotNull(simpleCrawler, "SimpleSiteCrawler should be registered");
    }

    [TestMethod]
    public void AddHttpClientCrawler_WithNullConfigureOptions_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        _services.AddHttpClientCrawler(null!));
    }

    [TestMethod]
    public void AddHttpClientCrawler_CalledMultipleTimes_DoesNotCauseIssues()
    {
        // Act
        _services.AddHttpClientCrawler();
        _services.AddHttpClientCrawler(options => options.MaxPages = 50);

        // Assert - Should not throw
        var serviceProvider = _services.BuildServiceProvider();
        var siteCrawler = serviceProvider.GetService<ISiteCrawler>();
        Assert.IsNotNull(siteCrawler);
    }

    [TestMethod]
    public void AddHttpClientCrawler_WithComplexOptions_ConfiguresCorrectly()
    {
        // Arrange
        Action<CrawlerOptions> configureOptions = options =>
        {
            options.MaxPages = 1000;
            options.MaxDepth = 10;
            options.RequestDelayMs = 2000;
            options.RespectRobotsTxt = true;
            options.UserAgent = "TestCrawler/1.0";
            options.FollowExternalLinks = false;
            options.MaxConcurrentRequests = 5;
        };

        // Act
        _services.AddHttpClientCrawler(configureOptions);

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var options = serviceProvider.GetService<CrawlerOptions>();

        Assert.IsNotNull(options);
        Assert.AreEqual(1000, options.MaxPages);
        Assert.AreEqual(10, options.MaxDepth);
        Assert.AreEqual(2000, options.RequestDelayMs);
        Assert.IsTrue(options.RespectRobotsTxt);
        Assert.AreEqual("TestCrawler/1.0", options.UserAgent);
        Assert.IsFalse(options.FollowExternalLinks);
        Assert.AreEqual(5, options.MaxConcurrentRequests);
    }

    [TestMethod]
    public void AddHttpClientCrawler_WithDependencies_CanResolveAllServices()
    {
        // Arrange
        _services.AddLogging();
        _services.AddMemoryCache();
        _services.AddHttpClient();

        // Act
        _services.AddHttpClientCrawler(options =>
     {
         options.MaxPages = 100;
         options.MaxDepth = 3;
     });

        // Assert
        var serviceProvider = _services.BuildServiceProvider();

        // Verify all services can be resolved without circular dependencies
        var siteCrawler = serviceProvider.GetService<ISiteCrawler>();
        var simpleCrawler = serviceProvider.GetService<SimpleSiteCrawler>();
        var options = serviceProvider.GetService<CrawlerOptions>();

        Assert.IsNotNull(siteCrawler);
        Assert.IsNotNull(simpleCrawler);
        Assert.IsNotNull(options);
    }
}
