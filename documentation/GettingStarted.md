# Getting Started with WebSpark.HttpClientUtility

This guide will walk you through setting up and using WebSpark.HttpClientUtility in your .NET application.

## Installation

### Via .NET CLI
```bash
dotnet add package WebSpark.HttpClientUtility
```

### Via Package Manager Console
```powershell
Install-Package WebSpark.HttpClientUtility
```

### Via NuGet Package Manager (Visual Studio)
Search for "WebSpark.HttpClientUtility" in the NuGet Package Manager UI.

## Basic Setup

### 1. Register Services

In your `Program.cs` (or `Startup.cs` for older projects):

```csharp
using WebSpark.HttpClientUtility;

// Minimal setup - just the basics
builder.Services.AddHttpClientUtility();
```

That's it! This single line registers all necessary services.

### 2. Create a Service Class

```csharp
using WebSpark.HttpClientUtility.RequestResult;

public class TodoService
{
 private readonly IHttpRequestResultService _httpService;
    private readonly ILogger<TodoService> _logger;

    public TodoService(
        IHttpRequestResultService httpService,
    ILogger<TodoService> logger)
    {
        _httpService = httpService;
  _logger = logger;
    }

    public async Task<Todo?> GetTodoAsync(int id)
{
     var request = new HttpRequestResult<Todo>
     {
  RequestPath = $"https://jsonplaceholder.typicode.com/todos/{id}",
            RequestMethod = HttpMethod.Get
        };

        var result = await _httpService.HttpSendRequestResultAsync(request);

        if (result.IsSuccessStatusCode && result.ResponseResults != null)
     {
     _logger.LogInformation(
    "Retrieved todo {Id} in {Duration}ms [CorrelationId: {CorrelationId}]",
               id, result.RequestDurationMilliseconds, result.CorrelationId);
          
      return result.ResponseResults;
 }

        _logger.LogWarning(
      "Failed to retrieve todo {Id}: {Status} [CorrelationId: {CorrelationId}]",
            id, result.StatusCode, result.CorrelationId);

        return null;
    }
}

public class Todo
{
 public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool Completed { get; set; }
}
```

### 3. Use the Service

```csharp
// In a controller
public class TodoController : ControllerBase
{
    private readonly TodoService _todoService;

    public TodoController(TodoService todoService) => _todoService = todoService;

  [HttpGet("{id}")]
    public async Task<IActionResult> GetTodo(int id)
    {
  var todo = await _todoService.GetTodoAsync(id);
     return todo != null ? Ok(todo) : NotFound();
  }
}
```

## Configuration Options

### Basic Configuration

```csharp
builder.Services.AddHttpClientUtility(options =>
{
  options.EnableTelemetry = true;      // Default: true
    options.EnableCaching = false;        // Default: false
    options.EnableResilience = false;     // Default: false
   options.UseNewtonsoftJson = false;    // Default: false (uses System.Text.Json)
});
```

### With Caching

```csharp
builder.Services.AddHttpClientUtility(options =>
{
    options.EnableCaching = true;
});
```

### With Resilience

```csharp
builder.Services.AddHttpClientUtility(options =>
{
    options.EnableResilience = true;
    options.ResilienceOptions.MaxRetryAttempts = 3;
    options.ResilienceOptions.RetryDelay = TimeSpan.FromSeconds(1);
    options.ResilienceOptions.CircuitBreakerThreshold = 5;
    options.ResilienceOptions.CircuitBreakerDuration = TimeSpan.FromSeconds(30);
});
```

### All Features Enabled

```csharp
builder.Services.AddHttpClientUtilityWithAllFeatures();
```

## Making Requests

### GET Request

```csharp
var request = new HttpRequestResult<WeatherForecast>
{
    RequestPath = "https://api.weather.com/forecast/seattle",
 RequestMethod = HttpMethod.Get
};

var result = await _httpService.HttpSendRequestResultAsync(request);
```

### POST Request

```csharp
var request = new HttpRequestResult<CreateUserResponse>
{
    RequestPath = "https://api.example.com/users",
    RequestMethod = HttpMethod.Post,
   Payload = new CreateUserRequest 
  { 
        Name = "John Doe", 
     Email = "john@example.com" 
    }
};

var result = await _httpService.HttpSendRequestResultAsync(request);
```

### With Headers

```csharp
var request = new HttpRequestResult<ApiResponse>
{
    RequestPath = "https://api.example.com/data",
    RequestMethod = HttpMethod.Get,
    RequestHeaders = new Dictionary<string, string>
    {
        { "Authorization", "Bearer YOUR_TOKEN" },
    { "X-API-Key", "YOUR_API_KEY" }
    }
};
```

### With Caching

```csharp
var request = new HttpRequestResult<Product>
{
    RequestPath = "https://api.example.com/products/123",
    RequestMethod = HttpMethod.Get,
    CacheDurationMinutes = 10  // Cache for 10 minutes
};
```

## Handling Responses

### Check Success

```csharp
var result = await _httpService.HttpSendRequestResultAsync(request);

if (result.IsSuccessStatusCode)
{
  // Success!
    var data = result.ResponseResults;
  Console.WriteLine($"Received: {data}");
}
else
{
    // Handle error
    Console.WriteLine($"Error: {result.StatusCode}");
 foreach (var error in result.ErrorList)
    {
        Console.WriteLine($"  - {error}");
    }
}
```

### Access Metadata

```csharp
var result = await _httpService.HttpSendRequestResultAsync(request);

Console.WriteLine($"Status: {result.StatusCode}");
Console.WriteLine($"Duration: {result.RequestDurationMilliseconds}ms");
Console.WriteLine($"Correlation ID: {result.CorrelationId}");
Console.WriteLine($"Completed: {result.CompletionDate}");

// Check if from cache (when caching enabled)
if (result.RequestContext.TryGetValue("CacheHit", out var cacheHit))
{
    Console.WriteLine($"Cache Hit: {cacheHit}");
}
```

## Error Handling

The library automatically handles common HTTP errors:

```csharp
try
{
    var result = await _httpService.HttpSendRequestResultAsync(request);
    
    if (!result.IsSuccessStatusCode)
    {
      // Check specific status codes
        if (result.StatusCode == HttpStatusCode.NotFound)
        {
return NotFound();
        }
   
 if (result.StatusCode == HttpStatusCode.Unauthorized)
        {
   return Unauthorized();
        }
        
        // Log errors with correlation ID
 _logger.LogError(
  "Request failed with {StatusCode}: {Errors} [CorrelationId: {CorrelationId}]",
    result.StatusCode,
     string.Join(", ", result.ErrorList),
            result.CorrelationId);
  }
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error during HTTP request");
}
```

## Testing

The library uses interfaces, making it easy to mock in tests:

```csharp
[TestClass]
public class TodoServiceTests
{
   [TestMethod]
public async Task GetTodoAsync_ReturnsData_WhenRequestSucceeds()
    {
        // Arrange
        var mockHttpService = new Mock<IHttpRequestResultService>();
        var expectedTodo = new Todo { Id = 1, Title = "Test" };
        
    mockHttpService
         .Setup(x => x.HttpSendRequestResultAsync(
   It.IsAny<HttpRequestResult<Todo>>(),
                It.IsAny<string>(),
  It.IsAny<string>(),
       It.IsAny<int>(),
     It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HttpRequestResult<Todo>
 {
                StatusCode = HttpStatusCode.OK,
                ResponseResults = expectedTodo
            });

        var service = new TodoService(
            mockHttpService.Object,
          Mock.Of<ILogger<TodoService>>());

        // Act
        var result = await service.GetTodoAsync(1);

  // Assert
        Assert.IsNotNull(result);
      Assert.AreEqual(1, result.Id);
   }
}
```

## Next Steps

- [Configuration Guide](Configuration.md) - Detailed configuration options
- [Caching Guide](Caching.md) - Advanced caching strategies
- [Resilience Guide](Resilience.md) - Retry and circuit breaker patterns
- [API Reference](ApiReference.md) - Complete API documentation

## Common Issues

### Assembly Resolution Errors

If you see errors about missing dependencies, ensure you're targeting .NET 8 or .NET 9:

```xml
<TargetFramework>net8.0</TargetFramework>
<!-- or -->
<TargetFramework>net9.0</TargetFramework>
```

### JSON Serialization Issues

By default, the library uses `System.Text.Json`. If you need `Newtonsoft.Json`:

```csharp
builder.Services.AddHttpClientUtility(options =>
{
 options.UseNewtonsoftJson = true;
});
```

### Cache Not Working

Make sure caching is enabled:

```csharp
builder.Services.AddHttpClientUtility(options =>
{
    options.EnableCaching = true;  // Required!
});
```

And set cache duration on your requests:

```csharp
var request = new HttpRequestResult<Data>
{
    // ...
    CacheDurationMinutes = 10  // Must be > 0
};
```
