using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Globalization;
using WebSpark.HttpClientUtility.ClientService;
using WebSpark.HttpClientUtility.BatchExecution;
using WebSpark.HttpClientUtility.RequestResult;
using WebSpark.HttpClientUtility.StringConverter;

namespace WebSpark.HttpClientUtility;

/// <summary>
/// Configuration options for HttpClientUtility services.
/// </summary>
public class HttpClientUtilityOptions
{
    /// <summary>
/// Enable response caching. Default is false.
    /// </summary>
    public bool EnableCaching { get; set; } = false;

    /// <summary>
    /// Enable Polly resilience policies (retry and circuit breaker). Default is false.
 /// </summary>
    public bool EnableResilience { get; set; } = false;

    /// <summary>
    /// Enable telemetry tracking for HTTP requests. Default is true.
  /// </summary>
    public bool EnableTelemetry { get; set; } = true;

    /// <summary>
    /// Polly options for resilience policies. Only used if EnableResilience is true.
  /// </summary>
    public HttpRequestResultPollyOptions ResilienceOptions { get; set; } = new()
    {
        MaxRetryAttempts = 3,
        RetryDelay = TimeSpan.FromSeconds(1),
      CircuitBreakerThreshold = 5,
        CircuitBreakerDuration = TimeSpan.FromSeconds(30)
    };

    /// <summary>
    /// Use Newtonsoft.Json for serialization. Default is false (uses System.Text.Json).
    /// </summary>
    public bool UseNewtonsoftJson { get; set; } = false;

    /// <summary>
    /// Enables batch execution orchestration services.
    /// </summary>
    public bool EnableBatchExecution { get; set; } = false;
}

/// <summary>
/// Extension methods for registering HttpClientUtility services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds HttpClientUtility services with default configuration.
    /// This includes basic HTTP client functionality with optional telemetry.
 /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddHttpClientUtility(this IServiceCollection services)
    {
        return services.AddHttpClientUtility(options => { });
    }

  /// <summary>
    /// Adds HttpClientUtility services with custom configuration.
    /// </summary>
    /// <param name="services">The service collection</param>
 /// <param name="configure">Configuration action for options</param>
    /// <returns>The service collection for chaining</returns>
    /// <example>
    /// <code>
    /// services.AddHttpClientUtility(options =>
    /// {
 ///     options.EnableCaching = true;
    ///     options.EnableResilience = true;
    ///     options.ResilienceOptions.MaxRetryAttempts = 5;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddHttpClientUtility(
        this IServiceCollection services,
        Action<HttpClientUtilityOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
      ArgumentNullException.ThrowIfNull(configure);

        var options = new HttpClientUtilityOptions();
                var configuration = TryGetExistingConfiguration(services);

                if (configuration is not null)
                {
                        ApplyResilienceOptionsFromConfiguration(configuration, options);
                }

    configure(options);

        // Validate configuration options before proceeding
        ValidateOptions(options);

       // Register HttpClient factory (if not already registered)
           services.AddHttpClient();

           // Register named HttpClient for WebSpark to ensure proper isolation
           // This prevents request interference when multiple requests share the same scope
           services.AddHttpClient("WebSparkHttpClient", client =>
           {
               // Set default timeout (can be overridden per request)
               client.Timeout = TimeSpan.FromSeconds(100);

               // Enable automatic decompression
               // Note: HttpClient handles cookies and redirects by default
           });

           // Register IConfiguration if not present (for test scenarios)
       services.TryAddSingleton<IConfiguration>(sp => new ConfigurationBuilder().Build());


           // Register JSON converter
         if (options.UseNewtonsoftJson)
           {
           services.TryAddSingleton<IStringConverter, NewtonsoftJsonStringConverter>();
       }
           else
           {
               services.TryAddSingleton<IStringConverter, SystemJsonStringConverter>();
         }

         // Register base services
           services.TryAddScoped<IHttpClientService, HttpClientService>();

           // Register HttpRequestResultService with factory that uses named HttpClient
           // Using named client ensures proper isolation between concurrent requests
       services.TryAddScoped<HttpRequestResultService>(sp =>
   {
               var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
   var logger = sp.GetRequiredService<ILogger<HttpRequestResultService>>();
         var configuration = sp.GetRequiredService<IConfiguration>();

               // Create a named client instance for better isolation
   var httpClient = httpClientFactory.CreateClient("WebSparkHttpClient");
     return new HttpRequestResultService(logger, configuration, httpClient);
           });

     // Register caching infrastructure if enabled
        if (options.EnableCaching)
        {
      services.AddMemoryCache();
      }

        // Build the decorator chain
        services.AddScoped<IHttpRequestResultService>(provider =>
     {
            // Start with the base service
            IHttpRequestResultService service = provider.GetRequiredService<HttpRequestResultService>();

   // Add caching decorator if enabled
     if (options.EnableCaching)
            {
           service = new HttpRequestResultServiceCache(
service,
                provider.GetRequiredService<ILogger<HttpRequestResultServiceCache>>(),
   provider.GetRequiredService<IMemoryCache>()
);
 }

        // Add resilience decorator if enabled
            if (options.EnableResilience)
            {
    service = new HttpRequestResultServicePolly(
          provider.GetRequiredService<ILogger<HttpRequestResultServicePolly>>(),
          service,
  options.ResilienceOptions
     );
            }

            // Add telemetry decorator if enabled (outermost layer)
      if (options.EnableTelemetry)
   {
      service = new HttpRequestResultServiceTelemetry(
    provider.GetRequiredService<ILogger<HttpRequestResultServiceTelemetry>>(),
              service
       );
            }

            return service;
        });

        if (options.EnableBatchExecution)
        {
            services.TryAddScoped<ITemplateSubstitutionService, TemplateSubstitutionService>();
            services.TryAddScoped<IBatchExecutionService, BatchExecutionService>();
            services.TryAddScoped<BatchStatisticsCollector>();
        }

        return services;
    }

    private static IConfiguration? TryGetExistingConfiguration(IServiceCollection services)
    {
        var descriptor = services.LastOrDefault(static d => d.ServiceType == typeof(IConfiguration));
        return descriptor?.ImplementationInstance as IConfiguration;
    }

    private static void ApplyResilienceOptionsFromConfiguration(
        IConfiguration configuration,
        HttpClientUtilityOptions options)
    {
        var section = configuration.GetSection("HttpRequestResultPollyOptions");
        if (!section.Exists())
        {
            return;
        }

        var resilience = options.ResilienceOptions;

        if (int.TryParse(section["MaxRetryAttempts"], out var maxRetryAttempts))
        {
            resilience.MaxRetryAttempts = maxRetryAttempts;
        }

        if (int.TryParse(section["CircuitBreakerThreshold"], out var circuitBreakerThreshold))
        {
            resilience.CircuitBreakerThreshold = circuitBreakerThreshold;
        }

        var retryDelay = section["RetryDelay"];
        if (!string.IsNullOrWhiteSpace(retryDelay) && TimeSpan.TryParse(retryDelay, out var parsedRetryDelay))
        {
            resilience.RetryDelay = parsedRetryDelay;
        }

        var circuitBreakerDuration = section["CircuitBreakerDuration"];
        if (!string.IsNullOrWhiteSpace(circuitBreakerDuration) && TimeSpan.TryParse(circuitBreakerDuration, out var parsedCircuitBreakerDuration))
        {
            resilience.CircuitBreakerDuration = parsedCircuitBreakerDuration;
        }

        // Legacy compatibility aliases from HttpClientDecoratorPattern.
        if (double.TryParse(
            section["RetryDelaySeconds"],
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var retryDelaySeconds))
        {
            resilience.RetryDelay = TimeSpan.FromSeconds(retryDelaySeconds);
        }

        if (double.TryParse(
            section["CircuitBreakerDurationSeconds"],
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out var circuitBreakerDurationSeconds))
        {
            resilience.CircuitBreakerDuration = TimeSpan.FromSeconds(circuitBreakerDurationSeconds);
        }
    }

    /// <summary>
 /// Adds HttpClientUtility services with caching enabled.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="cacheDurationMinutes">Default cache duration in minutes</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddHttpClientUtilityWithCaching(
   this IServiceCollection services,
      int cacheDurationMinutes = 5)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddHttpClientUtility(options =>
      {
  options.EnableCaching = true;
            options.EnableTelemetry = true;
     });
    }

    /// <summary>
    /// Adds HttpClientUtility services with resilience (Polly) enabled.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="maxRetries">Maximum number of retry attempts</param>
    /// <param name="retryDelaySeconds">Delay between retries in seconds</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddHttpClientUtilityWithResilience(
        this IServiceCollection services,
   int maxRetries = 3,
        double retryDelaySeconds = 1)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddHttpClientUtility(options =>
 {
            options.EnableResilience = true;
     options.ResilienceOptions.MaxRetryAttempts = maxRetries;
     options.ResilienceOptions.RetryDelay = TimeSpan.FromSeconds(retryDelaySeconds);
   options.EnableTelemetry = true;
});
    }

    /// <summary>
    /// Adds HttpClientUtility services with both caching and resilience enabled.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddHttpClientUtilityWithAllFeatures(
  this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.AddHttpClientUtility(options =>
  {
     options.EnableCaching = true;
            options.EnableResilience = true;
   options.EnableTelemetry = true;
 });
    }

    /// <summary>
    /// Validates the HttpClientUtilityOptions configuration.
    /// </summary>
    /// <param name="options">The options to validate</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when configuration values are invalid</exception>
    private static void ValidateOptions(HttpClientUtilityOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.EnableResilience)
        {
            var resilience = options.ResilienceOptions;
            ArgumentNullException.ThrowIfNull(resilience, nameof(options.ResilienceOptions));

            if (resilience.MaxRetryAttempts < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(resilience.MaxRetryAttempts),
                    resilience.MaxRetryAttempts,
                    "MaxRetryAttempts must be non-negative.");
            }

            if (resilience.MaxRetryAttempts > 20)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(resilience.MaxRetryAttempts),
                    resilience.MaxRetryAttempts,
                    "MaxRetryAttempts cannot exceed 20 to prevent excessive retry loops.");
            }

            if (resilience.RetryDelay < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(resilience.RetryDelay),
                    resilience.RetryDelay,
                    "RetryDelay must be non-negative.");
            }

            if (resilience.RetryDelay > TimeSpan.FromMinutes(5))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(resilience.RetryDelay),
                    resilience.RetryDelay,
                    "RetryDelay cannot exceed 5 minutes to prevent excessive wait times.");
            }

            if (resilience.CircuitBreakerThreshold <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(resilience.CircuitBreakerThreshold),
                    resilience.CircuitBreakerThreshold,
                    "CircuitBreakerThreshold must be positive.");
            }

            if (resilience.CircuitBreakerDuration < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(resilience.CircuitBreakerDuration),
                    resilience.CircuitBreakerDuration,
                    "CircuitBreakerDuration must be non-negative.");
            }

            if (resilience.CircuitBreakerDuration > TimeSpan.FromHours(1))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(resilience.CircuitBreakerDuration),
                    resilience.CircuitBreakerDuration,
                    "CircuitBreakerDuration cannot exceed 1 hour to prevent prolonged service outages.");
            }
        }
    }
}
