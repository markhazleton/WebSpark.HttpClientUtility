using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using WebSpark.HttpClientUtility.RequestResult;
using WebSpark.HttpClientUtility.StringConverter;

namespace WebSpark.HttpClientUtility.Crawler;

/// <summary>
/// Extension methods for setting up HttpClientCrawler services in an IServiceCollection
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds HttpClientCrawler services to the specified IServiceCollection
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to</param>
    /// <returns>The IServiceCollection so that additional calls can be chained</returns>
    public static IServiceCollection AddHttpClientCrawler(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register SignalR hub
        services.AddSignalR();

        // Register HttpClient factory if not already registered
        services.AddHttpClient();

        // Register logging if not already registered
        services.AddLogging();

        // Register a default IConfiguration if not already registered
        // Provide default configuration values for CurlCommandSaver
        services.TryAddSingleton<IConfiguration>(_ =>
        {
            var configData = new Dictionary<string, string?>
            {
                ["CsvOutputFolder"] = Path.Combine(Path.GetTempPath(), "curl_commands"),
                ["CsvFileName"] = "curl_commands"
            };
            return new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();
        });

        // Register default string converter if not already registered
        services.TryAddSingleton<IStringConverter, SystemJsonStringConverter>();

        // Register HttpRequestResultService if not already registered
        // Use a factory to provide HttpClient from the HttpClientFactory
        services.TryAddScoped<IHttpRequestResultService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<HttpRequestResultService>>();
            var configuration = provider.GetRequiredService<IConfiguration>();
            var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
            var httpClient = httpClientFactory.CreateClient();
            return new HttpRequestResultService(logger, configuration, httpClient);
        });

        // Register the crawlers
        services.AddTransient<ISiteCrawler, SiteCrawler>();
        services.AddTransient<SimpleSiteCrawler>();

        // Return the IServiceCollection so that additional calls can be chained
        return services;
    }

    /// <summary>
    /// Adds HttpClientCrawler services with customized options to the specified IServiceCollection
    /// </summary>
    /// <param name="services">The IServiceCollection to add services to</param>
    /// <param name="configureOptions">The action used to configure the crawler options</param>
    /// <returns>The IServiceCollection so that additional calls can be chained</returns>
    public static IServiceCollection AddHttpClientCrawler(
        this IServiceCollection services,
        Action<CrawlerOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        // Create options
        var options = new CrawlerOptions();
        configureOptions(options);

        // Register options
        services.AddSingleton(options);

        // Add the services
        return AddHttpClientCrawler(services);
    }
}
