using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.Reflection;
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

    [TestMethod]
    public void AddHttpClientUtility_WithSecondsBasedResilienceKeys_BindsPollyOptions()
    {
      // Arrange
      var services = new ServiceCollection();
      services.AddLogging();

      var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
          ["HttpRequestResultPollyOptions:MaxRetryAttempts"] = "4",
          ["HttpRequestResultPollyOptions:CircuitBreakerThreshold"] = "7",
          ["HttpRequestResultPollyOptions:RetryDelaySeconds"] = "2",
          ["HttpRequestResultPollyOptions:CircuitBreakerDurationSeconds"] = "45"
        })
        .Build();

      services.AddSingleton<IConfiguration>(configuration);

      // Act
      services.AddHttpClientUtility(options =>
      {
        options.EnableResilience = true;
      });

      var serviceProvider = services.BuildServiceProvider();
      var httpService = serviceProvider.GetRequiredService<IHttpRequestResultService>();
      var pollyOptions = GetPollyOptions(httpService);

      // Assert
      Assert.IsNotNull(pollyOptions, "Polly options should be available from the resilience decorator.");
      Assert.AreEqual(4, pollyOptions.MaxRetryAttempts);
      Assert.AreEqual(7, pollyOptions.CircuitBreakerThreshold);
      Assert.AreEqual(TimeSpan.FromSeconds(2), pollyOptions.RetryDelay);
      Assert.AreEqual(TimeSpan.FromSeconds(45), pollyOptions.CircuitBreakerDuration);
    }

    [TestMethod]
    public void AddHttpClientUtility_WithTimeSpanResilienceKeys_PreservesExistingBehavior()
    {
      // Arrange
      var services = new ServiceCollection();
      services.AddLogging();

      var configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
          ["HttpRequestResultPollyOptions:MaxRetryAttempts"] = "3",
          ["HttpRequestResultPollyOptions:CircuitBreakerThreshold"] = "5",
          ["HttpRequestResultPollyOptions:RetryDelay"] = "00:00:03",
          ["HttpRequestResultPollyOptions:CircuitBreakerDuration"] = "00:01:10"
        })
        .Build();

      services.AddSingleton<IConfiguration>(configuration);

      // Act
      services.AddHttpClientUtility(options =>
      {
        options.EnableResilience = true;
      });

      var serviceProvider = services.BuildServiceProvider();
      var httpService = serviceProvider.GetRequiredService<IHttpRequestResultService>();
      var pollyOptions = GetPollyOptions(httpService);

      // Assert
      Assert.IsNotNull(pollyOptions, "Polly options should be available from the resilience decorator.");
      Assert.AreEqual(3, pollyOptions.MaxRetryAttempts);
      Assert.AreEqual(5, pollyOptions.CircuitBreakerThreshold);
      Assert.AreEqual(TimeSpan.FromSeconds(3), pollyOptions.RetryDelay);
      Assert.AreEqual(TimeSpan.FromSeconds(70), pollyOptions.CircuitBreakerDuration);
    }

    private static HttpRequestResultPollyOptions? GetPollyOptions(IHttpRequestResultService rootService)
    {
      object? current = rootService;

      while (current is not null)
      {
        if (current is HttpRequestResultServicePolly)
        {
          var optionsField = current.GetType().GetField("_options", BindingFlags.Instance | BindingFlags.NonPublic);
          return optionsField?.GetValue(current) as HttpRequestResultPollyOptions;
        }

        var nextServiceField = current.GetType()
          .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
          .FirstOrDefault(f => typeof(IHttpRequestResultService).IsAssignableFrom(f.FieldType));

        current = nextServiceField?.GetValue(current);
      }

      return null;
    }
}
