# WebSpark.HttpClientUtility

**Drop-in HttpClient wrapper with Polly resilience, response caching, and OpenTelemetry for .NET 8-10 LTS APIs—configured in one line**

[![NuGet Version](https://img.shields.io/nuget/v/WebSpark.HttpClientUtility.svg)](https://www.nuget.org/packages/WebSpark.HttpClientUtility/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/WebSpark.HttpClientUtility.svg)](https://www.nuget.org/packages/WebSpark.HttpClientUtility/)
[![Crawler Package](https://img.shields.io/nuget/v/WebSpark.HttpClientUtility.Crawler.svg?label=Crawler)](https://www.nuget.org/packages/WebSpark.HttpClientUtility.Crawler/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://github.com/MarkHazleton/HttpClientUtility/actions/workflows/publish-nuget.yml/badge.svg)](https://github.com/MarkHazleton/HttpClientUtility/actions/workflows/publish-nuget.yml)
[![.NET 8-10 LTS](https://img.shields.io/badge/.NET-8--10%20LTS-512BD4.svg)](https://dotnet.microsoft.com/download/dotnet)
[![Documentation](https://img.shields.io/badge/docs-GitHub%20Pages-blue)](https://markhazleton.github.io/WebSpark.HttpClientUtility/)

---

Stop writing 50+ lines of HttpClient setup. Get enterprise-grade resilience (retries, circuit breakers), intelligent caching, structured logging with correlation IDs, and OpenTelemetry tracing in a single `AddHttpClientUtility()` call. Perfect for microservices, background workers, and web scrapers.

---

## 🚀 Why Choose WebSpark.HttpClientUtility?

**Your HTTP setup in 1 line vs. 50+**

| Feature | WebSpark.HttpClientUtility | Raw HttpClient | RestSharp | Refit |
|---------|---------------------------|----------------|-----------|-------|
| Setup Complexity | ⭐ One line | ⭐⭐⭐ 50+ lines manual | ⭐⭐ Low | ⭐⭐ Low |
| Built-in Retry/Circuit Breaker | ✅ Polly integrated | ❌ Manual Polly setup | ❌ Manual | ❌ Manual |
| Response Caching | ✅ Configurable, in-memory | ❌ Manual | ❌ Manual | ❌ Manual |
| Correlation IDs | ✅ Automatic | ❌ Manual middleware | ❌ Manual | ❌ Manual |
| OpenTelemetry | ✅ Built-in | ❌ Manual ActivitySource | ❌ Manual | ❌ Manual |
| Structured Logging | ✅ Rich context | ❌ Manual ILogger | ⭐⭐ Basic | ⭐⭐ Basic |
| Web Crawling | ✅ Separate package | ❌ No | ❌ No | ❌ No |
| Production Trust | ✅ 237 unique tests, LTS support | ✅ Microsoft-backed | ✅ Popular (7M+ downloads) | ✅ Popular (10M+ downloads) |

**When to use WebSpark:**

- ✅ Building microservices with distributed tracing requirements
- ✅ Need resilience patterns without writing Polly boilerplate
- ✅ Want intelligent caching for API rate-limit compliance
- ✅ Building web scrapers or crawlers (with Crawler package)

**When NOT to use WebSpark:**

- ❌ You need declarative, type-safe API clients (use Refit)
- ❌ You want maximum control and minimal magic (use raw HttpClient)
- ❌ Legacy .NET Framework 4.x projects (WebSpark requires .NET 8+)

---

## 🛡️ Production Trust

**Battle-Tested & Production-Ready**

- ✅ **237+ unit tests** (711 test runs across 3 frameworks) with 100% passing - tested on .NET 8, 9, and 10
- ✅ **Source Link enabled** - step-through debugging with symbol packages (.snupkg)
- ✅ **Trimming & AOT ready** - annotated for Native AOT and IL trimming compatibility
- ✅ **Package validation** - baseline validation ensures no breaking changes
- ✅ **Zero-warning builds** - strict code quality with `TreatWarningsAsErrors=true`
- ✅ **Continuous Integration** via GitHub Actions - every commit tested
- ✅ **Semantic Versioning** - predictable, safe upgrades
- ✅ **Zero breaking changes** within major versions - backward compatibility guaranteed
- ✅ **Framework Support:** .NET 8 LTS (until Nov 2026), .NET 9, .NET 10
- ✅ **MIT Licensed** - free for commercial use

**Support & Maintenance**

- 🔄 **Active development** - regular updates and improvements
- 📅 **Long-term support** - each major version supported for 18+ months
- 💬 **Community support** - GitHub Discussions for questions and best practices
- 📖 **Comprehensive documentation** - [Full docs site](https://markhazleton.github.io/WebSpark.HttpClientUtility/)

**Breaking Change Commitment**

We follow semantic versioning strictly:

- **Patch versions (2.0.x):** Bug fixes only, zero breaking changes
- **Minor versions (2.x.0):** New features, backward compatible
- **Major versions (x.0.0):** Breaking changes with detailed migration guides

---

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

## ⚡ 30-Second Quick Start

**Install**

```bash
dotnet add package WebSpark.HttpClientUtility
```

**Minimal Example (Absolute Minimum)**

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClientUtility();
var app = builder.Build();

app.MapGet("/weather", async (IHttpRequestResultService http) =>
{
    var request = new HttpRequestResult<WeatherData>
    {
        RequestPath = "https://api.weather.com/forecast?city=Seattle",
        RequestMethod = HttpMethod.Get
    };
    var result = await http.HttpSendRequestResultAsync(request);
    return result.IsSuccessStatusCode ? Results.Ok(result.ResponseResults) : Results.Problem();
});

app.Run();

record WeatherData(string City, int Temp);
```

That's it! You now have:

- ✅ Automatic correlation IDs for tracing
- ✅ Structured logging with request/response details
- ✅ Request timing telemetry
- ✅ Proper error handling and exception management
- ✅ Support for .NET 8 LTS, .NET 9, and .NET 10

<details>
<summary>📖 Show more: Service-based pattern with error handling</summary>

```csharp
// Program.cs
builder.Services.AddHttpClientUtility(options =>
{
    options.EnableCaching = true;      // Cache responses
    options.EnableResilience = true;   // Retry on failure
});

// WeatherService.cs
public class WeatherService
{
    private readonly IHttpRequestResultService _http;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        IHttpRequestResultService http,
        ILogger<WeatherService> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<WeatherData?> GetWeatherAsync(string city)
    {
        var request = new HttpRequestResult<WeatherData>
        {
            RequestPath = $"https://api.weather.com/forecast?city={city}",
            RequestMethod = HttpMethod.Get,
            CacheDurationMinutes = 10  // Cache for 10 minutes
        };

        var result = await _http.HttpSendRequestResultAsync(request);

        if (!result.IsSuccessStatusCode)
        {
            _logger.LogError(
                "Weather API failed: {StatusCode} - {Error}",
                result.StatusCode,
                result.ErrorDetails
            );
            return null;
        }

        return result.ResponseResults;
    }
}
```

</details>

<details>
<summary>📖 Show more: Full-featured with auth and observability</summary>

```csharp
// Program.cs - Advanced configuration
builder.Services.AddHttpClientUtility(options =>
{
    options.EnableCaching = true;
    options.EnableResilience = true;
    options.ResilienceOptions.MaxRetryAttempts = 3;
    options.ResilienceOptions.RetryDelay = TimeSpan.FromSeconds(2);
    options.DefaultTimeout = TimeSpan.FromSeconds(30);
});

// WeatherService.cs - Advanced usage
public async Task<WeatherData?> GetWeatherWithAuthAsync(string city, string apiKey)
{
    var request = new HttpRequestResult<WeatherData>
    {
        RequestPath = $"https://api.weather.com/forecast?city={city}",
        RequestMethod = HttpMethod.Get,
        CacheDurationMinutes = 10,
        Headers = new Dictionary<string, string>
        {
            ["X-API-Key"] = apiKey,
            ["Accept"] = "application/json"
        }
    };

    var result = await _http.HttpSendRequestResultAsync(request);

    // Correlation ID is automatically logged and propagated
    _logger.LogInformation(
        "Weather request completed in {Duration}ms with correlation {CorrelationId}",
        result.RequestDuration,
        result.CorrelationId
    );

    return result.IsSuccessStatusCode ? result.ResponseResults : null;
}
```

</details>

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
- **Source Link** - Step-through debugging with symbol packages
- **Trimming/AOT Ready** - Compatible with Native AOT and IL trimming
- **Package Validation** - Baseline validation ensures stability

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
dotnet add package WebSpark.HttpClientUtility
```

Your existing code continues to work exactly as before. All core HTTP features (authentication, caching, resilience, telemetry, etc.) are still in the base package with the same API.

### If You DO Use Web Crawling

Three simple steps to migrate:

**Step 1**: Install the crawler package
```bash
dotnet add package WebSpark.HttpClientUtility.Crawler
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

## 🤝 Contributing

Contributions are welcome! See our [Contributing Guide](documentation/CONTRIBUTING.md) for details.

1. Fork the repository
2. Create a feature branch
3. Add tests for your changes
4. Ensure all tests pass
5. Submit a pull request

## 📊 Project Stats

- **237 Unique Unit Tests (711 test runs)** - 100% passing across .NET 8, 9, and 10
- **Supports .NET 8 LTS, .NET 9, & .NET 10**
- **MIT Licensed** - Free for commercial use
- **Active Maintenance** - Regular updates

## 📦 Related Packages

| Package | Purpose | Status |
|---------|---------|--------|
| [WebSpark.HttpClientUtility.Testing](https://nuget.org/packages/WebSpark.HttpClientUtility.Testing) | Test helpers & fakes for unit testing | ✅ Available (v2.1.0+) |

**Testing Package Features:**

- **FakeHttpResponseHandler** - Mock HTTP responses without network calls
- **Fluent API** - Easy test setup with `ForRequest().RespondWith()`
- **Sequential Responses** - Test retry behavior with multiple responses
- **Request Verification** - Assert requests were made correctly
- **Latency Simulation** - Test timeout scenarios

```bash
dotnet add package WebSpark.HttpClientUtility.Testing
```

See the [Testing documentation](https://markhazleton.github.io/WebSpark.HttpClientUtility/testing/) for examples.

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
