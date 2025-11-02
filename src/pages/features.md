---
layout: layouts/base.njk
title: Features
description: Comprehensive features of WebSpark.HttpClientUtility including resilience, caching, telemetry, and web crawling.
permalink: /features/
---

# Features

WebSpark.HttpClientUtility provides a comprehensive set of features designed to simplify HTTP client usage in .NET applications.

## Core Features

### 🚀 Simple Service Registration

One-line setup with fluent configuration:

```csharp
// Basic setup
services.AddHttpClientUtility();

// With caching
services.AddHttpClientUtilityWithCaching();

// All features enabled
services.AddHttpClientUtilityWithAllFeatures();

// Custom configuration
services.AddHttpClientUtility(options => {
    options.EnableCaching = true;
    options.EnableResilience = true;
    options.EnableTelemetry = true;
    options.ResilienceOptions.MaxRetryAttempts = 5;
    options.ResilienceOptions.RetryDelay = TimeSpan.FromSeconds(2);
});
```

### 🔄 Resilience with Polly Integration

Built-in retry policies and circuit breakers:

- **Automatic Retry**: Configurable retry attempts with exponential backoff
- **Circuit Breaker**: Prevents cascading failures
- **Timeout Handling**: Request-level timeout configuration
- **Transient Fault Handling**: Automatic retry on network errors

Configuration options:
- `MaxRetryAttempts` (default: 3)
- `RetryDelay` (default: 1 second)
- `CircuitBreakerThreshold` (default: 5 failures)
- `CircuitBreakerDuration` (default: 30 seconds)

### 💾 Response Caching

Intelligent caching with configurable duration:

```csharp
var request = new HttpRequestResult<WeatherData>
{
    RequestPath = "https://api.example.com/weather",
    CacheDurationMinutes = 10 // Cache for 10 minutes
};

var result = await _httpService.HttpSendRequestResultAsync(request);
```

Features:
- **Per-Request Configuration**: Set cache duration per request
- **Automatic Invalidation**: Time-based cache expiration
- **Memory Efficient**: Uses `IMemoryCache` from Microsoft.Extensions.Caching
- **Cache Keys**: Based on request path for efficient lookup

### 📊 Telemetry & Observability

OpenTelemetry integration with comprehensive metrics:

```csharp
services.AddWebSparkOpenTelemetry(tracerBuilder => {
    tracerBuilder
        .AddOtlpExporter()
        .AddConsoleExporter();
});
```

Features:
- **Request Tracing**: Track request duration and status
- **Correlation IDs**: Auto-generated for distributed tracing
- **Metrics**: Success rate, response time, error counts
- **OTLP Support**: Modern OpenTelemetry Protocol integration

### 🕷️ Web Crawling

Full-featured web crawler with robots.txt support:

```csharp
var options = new CrawlerOptions
{
    BaseUrl = "https://example.com",
    MaxDepth = 3,
    MaxPages = 100,
    RespectRobotsTxt = true,
    UserAgent = "MyBot/1.0"
};

var crawler = new SiteCrawler(options, httpService, logger);
await crawler.CrawlAsync(cancellationToken);
```

Features:
- **Robots.txt Parsing**: Automatic compliance with robots.txt rules
- **Depth Control**: Limit crawl depth to prevent infinite loops
- **Rate Limiting**: Configurable delay between requests
- **SignalR Integration**: Real-time progress updates
- **Concurrent Crawling**: Parallel page processing

### 🔐 Authentication

Multiple authentication providers built-in:

```csharp
// Bearer Token
var bearerAuth = new BearerTokenAuthenticationProvider("your-token");

// Basic Auth
var basicAuth = new BasicAuthenticationProvider("username", "password");

// API Key
var apiKeyAuth = new ApiKeyAuthenticationProvider("X-API-Key", "your-key");

var request = new HttpRequestResult<Data>
{
    RequestPath = "https://api.example.com/data",
    AuthenticationProvider = bearerAuth
};
```

### 🧪 Testing Support

Mock service for unit testing:

```csharp
var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
{
    Content = new StringContent("{\"id\":1}", Encoding.UTF8, "application/json")
};

var mockService = new MockHttpRequestResultService<Data>(mockResponse);
var result = await mockService.HttpSendRequestResultAsync(request);

Assert.IsTrue(result.IsSuccessStatusCode);
```

### 🔧 Advanced Features

#### Object Pooling
Efficient memory management with object pool pattern for high-throughput scenarios.

#### Fire-and-Forget Requests
Background HTTP requests without waiting for responses.

#### Concurrent Requests
Execute multiple HTTP requests in parallel with automatic result aggregation.

#### Request/Response Streaming
Support for streaming large files and real-time data.

#### CURL Command Generation
Save requests as CURL commands for debugging and sharing.

## Decorator Pattern Architecture

The library uses a decorator pattern for composable features:

1. **Base Layer**: `HttpRequestResultService` - Core HTTP functionality
2. **Cache Layer**: `HttpRequestResultServiceCache` - Optional caching (wraps base)
3. **Resilience Layer**: `HttpRequestResultServicePolly` - Optional Polly policies (wraps cache)
4. **Telemetry Layer**: `HttpRequestResultServiceTelemetry` - Optional metrics (wraps resilience)

This design allows you to:
- Enable only the features you need
- Maintain clean separation of concerns
- Easy to test each layer independently
- Extend with custom decorators

## Framework Support

- ✅ .NET 8.0 (LTS - Long-Term Support)
- ✅ .NET 9.0 (Latest)

## Production Ready

- **252+ Passing Tests**: Comprehensive test coverage with MSTest
- **Strong Naming**: Assembly is signed for enterprise scenarios
- **Nullable Reference Types**: Full nullability annotations
- **Code Analysis**: Warning level 5 with .NET analyzers enabled
- **Multi-Targeting**: Supports both .NET 8 LTS and .NET 9

## Next Steps

<div class="cta-buttons">
  <a href="{{ '/getting-started/' | url }}" class="button primary">Get Started</a>
  <a href="{{ '/examples/' | url }}" class="button secondary">View Examples</a>
</div>
