using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace WebSpark.HttpClientUtility.Test.OpenTelemetry;

/// <summary>
/// Tests to verify OpenTelemetry integration works correctly with the installed packages.
/// </summary>
[TestClass]
public class OpenTelemetryIntegrationTests
{
    public TestContext? TestContext { get; set; }    /// <summary>
                                                     /// Test that OpenTelemetry can be configured with Console exporter.
                                                     /// </summary>
    [TestMethod]
    public void OpenTelemetry_ConsoleExporter_CanBeConfigured()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(ResourceBuilder.CreateDefault()
                    .AddService("HttpClientUtility.Test", "1.0.0"));
                options.AddConsoleExporter();
            });
        });

        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var logger = serviceProvider.GetService<ILogger<OpenTelemetryIntegrationTests>>();
        Assert.IsNotNull(logger);

        // Test logging
        logger?.LogInformation("OpenTelemetry Console exporter test message");
        TestContext?.WriteLine("Console exporter configuration successful");
    }    /// <summary>
         /// Test that OpenTelemetry tracing can be configured with multiple exporters.
         /// </summary>    [TestMethod]
    public void OpenTelemetry_TracingWithMultipleExporters_CanBeConfigured()
    {
        // Arrange & Act
        var exportedItems = new List<Activity>();
        var services = new ServiceCollection();
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService("HttpClientUtility.Test", "1.0.0"))
                    .AddSource("HttpClientUtility.Test")
                    .AddConsoleExporter()
                    .AddInMemoryExporter(exportedItems);
            });

        var serviceProvider = services.BuildServiceProvider();
        // Assert
        var tracerProvider = serviceProvider.GetService<TracerProvider>();
        Assert.IsNotNull(tracerProvider);

        // Create a test activity
        using var activitySource = new ActivitySource("HttpClientUtility.Test");
        using var activity = activitySource.StartActivity("TestActivity");
        activity?.SetTag("test.tag", "test.value");
        activity?.SetStatus(ActivityStatusCode.Ok);

        TestContext?.WriteLine("Tracing with multiple exporters configuration successful");
        TestContext?.WriteLine($"Test activity created: {activity?.DisplayName}");
    }    /// <summary>
         /// Test that OpenTelemetry can be configured with OTLP exporter.
         /// </summary>
    [TestMethod]
    public void OpenTelemetry_OtlpExporter_CanBeConfigured()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService("HttpClientUtility.Test", "1.0.0"))
                    .AddSource("HttpClientUtility.Test")
                    .AddOtlpExporter(options =>
                    {
                        // Configure for testing - use a test endpoint
                        options.Endpoint = new Uri("http://localhost:4317");
                        options.Protocol = OtlpExportProtocol.Grpc;
                    });
            });

        var serviceProvider = services.BuildServiceProvider();
        // Assert
        var tracerProvider = serviceProvider.GetService<TracerProvider>();
        Assert.IsNotNull(tracerProvider);

        TestContext?.WriteLine("OTLP exporter configuration successful");
    }    /// <summary>
         /// Test that OpenTelemetry can be configured with InMemory exporter for testing.
         /// </summary>
    [TestMethod]
    public void OpenTelemetry_InMemoryExporter_CanCollectActivities()
    {
        // Arrange
        var exportedItems = new List<Activity>();

        var services = new ServiceCollection();
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService("HttpClientUtility.Test", "1.0.0"))
                    .AddSource("HttpClientUtility.Test")
                    .AddInMemoryExporter(exportedItems);
            });

        var serviceProvider = services.BuildServiceProvider();
        var tracerProvider = serviceProvider.GetService<TracerProvider>();

        // Act
        using var activitySource = new ActivitySource("HttpClientUtility.Test");
        using (var activity = activitySource.StartActivity("TestInMemoryActivity"))
        {
            activity?.SetTag("test.operation", "in-memory-test");
            activity?.SetTag("test.component", "HttpClientUtility");
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        // Force flush to ensure activities are exported - using dispose to trigger flush
        tracerProvider?.Dispose();

        // Assert
        Assert.IsNotNull(tracerProvider);
        Assert.IsTrue(exportedItems.Count > 0, "Expected exported items to contain at least one activity");

        var testActivity = exportedItems.FirstOrDefault(a => a.DisplayName == "TestInMemoryActivity");
        Assert.IsNotNull(testActivity, "Expected to find test activity in exported items");
        Assert.AreEqual("in-memory-test", testActivity.GetTagItem("test.operation"));
        Assert.AreEqual("HttpClientUtility", testActivity.GetTagItem("test.component"));

        TestContext?.WriteLine($"InMemory exporter collected {exportedItems.Count} activities");
        TestContext?.WriteLine($"Test activity: {testActivity.DisplayName} with status: {testActivity.Status}");
    }    /// <summary>
         /// Test that HTTP instrumentation can be added to OpenTelemetry.
         /// </summary>
    [TestMethod]
    public void OpenTelemetry_HttpInstrumentation_CanBeConfigured()
    {
        // Arrange & Act
        var services = new ServiceCollection();
        services.AddHttpClient();
        services.AddOpenTelemetry()
            .WithTracing(builder =>
            {
                builder
                    .SetResourceBuilder(ResourceBuilder.CreateDefault()
                        .AddService("HttpClientUtility.Test", "1.0.0"))
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter();
            });

        var serviceProvider = services.BuildServiceProvider();
        // Assert
        var tracerProvider = serviceProvider.GetService<TracerProvider>();
        var httpClientFactory = serviceProvider.GetService<IHttpClientFactory>();

        Assert.IsNotNull(tracerProvider);
        Assert.IsNotNull(httpClientFactory);

        TestContext?.WriteLine("HTTP instrumentation configuration successful");
    }
}
