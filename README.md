# WebSpark.HttpClientUtility

[![NuGet Version](https://img.shields.io/nuget/v/WebSpark.HttpClientUtility.svg)](https://www.nuget.org/packages/WebSpark.HttpClientUtility/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/WebSpark.HttpClientUtility.svg)](https://www.nuget.org/packages/WebSpark.HttpClientUtility/)
[![Crawler Package](https://img.shields.io/nuget/v/WebSpark.HttpClientUtility.Crawler.svg?label=Crawler)](https://www.nuget.org/packages/WebSpark.HttpClientUtility.Crawler/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://github.com/MarkHazleton/HttpClientUtility/actions/workflows/publish-nuget.yml/badge.svg)](https://github.com/MarkHazleton/HttpClientUtility/actions/workflows/publish-nuget.yml)
[![.NET 8-10](https://img.shields.io/badge/.NET-8--10-blue.svg)](https://dotnet.microsoft.com/download/dotnet)
[![Documentation](https://img.shields.io/badge/docs-GitHub%20Pages-blue)](https://markhazleton.github.io/WebSpark.HttpClientUtility/)

**A production-ready HttpClient wrapper for .NET 8-10 that makes HTTP calls simple, resilient, and observable.**

Stop writing boilerplate HTTP code. Get built-in resilience, caching, telemetry, and structured logging out of the box.

## 📦 v2.0 - Now in Two Focused Packages!

Starting with v2.0, the library is split into two packages:

| Package | Purpose | Size | Use When |
|---------|---------|------|----------|
| **[WebSpark.HttpClientUtility](https://www.nuget.org/packages/WebSpark.HttpClientUtility/)** | Core HTTP features | 163 KB | You need HTTP client utilities (authentication, caching, resilience, telemetry) |
| **[WebSpark.HttpClientUtility.Crawler](https://www.nuget.org/packages/WebSpark.HttpClientUtility.Crawler/)** | Web crawling extension | 75 KB | You need web crawling, robots.txt parsing, sitemap generation |

**Upgrading from v1.x?** Most users need no code changes! See [Migration Guide](#-upgrading-from-v1x).

## 📚 Documentation

**[View Full Documentation →](https://markhazleton.github.io/WebSpark.HttpClientUtility/)**

The complete documentation site includes:
- Getting started guide
- Feature documentation
- API reference
- Code examples
- Best practices

## ⚡ Quick Start

### Core HTTP Features (Base Package)

**Install**
```bash
dotnet add package WebSpark.HttpClientUtility
```

**5-Minute Example**
```csharp
// Program.cs - Register services (ONE LINE!)
builder.Services.AddHttpClientUtility();

// YourService.cs - Make requests
public class WeatherService
{
    private readonly IHttpRequestResultService _httpService;
    
    public WeatherService(IHttpRequestResultService httpService) => _httpService = httpService;

    public async Task<WeatherData?> GetWeatherAsync(string city)
    {
        var request = new HttpRequestResult<WeatherData>
        {
     RequestPath = $"https://api.weather.com/forecast?city={city}",
     RequestMethod = HttpMethod.Get
        };
 
        var result = await _httpService.HttpSendRequestResultAsync(request);
        return result.IsSuccessStatusCode ? result.ResponseResults : null;
    }
}
```

That's it! You now have:
- ✅ Automatic correlation IDs for tracing
- ✅ Structured logging with request/response details
- ✅ Request timing telemetry
- ✅ Proper error handling and exception management
- ✅ Support for .NET 8 LTS, .NET 9, and .NET 10 (Preview)

### Web Crawling Features (Crawler Package)

**Install Both Packages**
```bash
dotnet add package WebSpark.HttpClientUtility
dotnet add package WebSpark.HttpClientUtility.Crawler
```

**Register Services**
```csharp
// Program.cs
builder.Services.AddHttpClientUtility();
builder.Services.AddHttpClientCrawler();  // Adds crawler features
```

**Use Crawler**
```csharp
public class SiteAnalyzer
{
    private readonly ISiteCrawler _crawler;
    
    public SiteAnalyzer(ISiteCrawler crawler) => _crawler = crawler;

    public async Task<CrawlResult> AnalyzeSiteAsync(string url)
    {
        var options = new CrawlerOptions
        {
            MaxDepth = 3,
            MaxPages = 100,
            RespectRobotsTxt = true
        };
        
        return await _crawler.CrawlAsync(url, options);
    }
}
```

## 🎯 Why Choose This Library?

| Challenge | Solution |
|-----------|----------|
| **Boilerplate Code** | One-line service registration replaces 50+ lines of manual setup |
| **Transient Failures** | Built-in Polly integration for retries and circuit breakers |
| **Repeated API Calls** | Automatic response caching with customizable duration |
| **Observability** | Correlation IDs, structured logging, and OpenTelemetry support |
| **Testing** | All services are interface-based for easy mocking |
| **Package Size** | Modular design - install only what you need |

## 🚀 Features

### Base Package Features
- **Simple API** - Intuitive request/response model
- **Authentication** - Bearer token, Basic auth, API key providers
- **Correlation IDs** - Automatic tracking across distributed systems
- **Structured Logging** - Rich context in all log messages
- **Telemetry** - Request timing and performance metrics
- **Error Handling** - Standardized exception processing
- **Type-Safe** - Strongly-typed request and response models
- **Caching** - In-memory response caching (optional)
- **Resilience** - Polly retry and circuit breaker policies (optional)
- **Concurrent Requests** - Parallel request processing
- **Fire-and-Forget** - Background request execution
- **Streaming** - Efficient handling of large responses
- **OpenTelemetry** - Full observability integration (optional)
- **CURL Export** - Generate CURL commands for debugging

### Crawler Package Features
- **Site Crawling** - Full website crawling with depth control
- **Robots.txt** - Automatic compliance with robots.txt rules
- **Sitemap Generation** - Create XML sitemaps from crawl results
- **HTML Parsing** - Extract links and metadata with HtmlAgilityPack
- **SignalR Progress** - Real-time crawl progress updates
- **CSV Export** - Export crawl results to CSV files
- **Performance Tracking** - Monitor crawl speed and efficiency

## 📚 Common Scenarios

### Enable Caching
```csharp
builder.Services.AddHttpClientUtility(options =>
{
    options.EnableCaching = true;
});

// In your service
var request = new HttpRequestResult<Product>
{
    RequestPath = "https://api.example.com/products/123",
    RequestMethod = HttpMethod.Get,
    CacheDurationMinutes = 10  // Cache for 10 minutes
};
```

### Add Resilience (Retry + Circuit Breaker)
```csharp
builder.Services.AddHttpClientUtility(options =>
{
    options.EnableResilience = true;
  options.ResilienceOptions.MaxRetryAttempts = 3;
    options.ResilienceOptions.RetryDelay = TimeSpan.FromSeconds(2);
});
```

### All Features Enabled
```csharp
builder.Services.AddHttpClientUtilityWithAllFeatures();
```

## 🔄 Upgrading from v1.x

### If You DON'T Use Web Crawling

**No code changes required!** Simply upgrade:

```bash
dotnet add package WebSpark.HttpClientUtility --version 2.0.0
```

Your existing code continues to work exactly as before. All core HTTP features (authentication, caching, resilience, telemetry, etc.) are still in the base package with the same API.

### If You DO Use Web Crawling

Three simple steps to migrate:

**Step 1**: Install the crawler package
```bash
dotnet add package WebSpark.HttpClientUtility.Crawler --version 2.0.0
```

**Step 2**: Add using directive
```csharp
using WebSpark.HttpClientUtility.Crawler;
```

**Step 3**: Update service registration
```csharp
// v1.x (old)
services.AddHttpClientUtility();

// v2.0 (new)
services.AddHttpClientUtility();
services.AddHttpClientCrawler();  // Add this line
```

That's it! Your crawler code (ISiteCrawler, SiteCrawler, SimpleSiteCrawler, etc.) works identically after these changes.

**Need Help?** See the [detailed migration guide](https://markhazleton.github.io/WebSpark.HttpClientUtility/getting-started/migration-v2/) or [open an issue](https://github.com/MarkHazleton/HttpClientUtility/issues).

## 📖 Documentation

- **[Getting Started Guide](documentation/GettingStarted.md)** - Complete walkthrough
- **[Configuration Options](documentation/Configuration.md)** - All settings explained
- **[Caching Guide](documentation/Caching.md)** - Response caching strategies
- **[Resilience Guide](documentation/Resilience.md)** - Retry and circuit breaker patterns
- **[Web Crawling](documentation/WebCrawling.md)** - Site crawler features
- **[Migration Guide](documentation/Migration.md)** - From raw HttpClient
- **[API Reference](documentation/ApiReference.md)** - Complete API documentation

## 🎓 Sample Projects

Explore working examples in the [samples directory](samples/):
- **BasicUsage** - Simple GET/POST requests
- **WithCaching** - Response caching implementation
- **WithResilience** - Retry and circuit breaker patterns
- **ConcurrentRequests** - Parallel request processing
- **WebCrawler** - Site crawling example

## 🆚 Comparison to Alternatives

| Feature | WebSpark.HttpClientUtility | Raw HttpClient | RestSharp | Refit |
|---------|---------------------------|----------------|-----------|-------|
| Setup Complexity | ⭐ One line | ⭐⭐⭐ Manual | ⭐⭐ Low | ⭐⭐ Low |
| Built-in Caching | ✅ Yes | ❌ Manual | ❌ Manual | ⚠️ Plugin |
| Built-in Resilience | ✅ Yes | ❌ Manual | ❌ Manual | ❌ Manual |
| Telemetry | ✅ Built-in | ⚠️ Manual | ⚠️ Manual | ⚠️ Manual |
| Type Safety | ✅ Yes | ⚠️ Partial | ✅ Yes | ✅ Yes |
| Web Crawling | ✅ Yes | ❌ No | ❌ No | ❌ No |
| .NET 8-10 Support | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |

## 🤝 Contributing

Contributions are welcome! See our [Contributing Guide](documentation/CONTRIBUTING.md) for details.

1. Fork the repository
2. Create a feature branch
3. Add tests for your changes
4. Ensure all tests pass
5. Submit a pull request

## 📊 Project Stats

- **252+ Unit Tests** - 100% passing
- **Supports .NET 8 LTS, .NET 9, & .NET 10 (Preview)**
- **MIT Licensed** - Free for commercial use
- **Active Maintenance** - Regular updates

## 📦 Related Packages

- [WebSpark.HttpClientUtility.Testing](https://nuget.org/packages/WebSpark.HttpClientUtility.Testing) - Test helpers (coming soon)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🔗 Links

- [GitHub Repository](https://github.com/markhazleton/httpclientutility)
- [NuGet Package](https://www.nuget.org/packages/WebSpark.HttpClientUtility)
- [Changelog](CHANGELOG.md)
- [Issue Tracker](https://github.com/markhazleton/httpclientutility/issues)
- [Discussions](https://github.com/markhazleton/httpclientutility/discussions)

---

**Questions or Issues?** [Open an issue](https://github.com/markhazleton/httpclientutility/issues) or [start a discussion](https://github.com/markhazleton/httpclientutility/discussions)!
