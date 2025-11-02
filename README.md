# WebSpark.HttpClientUtility

[![NuGet Version](https://img.shields.io/nuget/v/WebSpark.HttpClientUtility.svg)](https://www.nuget.org/packages/WebSpark.HttpClientUtility/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/WebSpark.HttpClientUtility.svg)](https://www.nuget.org/packages/WebSpark.HttpClientUtility/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://github.com/MarkHazleton/HttpClientUtility/actions/workflows/publish-nuget.yml/badge.svg)](https://github.com/MarkHazleton/HttpClientUtility/actions/workflows/publish-nuget.yml)
[![.NET 8+](https://img.shields.io/badge/.NET-8%2B-blue.svg)](https://dotnet.microsoft.com/download/dotnet)
[![Documentation](https://img.shields.io/badge/docs-GitHub%20Pages-blue)](https://markhazleton.github.io/WebSpark.HttpClientUtility/)

**A production-ready HttpClient wrapper for .NET 8+ that makes HTTP calls simple, resilient, and observable.**

Stop writing boilerplate HTTP code. Get built-in resilience, caching, telemetry, and structured logging out of the box.

## 📚 Documentation

**[View Full Documentation →](https://markhazleton.github.io/WebSpark.HttpClientUtility/)**

The complete documentation site includes:
- Getting started guide
- Feature documentation
- API reference
- Code examples
- Best practices

## ⚡ Quick Start

### Install
```bash
dotnet add package WebSpark.HttpClientUtility
```

### 5-Minute Example
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
- ✅ Support for .NET 8 LTS and .NET 9

## 🎯 Why Choose This Library?

| Challenge | Solution |
|-----------|----------|
| **Boilerplate Code** | One-line service registration replaces 50+ lines of manual setup |
| **Transient Failures** | Built-in Polly integration for retries and circuit breakers |
| **Repeated API Calls** | Automatic response caching with customizable duration |
| **Observability** | Correlation IDs, structured logging, and OpenTelemetry support |
| **Testing** | All services are interface-based for easy mocking |

## 🚀 Features

### Core Features (Always Included)
- **Simple API** - Intuitive request/response model
- **Correlation IDs** - Automatic tracking across distributed systems
- **Structured Logging** - Rich context in all log messages
- **Telemetry** - Request timing and performance metrics
- **Error Handling** - Standardized exception processing
- **Type-Safe** - Strongly-typed request and response models

### Optional Features (Enable as Needed)
- **Caching** - In-memory response caching
- **Resilience** - Polly retry and circuit breaker policies
- **Concurrent Requests** - Parallel request processing
- **Web Crawling** - Site crawling with robots.txt support
- **OpenTelemetry** - Full observability integration

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
| .NET 8 LTS Support | ✅ Yes | ✅ Yes | ✅ Yes | ✅ Yes |

## 🤝 Contributing

Contributions are welcome! See our [Contributing Guide](documentation/CONTRIBUTING.md) for details.

1. Fork the repository
2. Create a feature branch
3. Add tests for your changes
4. Ensure all tests pass
5. Submit a pull request

## 📊 Project Stats

- **252+ Unit Tests** - 100% passing
- **Supports .NET 8 LTS & .NET 9**
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
