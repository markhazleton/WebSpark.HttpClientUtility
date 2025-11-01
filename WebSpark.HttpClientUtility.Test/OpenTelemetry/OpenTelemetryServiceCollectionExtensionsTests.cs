using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using OpenTelemetry.Trace;
using WebSpark.HttpClientUtility.ClientService;
using WebSpark.HttpClientUtility.OpenTelemetry;
using WebSpark.HttpClientUtility.RequestResult;
using WebSpark.HttpClientUtility.StringConverter;

namespace WebSpark.HttpClientUtility.Test.OpenTelemetry;

[TestClass]
public class OpenTelemetryServiceCollectionExtensionsTests
{
    private IServiceCollection _services;

    [TestInitialize]
    public void Setup()
    {
        _services = new ServiceCollection();
        _services.AddLogging();

        // Add IConfiguration for HttpRequestResultService with required settings
        var configurationData = new Dictionary<string, string?>
        {
            ["CsvOutputFolder"] = Path.Combine(Path.GetTempPath(), "TestCurlCommands"),
            ["CsvFileName"] = "curl_commands"
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configurationData)
        .Build();
        _services.AddSingleton<IConfiguration>(configuration);

        // Add HttpClient for HttpRequestResultService
        _services.AddScoped<HttpClient>();

        // Add IStringConverter for HttpClientService
        _services.AddScoped<IStringConverter, SystemJsonStringConverter>();
    }

    [TestMethod]
    public void AddWebSparkOpenTelemetry_WithoutConfiguration_RegistersServices()
    {
        // Act
        var result = _services.AddWebSparkOpenTelemetry();

        // Assert
        Assert.AreSame(_services, result, "Should return the same service collection for chaining");

        var serviceProvider = _services.BuildServiceProvider();
        var tracerProvider = serviceProvider.GetService<TracerProvider>();
        Assert.IsNotNull(tracerProvider, "TracerProvider should be registered");
    }

    [TestMethod]
    public void AddWebSparkOpenTelemetry_WithConfiguration_AppliesConfiguration()
    {
        // Arrange
        var configurationCalled = false;
        Action<TracerProviderBuilder> configureTracing = builder =>
              {
                  configurationCalled = true;
                  builder.AddConsoleExporter();
              };

        // Act
        _services.AddWebSparkOpenTelemetry(configureTracing);

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var tracerProvider = serviceProvider.GetService<TracerProvider>();
        Assert.IsNotNull(tracerProvider);
        Assert.IsTrue(configurationCalled, "Configuration action should have been called");
    }

    [TestMethod]
    public void AddWebSparkHttpRequestResultServiceWithOpenTelemetry_RegistersCorrectImplementation()
    {
        // Arrange
        _services.AddWebSparkOpenTelemetry();

        // Act
        _services.AddWebSparkHttpRequestResultServiceWithOpenTelemetry();

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var httpService = serviceProvider.GetService<IHttpRequestResultService>();

        Assert.IsNotNull(httpService);
        Assert.IsInstanceOfType<OpenTelemetryHttpRequestResultService>(httpService,
 "Should be wrapped with OpenTelemetry service");
    }

    [TestMethod]
    public void AddWebSparkHttpClientServiceWithOpenTelemetry_RegistersCorrectImplementation()
    {
        // Arrange
        _services.AddHttpClient();
        _services.AddWebSparkOpenTelemetry();

        // Act
        _services.AddWebSparkHttpClientServiceWithOpenTelemetry();

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var httpService = serviceProvider.GetService<IHttpClientService>();

        Assert.IsNotNull(httpService);
        Assert.IsInstanceOfType<OpenTelemetryHttpClientService>(httpService,
              "Should be wrapped with OpenTelemetry service");
    }

    [TestMethod]
    public void AddWebSparkOpenTelemetryWithExporters_DevelopmentEnvironment_ConfiguresConsoleExporter()
    {
        // Arrange
        var mockEnvironment = new Mock<IHostEnvironment>();
        mockEnvironment.Setup(e => e.EnvironmentName).Returns("Development");

        // Act
        _services.AddWebSparkOpenTelemetryWithExporters(mockEnvironment.Object);

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var tracerProvider = serviceProvider.GetService<TracerProvider>();
        Assert.IsNotNull(tracerProvider, "TracerProvider should be registered for development");
    }

    [TestMethod]
    public void AddWebSparkOpenTelemetryWithExporters_ProductionEnvironment_ConfiguresWithoutConsole()
    {
        // Arrange
        var mockEnvironment = new Mock<IHostEnvironment>();
        mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");

        // Act
        _services.AddWebSparkOpenTelemetryWithExporters(mockEnvironment.Object);

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var tracerProvider = serviceProvider.GetService<TracerProvider>();
        Assert.IsNotNull(tracerProvider, "TracerProvider should be registered for production");
    }

    [TestMethod]
    public void AddWebSparkOpenTelemetryWithExporters_WithOtlpEndpoint_ConfiguresOtlpExporter()
    {
        // Arrange
        var mockEnvironment = new Mock<IHostEnvironment>();
        mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");
        var otlpEndpoint = "http://localhost:4317";

        // Act
        _services.AddWebSparkOpenTelemetryWithExporters(mockEnvironment.Object, otlpEndpoint);

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var tracerProvider = serviceProvider.GetService<TracerProvider>();
        Assert.IsNotNull(tracerProvider, "TracerProvider should be registered with OTLP endpoint");
    }

    [TestMethod]
    public void AddWebSparkOpenTelemetryWithExporters_TestingEnvironment_ConfiguresInMemoryExporter()
    {
        // Arrange
        var mockEnvironment = new Mock<IHostEnvironment>();
        mockEnvironment.Setup(e => e.EnvironmentName).Returns("Testing");

        // Act
        _services.AddWebSparkOpenTelemetryWithExporters(mockEnvironment.Object);

        // Assert
        var serviceProvider = _services.BuildServiceProvider();
        var tracerProvider = serviceProvider.GetService<TracerProvider>();
        Assert.IsNotNull(tracerProvider, "TracerProvider should be registered for testing");
    }

    [TestMethod]
    public void AddWebSparkOpenTelemetryWithExporters_EmptyOtlpEndpoint_DoesNotThrow()
    {
        // Arrange
        var mockEnvironment = new Mock<IHostEnvironment>();
        mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");

        // Act & Assert - Should not throw with empty endpoint
        _services.AddWebSparkOpenTelemetryWithExporters(mockEnvironment.Object, string.Empty);

        var serviceProvider = _services.BuildServiceProvider();
        var tracerProvider = serviceProvider.GetService<TracerProvider>();
        Assert.IsNotNull(tracerProvider);
    }

    [TestMethod]
    public void AddWebSparkOpenTelemetryWithExporters_NullOtlpEndpoint_DoesNotThrow()
    {
        // Arrange
        var mockEnvironment = new Mock<IHostEnvironment>();
        mockEnvironment.Setup(e => e.EnvironmentName).Returns("Production");

        // Act & Assert - Should not throw with null endpoint
        _services.AddWebSparkOpenTelemetryWithExporters(mockEnvironment.Object, null);

        var serviceProvider = _services.BuildServiceProvider();
        var tracerProvider = serviceProvider.GetService<TracerProvider>();
        Assert.IsNotNull(tracerProvider);
    }

    [TestMethod]
    public void AddWebSparkOpenTelemetry_CanChainWithOtherMethods()
    {
        // Arrange
        _services.AddHttpClient();

        // Act
        var result = _services
            .AddWebSparkOpenTelemetry()
     .AddWebSparkHttpRequestResultServiceWithOpenTelemetry()
  .AddWebSparkHttpClientServiceWithOpenTelemetry();

        // Assert
        Assert.AreSame(_services, result, "Should support method chaining");
    }

    [TestMethod]
    public void OpenTelemetryServiceExtensions_RegisteredServicesWork_WithDependencies()
    {
        // Arrange
        _services.AddHttpClient();

        // Act
        _services.AddWebSparkOpenTelemetry()
            .AddWebSparkHttpRequestResultServiceWithOpenTelemetry()
        .AddWebSparkHttpClientServiceWithOpenTelemetry();

        // Assert
        var serviceProvider = _services.BuildServiceProvider();

        // Verify all services can be resolved
        var tracerProvider = serviceProvider.GetService<TracerProvider>();
        var httpRequestService = serviceProvider.GetService<IHttpRequestResultService>();
        var httpClientService = serviceProvider.GetService<IHttpClientService>();

        Assert.IsNotNull(tracerProvider, "TracerProvider should be resolvable");
        Assert.IsNotNull(httpRequestService, "IHttpRequestResultService should be resolvable");
        Assert.IsNotNull(httpClientService, "IHttpClientService should be resolvable");

        // Verify correct wrapper types
        Assert.IsInstanceOfType<OpenTelemetryHttpRequestResultService>(httpRequestService);
        Assert.IsInstanceOfType<OpenTelemetryHttpClientService>(httpClientService);
    }
}
