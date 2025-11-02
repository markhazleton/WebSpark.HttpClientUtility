---
layout: layouts/base.njk
title: API Reference
description: Complete API reference for WebSpark.HttpClientUtility classes and interfaces.
permalink: /api-reference/
---

# API Reference

Complete reference documentation for WebSpark.HttpClientUtility.

## Core Interfaces

### IHttpRequestResultService

Main interface for making HTTP requests.

```csharp
public interface IHttpRequestResultService
{
    Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(
        HttpRequestResult<T> request,
        CancellationToken cancellationToken = default);
}
```

**Methods:**
- `HttpSendRequestResultAsync<T>` - Sends an HTTP request and returns a typed result

## Core Classes

### HttpRequestResult&lt;T&gt;

Represents an HTTP request and its response.

```csharp
public class HttpRequestResult<T>
{
    // Request Properties
    public string RequestPath { get; set; }
    public HttpMethod RequestMethod { get; set; }
    public object? RequestBody { get; set; }
    public Dictionary<string, string>? RequestHeaders { get; set; }
    public string? CorrelationId { get; set; }
    
    // Caching
    public int CacheDurationMinutes { get; set; }
    
    // Authentication
    public IAuthenticationProvider? AuthenticationProvider { get; set; }
    
    // Response Properties (populated after request)
    public T? ResponseResults { get; set; }
    public string? ResponseContent { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public bool IsSuccessStatusCode { get; set; }
    public long RequestDuration { get; set; }
    public Dictionary<string, IEnumerable<string>>? ResponseHeaders { get; set; }
}
```

**Key Properties:**

| Property | Type | Description |
|----------|------|-------------|
| `RequestPath` | `string` | The URL to send the request to |
| `RequestMethod` | `HttpMethod` | HTTP method (GET, POST, PUT, DELETE, etc.) |
| `RequestBody` | `object?` | Request body object (serialized to JSON) |
| `RequestHeaders` | `Dictionary<string, string>?` | Custom headers to include |
| `CorrelationId` | `string?` | Tracking ID (auto-generated if not provided) |
| `CacheDurationMinutes` | `int` | Cache duration in minutes (0 = no cache) |
| `AuthenticationProvider` | `IAuthenticationProvider?` | Authentication provider |
| `ResponseResults` | `T?` | Deserialized response object |
| `ResponseContent` | `string?` | Raw response content |
| `StatusCode` | `HttpStatusCode` | HTTP status code |
| `IsSuccessStatusCode` | `bool` | True if status code is 2xx |
| `RequestDuration` | `long` | Request duration in milliseconds |

## Service Registration Extensions

### ServiceCollectionExtensions

Extension methods for configuring services.

```csharp
public static class ServiceCollectionExtensions
{
    // Basic registration
    public static IServiceCollection AddHttpClientUtility(
        this IServiceCollection services);

    // With configuration
    public static IServiceCollection AddHttpClientUtility(
        this IServiceCollection services,
        Action<HttpClientUtilityOptions> configure);

    // Quick setups
    public static IServiceCollection AddHttpClientUtilityWithCaching(
        this IServiceCollection services);

    public static IServiceCollection AddHttpClientUtilityWithAllFeatures(
        this IServiceCollection services);
}
```

### HttpClientUtilityOptions

Configuration options for the library.

```csharp
public class HttpClientUtilityOptions
{
    public bool EnableCaching { get; set; } = false;
    public bool EnableResilience { get; set; } = false;
    public bool EnableTelemetry { get; set; } = false;
    public HttpRequestResultPollyOptions ResilienceOptions { get; set; }
}
```

### HttpRequestResultPollyOptions

Resilience configuration options.

```csharp
public class HttpRequestResultPollyOptions
{
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    public int CircuitBreakerThreshold { get; set; } = 5;
    public TimeSpan CircuitBreakerDuration { get; set; } = TimeSpan.FromSeconds(30);
}
```

## Authentication

### IAuthenticationProvider

Interface for authentication providers.

```csharp
public interface IAuthenticationProvider
{
    Task ApplyAuthenticationAsync(HttpRequestMessage request);
}
```

### BearerTokenAuthenticationProvider

Bearer token authentication.

```csharp
public class BearerTokenAuthenticationProvider : IAuthenticationProvider
{
    public BearerTokenAuthenticationProvider(string token);
    public Task ApplyAuthenticationAsync(HttpRequestMessage request);
}
```

### BasicAuthenticationProvider

Basic authentication (username/password).

```csharp
public class BasicAuthenticationProvider : IAuthenticationProvider
{
    public BasicAuthenticationProvider(string username, string password);
    public Task ApplyAuthenticationAsync(HttpRequestMessage request);
}
```

### ApiKeyAuthenticationProvider

API key authentication.

```csharp
public class ApiKeyAuthenticationProvider : IAuthenticationProvider
{
    public ApiKeyAuthenticationProvider(string headerName, string apiKey);
    public Task ApplyAuthenticationAsync(HttpRequestMessage request);
}
```

## Web Crawling

### CrawlerOptions

Configuration for web crawler.

```csharp
public class CrawlerOptions
{
    public string BaseUrl { get; set; }
    public int MaxDepth { get; set; } = 3;
    public int MaxPages { get; set; } = 100;
    public bool RespectRobotsTxt { get; set; } = true;
    public string UserAgent { get; set; } = "WebSparkCrawler/1.0";
    public TimeSpan DelayBetweenRequests { get; set; } = TimeSpan.FromMilliseconds(500);
}
```

### SiteCrawler

Web crawler implementation.

```csharp
public class SiteCrawler
{
    public SiteCrawler(
        CrawlerOptions options,
        IHttpRequestResultService httpService,
        ILogger<SiteCrawler> logger);

    public Task CrawlAsync(CancellationToken cancellationToken = default);
    
    public event EventHandler<PageCrawledEventArgs>? PageCrawled;
    public event EventHandler<CrawlProgressEventArgs>? CrawlProgress;
}
```

**Events:**
- `PageCrawled` - Fired after each page is crawled
- `CrawlProgress` - Fired periodically with progress updates

## Utilities

### QueryStringParametersList

Helper for building query strings.

```csharp
public class QueryStringParametersList : List<KeyValuePair<string, string>>
{
    public void Add(string key, string value);
    public string ToQueryString();
}
```

**Example:**
```csharp
var queryParams = new QueryStringParametersList
{
    { "page", "1" },
    { "pageSize", "50" }
};

var queryString = queryParams.ToQueryString(); // "?page=1&pageSize=50"
```

### HttpResponse

Response wrapper class.

```csharp
public class HttpResponse
{
    public HttpStatusCode StatusCode { get; set; }
    public string? Content { get; set; }
    public Dictionary<string, IEnumerable<string>>? Headers { get; set; }
    public bool IsSuccessStatusCode { get; set; }
}
```

## Testing

### MockHttpRequestResultService&lt;T&gt;

Mock service for unit testing.

```csharp
public class MockHttpRequestResultService<T> : IHttpRequestResultService
{
    public MockHttpRequestResultService(HttpResponseMessage response);
    public MockHttpRequestResultService(T responseData, HttpStatusCode statusCode = HttpStatusCode.OK);
    
    public Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(
        HttpRequestResult<T> request,
        CancellationToken cancellationToken = default);
}
```

## Concurrent Requests

### IConcurrentHttpService

Interface for concurrent request execution.

```csharp
public interface IConcurrentHttpService
{
    Task<IEnumerable<HttpRequestResult<T>>> ExecuteConcurrentRequestsAsync<T>(
        IEnumerable<HttpRequestResult<T>> requests,
        CancellationToken cancellationToken = default);
}
```

## OpenTelemetry Integration

### OpenTelemetry Extensions

```csharp
public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddWebSparkOpenTelemetry(
        this IServiceCollection services,
        Action<TracerProviderBuilder>? configure = null);
}
```

## Common Status Codes

| Code | Enum | Description |
|------|------|-------------|
| 200 | `HttpStatusCode.OK` | Success |
| 201 | `HttpStatusCode.Created` | Resource created |
| 204 | `HttpStatusCode.NoContent` | Success with no content |
| 400 | `HttpStatusCode.BadRequest` | Invalid request |
| 401 | `HttpStatusCode.Unauthorized` | Authentication required |
| 403 | `HttpStatusCode.Forbidden` | Access denied |
| 404 | `HttpStatusCode.NotFound` | Resource not found |
| 500 | `HttpStatusCode.InternalServerError` | Server error |
| 503 | `HttpStatusCode.ServiceUnavailable` | Service unavailable |

## Framework Targets

- .NET 8.0 (LTS)
- .NET 9.0

## NuGet Package

- **Package Name**: `WebSpark.HttpClientUtility`
- **Current Version**: {{ nuget.version }}
- **Downloads**: {{ nuget.displayDownloads }}

## Source Code

View the complete source code and contribute on [GitHub](https://github.com/markhazleton/WebSpark.HttpClientUtility).

## Next Steps

<div class="cta-buttons">
  <a href="{{ '/examples/' | url }}" class="button primary">View Examples</a>
  <a href="{{ '/getting-started/' | url }}" class="button secondary">Get Started</a>
</div>
