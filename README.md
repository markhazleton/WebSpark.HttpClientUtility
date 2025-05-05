# WebSpark.HttpClientUtility: Robust & Simplified .NET HttpClient Wrapper

![WebSpark.HttpClientUtility Logo](https://raw.githubusercontent.com/MarkHazleton/HttpClientUtility/main/WebSpark.HttpClientUtility/images/icon.png)

[![NuGet Version](https://img.shields.io/nuget/v/WebSpark.HttpClientUtility.svg)](https://www.nuget.org/packages/WebSpark.HttpClientUtility/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/WebSpark.HttpClientUtility.svg)](https://www.nuget.org/packages/WebSpark.HttpClientUtility/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Build Status](https://github.com/MarkHazleton/HttpClientUtility/actions/workflows/publish-nuget.yml/badge.svg)](https://github.com/MarkHazleton/HttpClientUtility/actions/workflows/publish-nuget.yml)

## 📦 Quick Links

- **[NuGet Package](https://www.nuget.org/packages/WebSpark.HttpClientUtility)**: Download and install the package
- **[GitHub Repository](https://github.com/markhazleton/httpclientutility)**: Source code, issue tracking, and contributions
- **[Changelog](https://github.com/markhazleton/httpclientutility/blob/main/CHANGELOG.md)**: Version history and updates

**Tired of boilerplate code and manual handling of resilience, caching, and telemetry for `HttpClient` in your .NET applications?** WebSpark.HttpClientUtility is a powerful yet easy-to-use library designed to streamline your HTTP interactions, making them more robust, observable, and maintainable. Build reliable API clients faster with built-in support for Polly resilience, response caching, concurrent requests, and standardized logging.

This library provides a comprehensive solution for common challenges faced when working with `HttpClient` in modern .NET (including .NET 8, .NET 9 and ASP.NET Core) applications.

## 📑 Table of Contents

- [Why Choose WebSpark.HttpClientUtility?](#why-choose-websparkhttpclientutility)
- [Key Features](#key-features)
- [Installation](#installation)
- [Getting Started](#getting-started)
  - [Dependency Injection Setup](#1-dependency-injection-setup)
  - [Basic Usage](#2-basic-usage-ihttprequestresultservice)
  - [Using Resilience (Polly)](#3-using-resilience-polly)
  - [Using Caching](#4-using-caching)
  - [Using Concurrent HTTP Requests](#5-using-concurrent-http-requests)
  - [Using FireAndForgetUtility](#6-using-the-fireandforgetutility-for-background-tasks)
  - [Using CurlCommandSaver](#7-using-curlcommandsaver-for-debugging)
  - [Azure Integration Example](#8-azure-integration-example)
  - [Using Web Crawler](#9-using-web-crawler)
- [Ideal Use Cases](#ideal-use-cases)
- [Contributing](#contributing)
- [License](#license)

## Why Choose WebSpark.HttpClientUtility?

- **Reduce Boilerplate:** Abstract away common patterns for request setup, response handling, serialization, and error management. Focus on your application logic, not HTTP plumbing.
- **Enhance Resilience:** Easily integrate industry-standard Polly policies (like retries and circuit breakers) using simple decorators, making your application more fault-tolerant.
- **Improve Performance:** Implement response caching with minimal effort via the caching decorator to reduce latency and load on external services.
- **Boost Observability:** Gain crucial insights with built-in telemetry (request timing) and structured logging, featuring correlation IDs for easy request tracing.
- **Simplify Concurrency:** Efficiently manage and execute multiple outbound HTTP requests in parallel with the dedicated concurrent processor.
- **Web Crawling Capabilities:** Build powerful web crawlers with configurable options, sitemap generation, robots.txt compliance, and SignalR integration for real-time progress updates.
- **Promote Best Practices:** Encourages a structured, testable, and maintainable approach to HTTP communication in .NET, aligning with modern software design principles.
- **Flexible & Extensible:** Designed with interfaces and decorators for easy customization and extension.

## Key Features

- **Simplified HTTP Client Operations:** Intuitive `IHttpClientService` and `HttpRequestResultService` for clean GET, POST, PUT, DELETE requests.
- **Structured & Informative Results:** `HttpRequestResult<T>` encapsulates response data, status codes, timing, errors, and correlation IDs in a single, easy-to-use object.
- **Seamless Polly Integration:** Add resilience patterns (retries, circuit breakers) via the `HttpRequestResultServicePolly` decorator without complex manual setup.
- **Effortless Response Caching:** Decorate with `HttpRequestResultServiceCache` for automatic in-memory caching of HTTP responses based on configurable durations.
- **Automatic Basic Telemetry:** `HttpClientServiceTelemetry` and `HttpRequestResultServiceTelemetry` wrappers capture request duration out-of-the-box for performance monitoring.
- **Efficient Concurrent Processing:** `HttpClientConcurrentProcessor` utility for managing and executing parallel HTTP requests effectively.
- **Web Crawling Engine:** `ISiteCrawler` interface with implementations including `SiteCrawler` and `SimpleSiteCrawler` for efficient crawling of websites, sitemap generation, and more.
- **Standardized & Rich Logging:** Utilities (`LoggingUtility`, `ErrorHandlingUtility`) provide correlation IDs, automatic URL sanitization (for security), and structured context for better diagnostics and easier debugging in logs.
- **Flexible JSON Serialization:** Choose between `System.Text.Json` (`SystemJsonStringConverter`) and `Newtonsoft.Json` (`NewtonsoftJsonStringConverter`) via the `IStringConverter` abstraction.
- **Safe Background Tasks:** `FireAndForgetUtility` for safely executing non-critical background tasks (like logging or notifications) without awaiting them and potentially blocking request threads.
- **Easy Debugging:** Option to save requests as cURL commands using `CurlCommandSaver` for simple reproduction and testing outside your application.

## Installation

Install the package from NuGet:

```powershell
Install-Package WebSpark.HttpClientUtility
```

Or via the .NET CLI:

```bash
dotnet add package WebSpark.HttpClientUtility
```

## Getting Started

### 1. Dependency Injection Setup

Register the necessary services in your `Program.cs` (minimal API or ASP.NET Core 6+) or `Startup.cs` (`ConfigureServices` method).

```csharp
using Microsoft.Extensions.Caching.Memory; // Required for caching
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebSpark.HttpClientUtility.ClientService;
using WebSpark.HttpClientUtility.MemoryCache; // If using MemoryCacheManager
using WebSpark.HttpClientUtility.RequestResult;
using WebSpark.HttpClientUtility.StringConverter;
using WebSpark.HttpClientUtility.FireAndForget; // If using FireAndForgetUtility
using WebSpark.HttpClientUtility.Concurrent; // If using concurrent processor

// --- Inside your service configuration ---

// 1. Add HttpClientFactory (essential for managing HttpClient instances)
services.AddHttpClient();

// 2. Register the core HttpClient service and its dependencies
// Choose your preferred JSON serializer
services.AddSingleton<IStringConverter, SystemJsonStringConverter>();
// Or use Newtonsoft.Json:
// services.AddSingleton<IStringConverter, NewtonsoftJsonStringConverter>();

// Register the basic service implementation
services.AddScoped<IHttpClientService, HttpClientService>();

// 3. Register the HttpRequestResult service stack (using decorators)
services.AddScoped<HttpRequestResultService>(); // Base service - always register

// Register the final IHttpRequestResultService using a factory to build the decorator chain
services.AddScoped<IHttpRequestResultService>(provider =>
{
    // Start with the base service instance
    IHttpRequestResultService service = provider.GetRequiredService<HttpRequestResultService>();

    // --- Chain the Optional Decorators (Order Matters!) ---
    // The order typically goes: Base -> Cache -> Polly -> Telemetry

    // Add Caching (Requires IMemoryCache registration)
    // Uncomment the next lines if you need caching
    // services.AddMemoryCache(); // Ensure MemoryCache is registered BEFORE this factory
    // service = new HttpRequestResultServiceCache(
    //     provider.GetRequiredService<ILogger<HttpRequestResultServiceCache>>(),
    //     service,
    //     provider.GetRequiredService<IMemoryCache>() // Get registered IMemoryCache
    // );

    // Add Polly Resilience (Requires options configuration)
    // Uncomment the next lines if you need Polly resilience
    // var pollyOptions = new HttpRequestResultPollyOptions
    // {
    //     MaxRetryAttempts = 3,
    //     RetryDelay = TimeSpan.FromSeconds(1),
    //     EnableCircuitBreaker = true,
    //     CircuitBreakerThreshold = 5,
    //     CircuitBreakerDuration = TimeSpan.FromSeconds(30)
    // }; // Configure as needed
    // service = new HttpRequestResultServicePolly(
    //     provider.GetRequiredService<ILogger<HttpRequestResultServicePolly>>(),
    //     service,
    //     pollyOptions
    // );

    // Add Telemetry (Usually the outermost layer)
    service = new HttpRequestResultServiceTelemetry(
        provider.GetRequiredService<ILogger<HttpRequestResultServiceTelemetry>>(),
        service
    );

    // Return the fully decorated service instance
    return service;
});

// 4. --- Optional Utilities ---
// services.AddSingleton<IMemoryCacheManager, MemoryCacheManager>(); // If using MemoryCacheManager helper
// services.AddSingleton<FireAndForgetUtility>(); // If using FireAndForgetUtility
// services.AddScoped<HttpClientConcurrentProcessor>(); // If using concurrent processor

// Add other application services...

// --- End of service configuration ---
```

- **Important:** Ensure you register `services.AddHttpClient()` and `services.AddMemoryCache()` (if using caching) *before* the factory that registers `IHttpRequestResultService`.
- Adjust the registration lifetimes (`Scoped`, `Singleton`, `Transient`) based on your application's needs. `Scoped` is generally a good default for services involved in a web request.

### 2. Basic Usage (`IHttpRequestResultService`)

Inject `IHttpRequestResultService` into your service, controller, or component.

```csharp
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebSpark.HttpClientUtility.RequestResult;

public class MyApiService
{
    private readonly IHttpRequestResultService _requestService;
    private readonly ILogger<MyApiService> _logger;

    // Inject the service via constructor
    public MyApiService(IHttpRequestResultService requestService, ILogger<MyApiService> logger)
    {
        _requestService = requestService;
        _logger = logger;
    }

    public async Task<MyData?> GetDataAsync(string id)
    {
        // Define the request details using HttpRequestResult<TResponse>
        var request = new HttpRequestResult<MyData> // Specify the expected response type
        {
            RequestPath = $"https://api.example.com/data/{id}", // The full URL
            RequestMethod = HttpMethod.Get,
            // Optional: Set CacheDurationMinutes if the caching decorator is enabled
            // CacheDurationMinutes = 5,
            // Optional: Add custom headers if needed
            // RequestHeaders = new Dictionary<string, string> { { "X-API-Key", "your-key" } }
        };

        _logger.LogInformation("Attempting to get data for ID: {Id}", id);

        // Send the request using the service
        // Resilience, Caching, Telemetry are handled automatically by the decorators (if enabled)
        var result = await _requestService.HttpSendRequestResultAsync(request);

        // Check the outcome using the properties of the result object
        if (result.IsSuccessStatusCode && result.ResponseResults != null)
        {
            _logger.LogInformation("Successfully retrieved data for ID: {Id}. CorrelationId: {CorrelationId}, Duration: {DurationMs}ms",
                id, result.CorrelationId, result.RequestDurationMilliseconds);
            return result.ResponseResults; // Access the deserialized data
        }
        else
        {
            // Log detailed error information provided by the result object
            _logger.LogError("Failed to retrieve data for ID: {Id}. Status: {StatusCode}, Errors: [{Errors}], CorrelationId: {CorrelationId}, Duration: {DurationMs}ms",
                id, result.StatusCode, string.Join(", ", result.ErrorList), result.CorrelationId, result.RequestDurationMilliseconds);
            // Handle the error appropriately (e.g., return null, throw exception)
            return null;
        }
    }

    public async Task<bool> PostDataAsync(MyData data)
    {
        var request = new HttpRequestResult<string> // Expecting a string response (e.g., confirmation ID)
        {
            RequestPath = "https://api.example.com/data",
            RequestMethod = HttpMethod.Post,
            // The 'Payload' object will be automatically serialized to JSON (using the registered IStringConverter)
            // and sent as the request body.
            Payload = data
        };

        _logger.LogInformation("Attempting to post data: {@Data}", data);

        var result = await _requestService.HttpSendRequestResultAsync(request);

        if (result.IsSuccessStatusCode)
        {
            _logger.LogInformation("Successfully posted data. Response: {Response}, CorrelationId: {CorrelationId}, Duration: {DurationMs}ms",
                result.ResponseResults, result.CorrelationId, result.RequestDurationMilliseconds);
            return true;
        }
        else
        {
            _logger.LogError("Failed to post data. Status: {StatusCode}, Errors: [{Errors}], CorrelationId: {CorrelationId}, Duration: {DurationMs}ms",
                result.StatusCode, string.Join(", ", result.ErrorList), result.CorrelationId, result.RequestDurationMilliseconds);
            return false;
        }
    }
}

// Example Data Transfer Object (DTO)
public class MyData
{
    public int Id { get; set; }
    public string? Name { get; set; }
    // Add other properties as needed
}
```

### 3. Using Resilience (Polly)

If you registered the `HttpRequestResultServicePolly` decorator (as shown in the DI setup) and configured `HttpRequestResultPollyOptions`, the retry and/or circuit breaker policies will be automatically applied whenever you call `_requestService.HttpSendRequestResultAsync`. No extra code is needed in your service method!

Here's a complete example configuring and using Polly resilience:

```csharp
// In Program.cs or Startup.cs
services.AddScoped<IHttpRequestResultService>(provider =>
{
    IHttpRequestResultService service = provider.GetRequiredService<HttpRequestResultService>();
    
    // Configure Polly options with progressive retry delay and circuit breaker
    var pollyOptions = new HttpRequestResultPollyOptions
    {
        MaxRetryAttempts = 3,
        RetryDelay = TimeSpan.FromSeconds(1),      // Base delay for first retry
        RetryStrategy = RetryStrategy.Exponential, // Each subsequent retry will wait longer
        EnableCircuitBreaker = true,
        CircuitBreakerThreshold = 5,               // Open circuit after 5 consecutive failures
        CircuitBreakerDuration = TimeSpan.FromSeconds(30)  // Keep circuit open for 30 seconds
    };
    
    // Add the Polly decorator with the configured options
    service = new HttpRequestResultServicePolly(
        provider.GetRequiredService<ILogger<HttpRequestResultServicePolly>>(),
        service,
        pollyOptions
    );
    
    return service;
});

// In your service class:
public async Task<WeatherForecast?> GetWeatherWithResilience(string city)
{
    var request = new HttpRequestResult<WeatherForecast>
    {
        RequestPath = $"https://api.weather.example.com/forecast/{city}",
        RequestMethod = HttpMethod.Get,
        // You can check if the circuit is open before making a request
        IsDebugEnabled = true // Enable detailed logging of retries and circuit breaker events
    };
    
    var result = await _requestService.HttpSendRequestResultAsync(request);
    
    // The result includes information about retries and circuit breaker state
    if (result.IsSuccessStatusCode)
    {
        _logger.LogInformation(
            "Weather data retrieved after {RetryCount} retries. Circuit state: {CircuitState}",
            result.RequestContext.TryGetValue("RetryCount", out var retryCount) ? retryCount : 0,
            result.RequestContext.TryGetValue("FinalCircuitState", out var state) ? state : "Unknown");
        
        return result.ResponseResults;
    }
    
    return null;
}
```

### 4. Using Caching

Configure and use the caching decorator to avoid redundant API calls and improve performance:

```csharp
// In Program.cs or Startup.cs
services.AddMemoryCache(); // Register IMemoryCache

services.AddScoped<IHttpRequestResultService>(provider =>
{
    IHttpRequestResultService service = provider.GetRequiredService<HttpRequestResultService>();
    
    // Add caching decorator
    service = new HttpRequestResultServiceCache(
        provider.GetRequiredService<ILogger<HttpRequestResultServiceCache>>(),
        service,
        provider.GetRequiredService<IMemoryCache>()
    );
    
    return service;
});

// In your service class:
public async Task<ProductDetails?> GetProductDetailsWithCaching(string productId)
{
    var request = new HttpRequestResult<ProductDetails>
    {
        RequestPath = $"https://api.example.com/products/{productId}",
        RequestMethod = HttpMethod.Get,
        CacheDurationMinutes = 15 // Cache product details for 15 minutes
    };
    
    var result = await _requestService.HttpSendRequestResultAsync(request);
    
    if (result.IsSuccessStatusCode)
    {
        // Check if this was a cache hit
        bool isCacheHit = result.RequestContext.TryGetValue("CacheHit", out var cacheHit) && 
                          cacheHit is bool hitBool && hitBool;
        
        string source = isCacheHit ? "cache" : "API";
        _logger.LogInformation("Product details retrieved from {Source}", source);
        
        if (isCacheHit && result.RequestContext.TryGetValue("CacheAge", out var age))
        {
            _logger.LogDebug("Cache entry age: {Age}", age);
        }
        
        return result.ResponseResults;
    }
    
    return null;
}
```

### 5. Using Concurrent HTTP Requests

Process multiple HTTP requests in parallel with controlled concurrency:

```csharp
// In Program.cs or Startup.cs
services.AddScoped<HttpClientConcurrentProcessor>();

// In your service class:
public class ProductService
{
    private readonly HttpClientConcurrentProcessor _concurrentProcessor;
    private readonly ILogger<ProductService> _logger;
    
    public ProductService(
        HttpClientConcurrentProcessor concurrentProcessor,
        ILogger<ProductService> logger)
    {
        _concurrentProcessor = concurrentProcessor;
        _logger = logger;
    }
    
    public async Task<IEnumerable<ProductPrice>> GetPricesForProductsAsync(
        IEnumerable<string> productIds,
        CancellationToken cancellationToken = default)
    {
        // Configure the concurrent processor
        _concurrentProcessor.MaxTaskCount = productIds.Count();
        _concurrentProcessor.MaxDegreeOfParallelism = 5; // Process 5 requests at a time
        
        // Create a factory function for generating the request tasks
        Func<int, HttpClientConcurrentModel> taskFactory = taskId =>
        {
            string productId = productIds.ElementAt(taskId - 1); // taskId is 1-based
            return new HttpClientConcurrentModel(
                taskId,
                $"https://api.example.com/products/{productId}/price"
            );
        };
        
        // Set the task factory and start processing
        _concurrentProcessor.SetTaskDataFactory(taskFactory);
        var results = await _concurrentProcessor.ProcessAllAsync(cancellationToken);
        
        // Transform the results
        var prices = new List<ProductPrice>();
        foreach (var result in results)
        {
            if (result.StatusCall.IsSuccessStatusCode && result.StatusCall.ResponseResults != null)
            {
                prices.Add(new ProductPrice
                {
                    ProductId = productIds.ElementAt(result.TaskId - 1),
                    Price = result.StatusCall.ResponseResults.Price,
                    Currency = result.StatusCall.ResponseResults.Currency
                });
                
                _logger.LogInformation(
                    "Fetched price for product {ProductId} in {Duration}ms",
                    productIds.ElementAt(result.TaskId - 1),
                    result.DurationMS);
            }
            else
            {
                _logger.LogWarning(
                    "Failed to fetch price for product {ProductId}: {Status} {Error}",
                    productIds.ElementAt(result.TaskId - 1),
                    result.StatusCall.StatusCode,
                    string.Join(", ", result.StatusCall.ErrorList));
            }
        }
        
        return prices;
    }
}

public class ProductPrice
{
    public string ProductId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
}
```

### 6. Using the FireAndForgetUtility for Background Tasks

Execute non-critical operations without blocking the main request thread:

```csharp
// In Program.cs or Startup.cs
services.AddSingleton<FireAndForgetUtility>();

// In your service class:
public class NotificationService
{
    private readonly IHttpRequestResultService _requestService;
    private readonly FireAndForgetUtility _fireAndForget;
    private readonly ILogger<NotificationService> _logger;
    
    public NotificationService(
        IHttpRequestResultService requestService,
        FireAndForgetUtility fireAndForget,
        ILogger<NotificationService> logger)
    {
        _requestService = requestService;
        _fireAndForget = fireAndForget;
        _logger = logger;
    }
    
    public void SendNotificationInBackground(string userId, string message)
    {
        // Define the task that will run in the background
        async Task SendNotificationAsync()
        {
            try
            {
                var notification = new NotificationModel
                {
                    UserId = userId,
                    Message = message,
                    Timestamp = DateTime.UtcNow
                };
                
                var request = new HttpRequestResult<string>
                {
                    RequestPath = "https://api.notifications.example.com/send",
                    RequestMethod = HttpMethod.Post,
                    Payload = notification
                };
                
                var result = await _requestService.HttpSendRequestResultAsync(request);
                
                if (!result.IsSuccessStatusCode)
                {
                    _logger.LogWarning(
                        "Failed to send notification to user {UserId}: {Status}",
                        userId,
                        result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                // The FireAndForgetUtility will already log this exception,
                // but you can add additional error handling here if needed
            }
        }
        
        // Fire and forget the notification task
        _fireAndForget.SafeFireAndForget(
            SendNotificationAsync(),
            $"Send notification to user {userId}");
        
        _logger.LogInformation(
            "Notification to user {UserId} queued for background delivery",
            userId);
    }
}

public class NotificationModel
{
    public string UserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
```

### 7. Using CurlCommandSaver for Debugging

Generate cURL commands for requests to assist with debugging:

```csharp
// In your service class:
public class ApiDebugService
{
    private readonly IHttpRequestResultService _requestService;
    private readonly ILogger<ApiDebugService> _logger;
    
    public ApiDebugService(
        IHttpRequestResultService requestService,
        ILogger<ApiDebugService> logger)
    {
        _requestService = requestService;
        _logger = logger;
    }
    
    public async Task<ProductDetails?> GetProductWithCurlDebug(string productId)
    {
        var request = new HttpRequestResult<ProductDetails>
        {
            RequestPath = $"https://api.example.com/products/{productId}",
            RequestMethod = HttpMethod.Get,
            RequestHeaders = new Dictionary<string, string> 
            { 
                { "X-API-Key", "your-api-key" },
                { "Accept", "application/json" }
            }
        };
        
        // Generate and save curl command before making the request
        var curlCommand = CurlCommandSaver.GenerateCurlCommand(
            request.RequestPath,
            request.RequestMethod,
            request.RequestHeaders);
        
        // Save to a temporary file for debugging
        string debugFilePath = Path.Combine(Path.GetTempPath(), $"curl-debug-{Guid.NewGuid()}.txt");
        await File.WriteAllTextAsync(debugFilePath, curlCommand);
        
        _logger.LogDebug("cURL command for debugging saved to {FilePath}", debugFilePath);
        
        // Make the actual request
        var result = await _requestService.HttpSendRequestResultAsync(request);
        
        // Add the cURL command to the result context for reference
        result.RequestContext["CurlCommand"] = curlCommand;
        
        if (result.IsSuccessStatusCode)
        {
            return result.ResponseResults;
        }
        else
        {
            _logger.LogError(
                "Request failed. For debugging, you can use the cURL command saved at {FilePath}",
                debugFilePath);
            return null;
        }
    }
}
```

### 8. Azure Integration Example

When working with Azure services, you can leverage the library's features for resilience and telemetry:

```csharp
using Azure.Identity;
using WebSpark.HttpClientUtility.RequestResult;

public class AzureStorageService
{
    private readonly IHttpRequestResultService _requestService;
    private readonly ILogger<AzureStorageService> _logger;
    private readonly string _storageAccountName;
    
    public AzureStorageService(
        IHttpRequestResultService requestService,
        ILogger<AzureStorageService> logger,
        IConfiguration configuration)
    {
        _requestService = requestService;
        _logger = logger;
        _storageAccountName = configuration["Azure:StorageAccountName"];
    }
    
    public async Task<IEnumerable<BlobMetadata>> ListBlobsAsync(string containerName)
    {
        // Get token for Azure Storage
        var credential = new DefaultAzureCredential();
        var token = await credential.GetTokenAsync(
            new Azure.Core.TokenRequestContext(new[] { "https://storage.azure.com/.default" }));
        
        // Prepare the request with proper auth headers
        var request = new HttpRequestResult<BlobListResponse>
        {
            RequestPath = $"https://{_storageAccountName}.blob.core.windows.net/{containerName}?restype=container&comp=list",
            RequestMethod = HttpMethod.Get,
            RequestHeaders = new Dictionary<string, string>
            {
                { "Authorization", $"Bearer {token.Token}" },
                { "x-ms-version", "2020-10-02" }
            },
            CacheDurationMinutes = 5,  // Cache for 5 minutes
            Retries = 3  // Retry up to 3 times on failure
        };
        
        // Execute the request with built-in resilience
        var result = await _requestService.HttpSendRequestResultAsync(request);
        
        if (result.IsSuccessStatusCode && result.ResponseResults?.Blobs?.Items != null)
        {
            return result.ResponseResults.Blobs.Items;
        }
        
        _logger.LogError(
            "Failed to list blobs in container {Container}: {Status} {Error}",
            containerName,
            result.StatusCode,
            string.Join(", ", result.ErrorList));
            
        return Array.Empty<BlobMetadata>();
    }
}

// Response models
public class BlobListResponse
{
    public BlobItems? Blobs { get; set; }
}

public class BlobItems
{
    public List<BlobMetadata> Items { get; set; } = new();
}

public class BlobMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ContentType { get; set; } = string.Empty;
}
```

### 9. Using Web Crawler

The library includes a powerful web crawler with configurable options, real-time updates via SignalR, and sitemap generation:

```csharp
// In Program.cs or Startup.cs
services.AddHttpClient();
services.AddMemoryCache();
services.AddSingleton<IStringConverter, SystemJsonStringConverter>();

// Register crawler services
services.AddHttpClientCrawler();

// In your service class:
public class WebCrawlerService
{
    private readonly ISiteCrawler _crawler;
    private readonly ILogger<WebCrawlerService> _logger;
    
    public WebCrawlerService(
        ISiteCrawler crawler,
        ILogger<WebCrawlerService> logger)
    {
        _crawler = crawler;
        _logger = logger;
    }
    
    public async Task<CrawlDomainViewModel> CrawlWebsiteAsync(
        string startUrl,
        int maxPages = 100,
        int maxDepth = 3,
        bool respectRobotsTxt = true,
        CancellationToken cancellationToken = default)
    {
        // Configure crawler options
        var options = new CrawlerOptions
        {
            MaxPages = maxPages,
            MaxDepth = maxDepth,
            RespectRobotsTxt = respectRobotsTxt,
            RequestDelayMs = 200, // Be polite with 200ms between requests
            UserAgent = "WebSpark.HttpClientUtility.Crawler/1.0.4 (+https://yoursite.com/bot)",
            MaxConcurrentRequests = 5,
            FollowExternalLinks = false, // Only crawl links on the same domain
            TimeoutSeconds = 30,
            ValidateHtml = true,
            GenerateSitemap = true
        };
        
        _logger.LogInformation(
            "Starting crawl of {Url} with max {MaxPages} pages and depth {MaxDepth}",
            startUrl, maxPages, maxDepth);
        
        var result = await _crawler.CrawlAsync(startUrl, options, cancellationToken);
        
        _logger.LogInformation(
            "Crawl completed: {PageCount} pages crawled. Sitemap generated with {SitemapSize} bytes",
            result.CrawlResults.Count,
            result.Sitemap?.Length ?? 0);
            
        return result;
    }
    
    public string GetSitemapUrlsAsText(CrawlDomainViewModel crawlResult)
    {
        if (string.IsNullOrEmpty(crawlResult.Sitemap))
        {
            return "No sitemap generated";
        }
        
        // Extract URLs from the sitemap XML
        var document = new XmlDocument();
        document.LoadXml(crawlResult.Sitemap);
        
        var urlNodes = document.SelectNodes("//url/loc");
        if (urlNodes == null)
        {
            return "No URLs found in sitemap";
        }
        
        var urls = new StringBuilder();
        foreach (XmlNode node in urlNodes)
        {
            urls.AppendLine(node.InnerText);
        }
        
        return urls.ToString();
    }
}
```

The `SimpleSiteCrawler` implementation offers additional features:

```csharp
// In Program.cs or Startup.cs
services.AddHttpClient();
services.AddScoped<SimpleSiteCrawler>();

// In your service class:
public class AdvancedCrawlerService
{
    private readonly SimpleSiteCrawler _crawler;
    private readonly ILogger<AdvancedCrawlerService> _logger;
    
    public AdvancedCrawlerService(
        SimpleSiteCrawler crawler,
        ILogger<AdvancedCrawlerService> logger)
    {
        _crawler = crawler;
        _logger = logger;
    }
    
    public async Task<string> ArchiveWebsiteAsync(
        string startUrl,
        string outputDirectory,
        CancellationToken cancellationToken = default)
    {
        var options = new CrawlerOptions
        {
            MaxPages = 1000,
            MaxDepth = 5,
            RespectRobotsTxt = true,
            SavePagesToDisk = true, // Enable saving pages to disk
            OutputDirectory = outputDirectory,
            AdaptiveRateLimit = true, // Automatically adjust request rate based on server response
            OptimizeMemoryUsage = true, // Enable for large crawls
            IncludePatterns = new List<string> { @"\.html$", @"\.aspx$", @"/$" }, // Only HTML pages
            ExcludePatterns = new List<string> { @"/login", @"/logout", @"/admin" } // Skip sensitive areas
        };
        
        _logger.LogInformation(
            "Starting website archival of {Url} to {Directory}",
            startUrl, outputDirectory);
            
        var result = await _crawler.CrawlAsync(startUrl, options, cancellationToken);
        
        // Extract performance metrics
        var successCount = result.CrawlResults.Count(r => r.StatusCode == HttpStatusCode.OK);
        var errorCount = result.CrawlResults.Count - successCount;
        
        _logger.LogInformation(
            "Archival completed: {SuccessCount} pages saved, {ErrorCount} errors",
            successCount, errorCount);
            
        return Path.Combine(outputDirectory, "index.html");
    }
}
```

## Ideal Use Cases

- Building robust API clients for internal or external services.
- Simplifying HTTP interactions in microservices.
- Adding resilience and caching to existing applications with minimal refactoring.
- Creating web crawlers for content indexing, site analysis, or data gathering.
- Generating sitemaps for SEO optimization.
- Any .NET application that needs to make reliable and observable HTTP calls.

## Contributing

Contributions are welcome! If you find a bug, have a feature request, or want to improve the library, please feel free to:

1. Check for existing issues or open a new issue.
2. Fork the repository.
3. Create a new branch (`git checkout -b feature/your-feature-name`).
4. Make your changes and add tests if applicable.
5. Ensure the code builds and tests pass.
6. Commit your changes (`git commit -am 'feat: Add some amazing feature'`).
7. Push to the branch (`git push origin feature/your-feature-name`).
8. Create a new Pull Request against the `main` branch.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
