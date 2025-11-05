using Microsoft.Extensions.DependencyInjection;
using WebSpark.HttpClientUtility.RequestResult;

namespace WebSpark.HttpClientUtility.Test;

[TestClass]
public class ServiceCollectionExtensionsTests
{
    [TestMethod]
    public void AddHttpClientUtility_WithDefaultOptions_RegistersServices()
    {
        // Arrange
    var services = new ServiceCollection();
    services.AddLogging();

        // Act
        services.AddHttpClientUtility();
        var serviceProvider = services.BuildServiceProvider();

    // Assert
        var httpService = serviceProvider.GetService<IHttpRequestResultService>();
        Assert.IsNotNull(httpService, "IHttpRequestResultService should be registered");
    }

    [TestMethod]
    public void AddHttpClientUtility_WithCachingEnabled_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

 // Act
        services.AddHttpClientUtility(options =>
      {
            options.EnableCaching = true;
        });
        var serviceProvider = services.BuildServiceProvider();

    // Assert
     var httpService = serviceProvider.GetService<IHttpRequestResultService>();
      Assert.IsNotNull(httpService, "IHttpRequestResultService should be registered");
        
        // Verify MemoryCache was registered
    var memoryCache = serviceProvider.GetService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
        Assert.IsNotNull(memoryCache, "IMemoryCache should be registered when caching is enabled");
    }

    [TestMethod]
    public void AddHttpClientUtility_WithResilienceEnabled_RegistersServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
  services.AddHttpClientUtility(options =>
   {
            options.EnableResilience = true;
    options.ResilienceOptions.MaxRetryAttempts = 5;
    });
   var serviceProvider = services.BuildServiceProvider();

        // Assert
  var httpService = serviceProvider.GetService<IHttpRequestResultService>();
  Assert.IsNotNull(httpService, "IHttpRequestResultService should be registered");
    }

    [TestMethod]
    public void AddHttpClientUtilityWithCaching_RegistersServices()
    {
      // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        // Act
      services.AddHttpClientUtilityWithCaching();
      var serviceProvider = services.BuildServiceProvider();

      // Assert
        var httpService = serviceProvider.GetService<IHttpRequestResultService>();
        Assert.IsNotNull(httpService, "IHttpRequestResultService should be registered");
        
        var memoryCache = serviceProvider.GetService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
        Assert.IsNotNull(memoryCache, "IMemoryCache should be registered");
    }

    [TestMethod]
    public void AddHttpClientUtilityWithResilience_RegistersServices()
    {
        // Arrange
var services = new ServiceCollection();
  services.AddLogging();

     // Act
        services.AddHttpClientUtilityWithResilience();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
  var httpService = serviceProvider.GetService<IHttpRequestResultService>();
        Assert.IsNotNull(httpService, "IHttpRequestResultService should be registered");
    }

 [TestMethod]
    public void AddHttpClientUtilityWithAllFeatures_RegistersServices()
    {
        // Arrange
    var services = new ServiceCollection();
      services.AddLogging();

        // Act
        services.AddHttpClientUtilityWithAllFeatures();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var httpService = serviceProvider.GetService<IHttpRequestResultService>();
        Assert.IsNotNull(httpService, "IHttpRequestResultService should be registered");
        
    var memoryCache = serviceProvider.GetService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
   Assert.IsNotNull(memoryCache, "IMemoryCache should be registered");
    }

  [TestMethod]
    public void AddHttpClientUtility_WithNewtonsoftJson_RegistersCorrectConverter()
    {
        // Arrange
        var services = new ServiceCollection();
      services.AddLogging();

 // Act
        services.AddHttpClientUtility(options =>
        {
            options.UseNewtonsoftJson = true;
        });
        var serviceProvider = services.BuildServiceProvider();

      // Assert
        var converter = serviceProvider.GetService<WebSpark.HttpClientUtility.StringConverter.IStringConverter>();
        Assert.IsNotNull(converter, "IStringConverter should be registered");
   Assert.IsInstanceOfType(converter, typeof(WebSpark.HttpClientUtility.StringConverter.NewtonsoftJsonStringConverter));
    }

    [TestMethod]
    public void AddHttpClientUtility_WithSystemTextJson_RegistersCorrectConverter()
    {
        // Arrange
        var services = new ServiceCollection();
      services.AddLogging();

        // Act
        services.AddHttpClientUtility(options =>
        {
            options.UseNewtonsoftJson = false; // Default
     });
        var serviceProvider = services.BuildServiceProvider();

        // Assert
   var converter = serviceProvider.GetService<WebSpark.HttpClientUtility.StringConverter.IStringConverter>();
   Assert.IsNotNull(converter, "IStringConverter should be registered");
   Assert.IsInstanceOfType(converter, typeof(WebSpark.HttpClientUtility.StringConverter.SystemJsonStringConverter));
    }
}
