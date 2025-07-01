using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using WebSpark.HttpClientUtility.ClientService;
using WebSpark.HttpClientUtility.RequestResult;

namespace WebSpark.HttpClientUtility.OpenTelemetry;

/// <summary>
/// Extension methods for configuring OpenTelemetry with WebSpark.HttpClientUtility services.
/// </summary>
public static class OpenTelemetryServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenTelemetry instrumentation for WebSpark.HttpClientUtility services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configureTracing">Optional action to configure additional tracing options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWebSparkOpenTelemetry(
        this IServiceCollection services,
        Action<TracerProviderBuilder>? configureTracing = null)
    {
        // Add OpenTelemetry services
        services.AddOpenTelemetry()
            .ConfigureResource(resourceBuilder =>
            {
                resourceBuilder
                    .AddService("WebSpark.HttpClientUtility", "1.0.9")
                    .AddAttributes(new[]
                    {
                        new KeyValuePair<string, object>("library.name", "WebSpark.HttpClientUtility"),
                        new KeyValuePair<string, object>("library.version", "1.0.9")
                    });
            }).WithTracing(tracerBuilder =>
            {
                tracerBuilder
                    .AddSource(OpenTelemetryHttpRequestResultService.ActivitySourceName)
                    .AddSource(OpenTelemetryHttpClientService.ActivitySourceName)
                    .AddHttpClientInstrumentation(options =>
                    {
                        // Configure HTTP client instrumentation
                        options.RecordException = true;
                        options.FilterHttpRequestMessage = (request) =>
                        {
                            // Filter out health checks and other noise
                            var uri = request.RequestUri?.ToString() ?? string.Empty;
                            return !uri.Contains("/health") && !uri.Contains("/ping");
                        };
                        options.EnrichWithHttpRequestMessage = (activity, request) =>
                        {
                            // Add correlation ID from headers if available
                            if (request.Headers.TryGetValues("X-Correlation-ID", out var correlationIds))
                            {
                                activity.SetTag("correlation.id", correlationIds.FirstOrDefault());
                            }
                        };
                        options.EnrichWithHttpResponseMessage = (activity, response) =>
                        {
                            // Add response size information
                            if (response.Content.Headers.ContentLength.HasValue)
                            {
                                activity.SetTag("http.response.size", response.Content.Headers.ContentLength.Value);
                            }
                        };
                    });

                // Allow additional configuration
                configureTracing?.Invoke(tracerBuilder);
            });

        return services;
    }

    /// <summary>
    /// Adds the OpenTelemetry-instrumented HTTP request result service to the service collection.
    /// This replaces the custom telemetry wrapper with standardized OpenTelemetry instrumentation.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWebSparkHttpRequestResultServiceWithOpenTelemetry(
        this IServiceCollection services)
    {
        // Register the base service
        services.AddScoped<HttpRequestResultService>();

        // Register the OpenTelemetry wrapper as the main interface
        services.AddScoped<IHttpRequestResultService>(provider =>
        {
            var baseService = provider.GetRequiredService<HttpRequestResultService>();
            var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<OpenTelemetryHttpRequestResultService>>();

            return new OpenTelemetryHttpRequestResultService(baseService, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds the OpenTelemetry-instrumented HTTP client service to the service collection.
    /// This replaces the custom telemetry wrapper with standardized OpenTelemetry instrumentation.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWebSparkHttpClientServiceWithOpenTelemetry(
        this IServiceCollection services)
    {
        // Register the base service
        services.AddScoped<HttpClientService>();

        // Register the OpenTelemetry wrapper as the main interface
        services.AddScoped<IHttpClientService>(provider =>
        {
            var baseService = provider.GetRequiredService<HttpClientService>();
            var logger = provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<OpenTelemetryHttpClientService>>();

            return new OpenTelemetryHttpClientService(baseService, logger);
        });

        return services;
    }

    /// <summary>
    /// Configures OpenTelemetry with common exporters for development and production scenarios.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="environment">The hosting environment for conditional configuration.</param>
    /// <param name="otlpEndpoint">Optional OTLP endpoint for OpenTelemetry Protocol export (supports Jaeger, Zipkin, and other OTLP-compatible systems).</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddWebSparkOpenTelemetryWithExporters(
        this IServiceCollection services,
        IHostEnvironment environment,
        string? otlpEndpoint = null)
    {
        services.AddWebSparkOpenTelemetry(tracerBuilder =>
        {
            if (environment.IsDevelopment())
            {
                // In development, use console exporter for easy debugging
                tracerBuilder.AddConsoleExporter();
            }

            // Add OTLP exporter if endpoint is provided
            // OTLP supports multiple backends including Jaeger, Zipkin, and others
            if (!string.IsNullOrEmpty(otlpEndpoint))
            {
                tracerBuilder.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                    // Additional configuration can be added here for headers, timeout, etc.
                });
            }

            // Always add in-memory exporter for testing scenarios
            if (environment.IsDevelopment() || environment.EnvironmentName == "Testing")
            {
                tracerBuilder.AddInMemoryExporter(new List<System.Diagnostics.Activity>());
            }
        });

        return services;
    }
}
