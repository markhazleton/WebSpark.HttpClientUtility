---
layout: layouts/base.njk
title: Examples
description: Real-world examples of using WebSpark.HttpClientUtility in various scenarios.
permalink: /examples/
---

# Examples

Practical examples for common scenarios.

## Basic HTTP Requests

### Simple GET Request

```csharp
var request = new HttpRequestResult<UserProfile>
{
    RequestPath = "https://api.example.com/users/123",
    RequestMethod = HttpMethod.Get
};

var result = await _httpService.HttpSendRequestResultAsync(request);

if (result.IsSuccessStatusCode)
{
    var user = result.ResponseResults;
    Console.WriteLine($"User: {user.Name}, Email: {user.Email}");
}
```

### POST Request with JSON Body

```csharp
var newUser = new CreateUserRequest
{
    Name = "Jane Doe",
    Email = "jane@example.com",
    Role = "Developer"
};

var request = new HttpRequestResult<UserResponse>
{
    RequestPath = "https://api.example.com/users",
    RequestMethod = HttpMethod.Post,
    RequestBody = newUser
};

var result = await _httpService.HttpSendRequestResultAsync(request);
Console.WriteLine($"Created user with ID: {result.ResponseResults.Id}");
```

### PUT Request (Update)

```csharp
var updateData = new UpdateUserRequest
{
    Name = "Jane Smith",
    Email = "jane.smith@example.com"
};

var request = new HttpRequestResult<UserResponse>
{
    RequestPath = "https://api.example.com/users/123",
    RequestMethod = HttpMethod.Put,
    RequestBody = updateData
};

var result = await _httpService.HttpSendRequestResultAsync(request);
```

### DELETE Request

```csharp
var request = new HttpRequestResult<object>
{
    RequestPath = "https://api.example.com/users/123",
    RequestMethod = HttpMethod.Delete
};

var result = await _httpService.HttpSendRequestResultAsync(request);

if (result.IsSuccessStatusCode)
{
    Console.WriteLine("User deleted successfully");
}
```

## Authentication Examples

### Bearer Token Authentication

```csharp
var request = new HttpRequestResult<SecureData>
{
    RequestPath = "https://api.example.com/secure/data",
    RequestMethod = HttpMethod.Get,
    AuthenticationProvider = new BearerTokenAuthenticationProvider("eyJhbGciOiJIUzI1...")
};

var result = await _httpService.HttpSendRequestResultAsync(request);
```

### Basic Authentication

```csharp
var request = new HttpRequestResult<Data>
{
    RequestPath = "https://api.example.com/data",
    RequestMethod = HttpMethod.Get,
    AuthenticationProvider = new BasicAuthenticationProvider("username", "password")
};
```

### API Key Authentication

```csharp
var request = new HttpRequestResult<Data>
{
    RequestPath = "https://api.example.com/data",
    RequestMethod = HttpMethod.Get,
    AuthenticationProvider = new ApiKeyAuthenticationProvider("X-API-Key", "your-api-key-here")
};
```

## Caching Examples

### Cache API Response

```csharp
var request = new HttpRequestResult<WeatherData>
{
    RequestPath = "https://api.weather.com/current/seattle",
    RequestMethod = HttpMethod.Get,
    CacheDurationMinutes = 30 // Cache for 30 minutes
};

// First call - hits the API
var result1 = await _httpService.HttpSendRequestResultAsync(request);

// Second call within 30 minutes - returns cached data
var result2 = await _httpService.HttpSendRequestResultAsync(request);
```

### Different Cache Durations

```csharp
// Frequently changing data - short cache
var realtimeRequest = new HttpRequestResult<StockPrice>
{
    RequestPath = "https://api.stocks.com/price/MSFT",
    CacheDurationMinutes = 1
};

// Rarely changing data - long cache
var configRequest = new HttpRequestResult<AppConfig>
{
    RequestPath = "https://api.example.com/config",
    CacheDurationMinutes = 1440 // 24 hours
};
```

## Error Handling

### Handle Different Status Codes

```csharp
var result = await _httpService.HttpSendRequestResultAsync(request);

switch (result.StatusCode)
{
    case HttpStatusCode.OK:
        var data = result.ResponseResults;
        Console.WriteLine($"Success: {data}");
        break;
    
    case HttpStatusCode.NotFound:
        Console.WriteLine("Resource not found");
        break;
    
    case HttpStatusCode.Unauthorized:
        Console.WriteLine("Authentication required");
        break;
    
    case HttpStatusCode.BadRequest:
        Console.WriteLine($"Bad request: {result.ResponseContent}");
        break;
    
    default:
        Console.WriteLine($"Error {result.StatusCode}: {result.ResponseContent}");
        break;
}
```

### Resilience with Automatic Retry

```csharp
// With resilience enabled, this will automatically retry on transient failures
builder.Services.AddHttpClientUtility(options => {
    options.EnableResilience = true;
    options.ResilienceOptions.MaxRetryAttempts = 3;
    options.ResilienceOptions.RetryDelay = TimeSpan.FromSeconds(2);
});

// Request will retry up to 3 times with 2 second delay
var result = await _httpService.HttpSendRequestResultAsync(request);
```

## Advanced Examples

### Custom Headers

```csharp
var request = new HttpRequestResult<Data>
{
    RequestPath = "https://api.example.com/data",
    RequestMethod = HttpMethod.Get,
    RequestHeaders = new Dictionary<string, string>
    {
        { "Accept", "application/json" },
        { "Accept-Language", "en-US" },
        { "X-Custom-Header", "custom-value" },
        { "User-Agent", "MyApp/1.0" }
    }
};
```

### Query String Parameters

```csharp
var queryParams = new QueryStringParametersList
{
    { "page", "1" },
    { "pageSize", "50" },
    { "sort", "name" },
    { "filter", "active" }
};

var request = new HttpRequestResult<PagedResult<User>>
{
    RequestPath = $"https://api.example.com/users{queryParams.ToQueryString()}",
    RequestMethod = HttpMethod.Get
};
```

### Correlation ID for Distributed Tracing

```csharp
var request = new HttpRequestResult<Data>
{
    RequestPath = "https://api.example.com/data",
    RequestMethod = HttpMethod.Get,
    CorrelationId = Guid.NewGuid().ToString() // Auto-generated if not provided
};

var result = await _httpService.HttpSendRequestResultAsync(request);
Console.WriteLine($"Request tracked with ID: {result.CorrelationId}");
```

### Concurrent Requests

```csharp
using WebSpark.HttpClientUtility.Concurrent;

var requests = new List<HttpRequestResult<Data>>
{
    new() { RequestPath = "https://api.example.com/data/1", RequestMethod = HttpMethod.Get },
    new() { RequestPath = "https://api.example.com/data/2", RequestMethod = HttpMethod.Get },
    new() { RequestPath = "https://api.example.com/data/3", RequestMethod = HttpMethod.Get }
};

var results = await _concurrentService.ExecuteConcurrentRequestsAsync(requests);

foreach (var result in results)
{
    Console.WriteLine($"Path: {result.RequestPath}, Success: {result.IsSuccessStatusCode}");
}
```

## Web Crawling Example

### Basic Crawler Setup

```csharp
using WebSpark.HttpClientUtility.Crawler;

var options = new CrawlerOptions
{
    BaseUrl = "https://example.com",
    MaxDepth = 3,
    MaxPages = 100,
    RespectRobotsTxt = true,
    UserAgent = "MyBot/1.0",
    DelayBetweenRequests = TimeSpan.FromMilliseconds(500)
};

var crawler = new SiteCrawler(options, _httpService, _logger);

crawler.PageCrawled += (sender, e) =>
{
    Console.WriteLine($"Crawled: {e.Url} - Status: {e.StatusCode}");
};

await crawler.CrawlAsync(cancellationToken);
```

### Crawler with Progress Updates

```csharp
var crawler = new SiteCrawler(options, _httpService, _logger);

crawler.CrawlProgress += (sender, progress) =>
{
    Console.WriteLine($"Progress: {progress.PagesProcessed}/{progress.TotalPages}");
    Console.WriteLine($"Success Rate: {progress.SuccessRate:P}");
};

await crawler.CrawlAsync(cancellationToken);
```

## Testing Examples

### Mock Successful Response

```csharp
[TestMethod]
public async Task TestSuccessfulApiCall()
{
    // Arrange
    var expectedData = new UserData { Id = 1, Name = "Test User" };
    var mockResponse = new HttpResponseMessage(HttpStatusCode.OK)
    {
        Content = new StringContent(
            JsonSerializer.Serialize(expectedData),
            Encoding.UTF8,
            "application/json"
        )
    };

    var mockService = new MockHttpRequestResultService<UserData>(mockResponse);
    var service = new MyService(mockService);

    // Act
    var result = await service.GetUserAsync(1);

    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(expectedData.Name, result.Name);
}
```

### Mock Error Response

```csharp
[TestMethod]
public async Task TestApiError()
{
    // Arrange
    var mockResponse = new HttpResponseMessage(HttpStatusCode.NotFound)
    {
        Content = new StringContent("User not found")
    };

    var mockService = new MockHttpRequestResultService<UserData>(mockResponse);
    var service = new MyService(mockService);

    // Act
    var result = await service.GetUserAsync(999);

    // Assert
    Assert.IsNull(result);
}
```

## Integration Examples

### ASP.NET Core Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IHttpRequestResultService _httpService;

    public WeatherController(IHttpRequestResultService httpService)
    {
        _httpService = httpService;
    }

    [HttpGet("{city}")]
    public async Task<IActionResult> GetWeather(string city)
    {
        var request = new HttpRequestResult<WeatherData>
        {
            RequestPath = $"https://api.weather.com/v1/current/{city}",
            RequestMethod = HttpMethod.Get,
            CacheDurationMinutes = 15
        };

        var result = await _httpService.HttpSendRequestResultAsync(request);

        if (result.IsSuccessStatusCode)
        {
            return Ok(result.ResponseResults);
        }

        return StatusCode((int)result.StatusCode, result.ResponseContent);
    }
}
```

### Background Service

```csharp
public class DataSyncService : BackgroundService
{
    private readonly IHttpRequestResultService _httpService;
    private readonly ILogger<DataSyncService> _logger;

    public DataSyncService(
        IHttpRequestResultService httpService,
        ILogger<DataSyncService> logger)
    {
        _httpService = httpService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var request = new HttpRequestResult<SyncData>
                {
                    RequestPath = "https://api.example.com/sync",
                    RequestMethod = HttpMethod.Get
                };

                var result = await _httpService.HttpSendRequestResultAsync(request);

                if (result.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Sync completed successfully");
                }
                else
                {
                    _logger.LogWarning($"Sync failed: {result.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sync error");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

## Next Steps

<div class="cta-buttons">
  <a href="{{ '/api-reference/' | url }}" class="button primary">API Reference</a>
  <a href="{{ '/features/' | url }}" class="button secondary">View All Features</a>
</div>
