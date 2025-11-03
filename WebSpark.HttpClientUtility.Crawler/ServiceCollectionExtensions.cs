using Microsoft.Extensions.DependencyInjection;

namespace WebSpark.HttpClientUtility.Crawler;

/// <summary>
/// Extension methods for configuring web crawler services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds web crawler services to the service collection.
    /// Requires AddHttpClientUtility() to be called first.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHttpClientCrawler(this IServiceCollection services)
    {
        // TODO: Add crawler service registrations
        // - ISiteCrawler
        // - SiteCrawler
        // - SimpleSiteCrawler
        // - CrawlerPerformanceTracker
        // - SignalR hub registration
        
        return services;
    }

    /// <summary>
    /// Adds web crawler services with configuration options.
    /// Requires AddHttpClientUtility() to be called first.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure crawler options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHttpClientCrawler(
        this IServiceCollection services,
        Action<CrawlerOptions> configureOptions)
    {
        // TODO: Add crawler service registrations with options
        
        return services;
    }
}
