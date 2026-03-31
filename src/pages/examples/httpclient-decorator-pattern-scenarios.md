---
layout: layouts/base.njk
title: HttpClient Decorator Pattern Scenarios
description: Harvested resilience and concurrency examples from the legacy HttpClientDecoratorPattern repository.
permalink: /examples/httpclient-decorator-pattern-scenarios/
---

# HttpClient Decorator Pattern Scenarios

This page captures practical runtime scenarios harvested from the legacy demo and mapped to the current package-first API surface.

## Resilience Configuration with Seconds-Based Aliases

The current package supports both TimeSpan-style settings and legacy seconds-based aliases under `HttpRequestResultPollyOptions`.

```json
{
  "HttpRequestResultPollyOptions": {
    "MaxRetryAttempts": 3,
    "RetryDelaySeconds": 1,
    "CircuitBreakerThreshold": 5,
    "CircuitBreakerDurationSeconds": 30
  }
}
```

Equivalent TimeSpan form:

```json
{
  "HttpRequestResultPollyOptions": {
    "MaxRetryAttempts": 3,
    "RetryDelay": "00:00:01",
    "CircuitBreakerThreshold": 5,
    "CircuitBreakerDuration": "00:00:30"
  }
}
```

## Circuit Breaker Scenario

```csharp
builder.Services.AddHttpClientUtility(options =>
{
    options.EnableResilience = true;
    options.ResilienceOptions.MaxRetryAttempts = 3;
    options.ResilienceOptions.CircuitBreakerThreshold = 5;
    options.ResilienceOptions.CircuitBreakerDuration = TimeSpan.FromSeconds(30);
});

var result = await httpService.HttpSendRequestResultAsync(new HttpRequestResult<StatusPayload>
{
    RequestPath = "https://api.example.com/status",
    RequestMethod = HttpMethod.Get
});

if (!result.IsSuccessStatusCode)
{
    logger.LogWarning("Request failed with {StatusCode}; retries applied by Polly", result.StatusCode);
}
```

## Concurrent Calls Scenario

```csharp
using WebSpark.HttpClientUtility.Concurrent;

var requests = new List<HttpRequestResult<ApiResult>>
{
    new() { RequestPath = "https://api.example.com/a", RequestMethod = HttpMethod.Get },
    new() { RequestPath = "https://api.example.com/b", RequestMethod = HttpMethod.Get },
    new() { RequestPath = "https://api.example.com/c", RequestMethod = HttpMethod.Get }
};

var concurrent = serviceProvider.GetRequiredService<IConcurrentHttpRequestResultService>();
var responses = await concurrent.ProcessRequestListAsync(requests, maxConcurrentRequests: 3);
```

## Notes

- Keep decorator order unchanged: base -> cache -> polly -> telemetry.
- Prefer one-line DI registration (`AddHttpClientUtility`) and opt in to features through options.
- Use automated tests when changing resilience settings to preserve compatibility.