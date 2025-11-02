---
layout: layouts/base.njk
title: Getting Started
description: Quick start guide for WebSpark.HttpClientUtility - from installation to your first request.
permalink: /getting-started/
---

# Getting Started

Get up and running with WebSpark.HttpClientUtility in minutes.

## Installation

Install via NuGet Package Manager:

```powershell
dotnet add package WebSpark.HttpClientUtility
```

Or via Package Manager Console:

```powershell
Install-Package WebSpark.HttpClientUtility
```

## Basic Setup

### Step 1: Register Services

In your `Program.cs` or `Startup.cs`:

```csharp
using WebSpark.HttpClientUtility;

var builder = WebApplication.CreateBuilder(args);

// Basic registration
builder.Services.AddHttpClientUtility();

var app = builder.Build();
```

### Step 2: Inject and Use

```csharp
using WebSpark.HttpClientUtility.RequestResult;

public class WeatherService
{
    private readonly IHttpRequestResultService _httpService;

    public WeatherService(IHttpRequestResultService httpService)
    {
        _httpService = httpService;
    }

    public async Task<WeatherData?> GetWeatherAsync(string city)
    {
        var request = new HttpRequestResult<WeatherData>
        {
            RequestPath = $"https://api.weather.com/v1/current/{city}",
            RequestMethod = HttpMethod.Get
        };

        var result = await _httpService.HttpSendRequestResultAsync(request);

        if (result.IsSuccessStatusCode)
        {
            return result.ResponseResults;
        }

        return null;
    }
}
```

## Enable Optional Features

### Caching

```csharp
builder.Services.AddHttpClientUtility(options => {
    options.EnableCaching = true;
});
```

Use caching in requests:

```csharp
var request = new HttpRequestResult<WeatherData>
{
    RequestPath = "https://api.weather.com/v1/current/seattle",
    RequestMethod = HttpMethod.Get,
    CacheDurationMinutes = 15 // Cache for 15 minutes
};
```

### Resilience (Retry & Circuit Breaker)

```csharp
builder.Services.AddHttpClientUtility(options => {
    options.EnableResilience = true;
    options.ResilienceOptions.MaxRetryAttempts = 3;
    options.ResilienceOptions.RetryDelay = TimeSpan.FromSeconds(1);
});
```

### Telemetry

```csharp
using WebSpark.HttpClientUtility.OpenTelemetry;

builder.Services.AddHttpClientUtility(options => {
    options.EnableTelemetry = true;
});

builder.Services.AddWebSparkOpenTelemetry(tracerBuilder => {
    tracerBuilder.AddOtlpExporter();
});
```

### All Features Enabled

Quick setup with everything enabled:

```csharp
builder.Services.AddHttpClientUtilityWithAllFeatures();
```

## Working with Requests

### POST Request with JSON

```csharp
var request = new HttpRequestResult<UserResponse>
{
    RequestPath = "https://api.example.com/users",
    RequestMethod = HttpMethod.Post,
    RequestBody = new { Name = "John Doe", Email = "john@example.com" }
};

var result = await _httpService.HttpSendRequestResultAsync(request);
```

### Adding Headers

```csharp
var request = new HttpRequestResult<Data>
{
    RequestPath = "https://api.example.com/data",
    RequestMethod = HttpMethod.Get,
    RequestHeaders = new Dictionary<string, string>
    {
        { "Accept", "application/json" },
        { "X-Custom-Header", "value" }
    }
};
```

### Authentication

```csharp
using WebSpark.HttpClientUtility.Authentication;

var request = new HttpRequestResult<Data>
{
    RequestPath = "https://api.example.com/secure/data",
    RequestMethod = HttpMethod.Get,
    AuthenticationProvider = new BearerTokenAuthenticationProvider("your-token")
};
```

## Handling Responses

### Check Status

```csharp
var result = await _httpService.HttpSendRequestResultAsync(request);

if (result.IsSuccessStatusCode)
{
    var data = result.ResponseResults;
    Console.WriteLine($"Success: {data}");
}
else
{
    Console.WriteLine($"Error {result.StatusCode}: {result.ResponseContent}");
}
```

### Access Response Details

```csharp
var result = await _httpService.HttpSendRequestResultAsync(request);

Console.WriteLine($"Status Code: {result.StatusCode}");
Console.WriteLine($"Duration: {result.RequestDuration}ms");
Console.WriteLine($"Correlation ID: {result.CorrelationId}");
Console.WriteLine($"Response Headers: {result.ResponseHeaders}");
```

## Configuration Options

### Complete Configuration Example

```csharp
builder.Services.AddHttpClientUtility(options =>
{
    // Enable features
    options.EnableCaching = true;
    options.EnableResilience = true;
    options.EnableTelemetry = true;

    // Resilience settings
    options.ResilienceOptions.MaxRetryAttempts = 5;
    options.ResilienceOptions.RetryDelay = TimeSpan.FromSeconds(2);
    options.ResilienceOptions.CircuitBreakerThreshold = 10;
    options.ResilienceOptions.CircuitBreakerDuration = TimeSpan.FromMinutes(1);

    // Cache settings are per-request via CacheDurationMinutes
});
```

## Testing Your Code

Use the mock service for unit tests:

```csharp
using WebSpark.HttpClientUtility.MockService;

[TestMethod]
public async Task TestWeatherService()
{
    // Arrange
    var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new StringContent(
            "{\"temperature\":72,\"condition\":\"Sunny\"}",
            Encoding.UTF8,
            "application/json"
        )
    };

    var mockService = new MockHttpRequestResultService<WeatherData>(mockResponse);
    var weatherService = new WeatherService(mockService);

    // Act
    var weather = await weatherService.GetWeatherAsync("seattle");

    // Assert
    Assert.IsNotNull(weather);
    Assert.AreEqual(72, weather.Temperature);
}
```

## Common Patterns

### Retry Failed Requests Automatically

Resilience is enabled - just make the request:

```csharp
// Will automatically retry on transient failures
var result = await _httpService.HttpSendRequestResultAsync(request);
```

### Cache API Responses

```csharp
// First call hits the API
var result1 = await _httpService.HttpSendRequestResultAsync(request);

// Second call within cache duration returns cached data
var result2 = await _httpService.HttpSendRequestResultAsync(request);
```

### Track Request Performance

```csharp
var result = await _httpService.HttpSendRequestResultAsync(request);
Console.WriteLine($"Request took {result.RequestDuration}ms");
```

## Next Steps

<div class="cta-buttons">
  <a href="{{ '/features/' | url }}" class="button primary">View All Features</a>
  <a href="{{ '/examples/' | url }}" class="button secondary">See More Examples</a>
  <a href="{{ '/api-reference/' | url }}" class="button secondary">API Reference</a>
</div>

## Need Help?

- 📖 [View the README](https://github.com/markhazleton/WebSpark.HttpClientUtility)
- 🐛 [Report Issues](https://github.com/markhazleton/WebSpark.HttpClientUtility/issues)
- 💬 [Ask Questions](https://github.com/markhazleton/WebSpark.HttpClientUtility/discussions)
