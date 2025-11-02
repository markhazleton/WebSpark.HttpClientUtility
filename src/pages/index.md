---
layout: layouts/base.njk
title: Home
description: A production-ready HttpClient wrapper for .NET 8+ with resilience, caching, telemetry, and web crawling capabilities.
permalink: /
---

<div class="hero">
  <h1>{{ site.name }}</h1>
  <p class="hero-tagline">{{ nuget.description }}</p>
  
  <div class="hero-stats">
    <span class="stat">v{{ nuget.version }}</span>
    <span class="stat">{{ nuget.displayDownloads }} downloads</span>
  </div>
  
  <div class="hero-cta">
    <code>dotnet add package WebSpark.HttpClientUtility</code>
  </div>
</div>

## Why Choose This Library?

- **One-Line Setup**: Simple service registration with `AddHttpClientUtility()`
- **Battle-Tested**: 252+ passing tests, production-ready for .NET 8 LTS and .NET 9
- **Zero Boilerplate**: Eliminate repetitive HTTP client code
- **Enterprise-Grade**: Used in production applications with proven reliability

## Features

✅ **Simple one-line service registration**  
No complex configuration required. Just call `services.AddHttpClientUtility()` and you're ready to go.

✅ **Built-in resilience with Polly integration**  
Automatic retry policies, circuit breakers, and timeout handling out of the box.

✅ **Automatic response caching**  
Intelligent caching with configurable duration and cache invalidation strategies.

✅ **Comprehensive telemetry and logging**  
OpenTelemetry integration with correlation IDs for distributed tracing.

✅ **Web crawling capabilities**  
Robots.txt parsing, SignalR progress updates, and concurrent crawling support.

✅ **Flexible authentication**  
Built-in providers for Bearer tokens, Basic auth, and API keys.

## Quick Example

```csharp
// Register services with optional features
builder.Services.AddHttpClientUtility(options => {
    options.EnableCaching = true;
    options.EnableResilience = true;
    options.EnableTelemetry = true;
});

// Make HTTP requests with automatic retries and caching
var request = new HttpRequestResult<WeatherData>
{
    RequestPath = "https://api.example.com/weather",
    RequestMethod = HttpMethod.Get,
    CacheDurationMinutes = 10, // Optional caching
    AuthenticationProvider = new BearerTokenAuthenticationProvider("your-token")
};

var result = await _httpService.HttpSendRequestResultAsync(request);

if (result.IsSuccessStatusCode)
{
    var weather = result.ResponseResults;
    Console.WriteLine($"Temperature: {weather.Temperature}°C");
}
```

## Get Started

<div class="cta-buttons">
  <a href="{{ '/getting-started/' | url }}" class="button primary">Get Started</a>
  <a href="{{ '/features/' | url }}" class="button secondary">View Features</a>
</div>

## What's Included

The library provides a complete HTTP client wrapper with:

- **Core Services**: `IHttpRequestResultService` for making HTTP requests
- **Caching Layer**: Automatic response caching with configurable expiration
- **Resilience Layer**: Polly-based retry and circuit breaker policies
- **Telemetry Layer**: OpenTelemetry integration with metrics and tracing
- **Web Crawler**: Full-featured web crawling with robots.txt support
- **Authentication**: Multiple authentication providers (Bearer, Basic, API Key)
- **Testing Utilities**: Mock service for unit testing

## Architecture

The library uses a **decorator pattern** for composing features:

1. **Base Service**: Core HTTP request functionality
2. **Cache Decorator**: Adds caching capabilities (optional)
3. **Resilience Decorator**: Adds Polly policies (optional)
4. **Telemetry Decorator**: Adds metrics and tracing (optional)

This design allows you to enable only the features you need while maintaining clean, testable code.

## Targets

- .NET 8.0 LTS (Long-Term Support)
- .NET 9.0 (Latest)

## License

Licensed under MIT. Free for commercial and open-source use.
