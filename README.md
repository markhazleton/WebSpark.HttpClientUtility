# WebSpark.HttpClientUtility: Robust & Simplified .NET HttpClient Wrapper

[![NuGet Version](https://img.shields.io/nuget/v/WebSpark.HttpClientUtility.svg)](https://www.nuget.org/packages/WebSpark.HttpClientUtility/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
<!-- Add build status badge once CI/CD is set up -->
<!-- [![Build Status](https://dev.azure.com/your-org/your-project/_apis/build/status/your-pipeline?branchName=main)](https://dev.azure.com/your-org/your-project/_build/latest?definitionId=your-pipeline&branchName=main) -->

**Tired of boilerplate code and manual handling of resilience, caching, and telemetry for `HttpClient` in your .NET applications?** WebSpark.HttpClientUtility is a powerful yet easy-to-use library designed to streamline your HTTP interactions, making them more robust, observable, and maintainable. Build reliable API clients faster with built-in support for Polly resilience, response caching, concurrent requests, and standardized logging.

This library provides a comprehensive solution for common challenges faced when working with `HttpClient` in modern .NET (including .NET 8, .NET 9 and ASP.NET Core) applications.

## Why Choose WebSpark.HttpClientUtility?

* **Reduce Boilerplate:** Abstract away common patterns for request setup, response handling, serialization, and error management. Focus on your application logic, not HTTP plumbing.
* **Enhance Resilience:** Easily integrate industry-standard Polly policies (like retries and circuit breakers) using simple decorators, making your application more fault-tolerant.
* **Improve Performance:** Implement response caching with minimal effort via the caching decorator to reduce latency and load on external services.
* **Boost Observability:** Gain crucial insights with built-in telemetry (request timing) and structured logging, featuring correlation IDs for easy request tracing.
* **Simplify Concurrency:** Efficiently manage and execute multiple outbound HTTP requests in parallel with the dedicated concurrent processor.
* **Promote Best Practices:** Encourages a structured, testable, and maintainable approach to HTTP communication in .NET, aligning with modern software design principles.
* **Flexible & Extensible:** Designed with interfaces and decorators for easy customization and extension.

## Key Features

* **Simplified HTTP Client Operations:** Intuitive `IHttpClientService` and `HttpRequestResultService` for clean GET, POST, PUT, DELETE requests.
* **Structured & Informative Results:** `HttpRequestResult<T>` encapsulates response data, status codes, timing, errors, and correlation IDs in a single, easy-to-use object.
* **Seamless Polly Integration:** Add resilience patterns (retries, circuit breakers) via the `HttpRequestResultServicePolly` decorator without complex manual setup.
* **Effortless Response Caching:** Decorate with `HttpRequestResultServiceCache` for automatic in-memory caching of HTTP responses based on configurable durations.
* **Automatic Basic Telemetry:** `HttpClientServiceTelemetry` and `HttpRequestResultServiceTelemetry` wrappers capture request duration out-of-the-box for performance monitoring.
* **Efficient Concurrent Processing:** `HttpClientConcurrentProcessor` utility for managing and executing parallel HTTP requests effectively.
* **Standardized & Rich Logging:** Utilities (`LoggingUtility`, `ErrorHandlingUtility`) provide correlation IDs, automatic URL sanitization (for security), and structured context for better diagnostics and easier debugging in logs.
* **Flexible JSON Serialization:** Choose between `System.Text.Json` (`SystemJsonStringConverter`) and `Newtonsoft.Json` (`NewtonsoftJsonStringConverter`) via the `IStringConverter` abstraction.
* **Safe Background Tasks:** `FireAndForgetUtility` for safely executing non-critical background tasks (like logging or notifications) without awaiting them and potentially blocking request threads.
* **Easy Debugging:** Option to save requests as cURL commands using `CurlCommandSaver` for simple reproduction and testing outside your application.

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

* **Important:** Ensure you register `services.AddHttpClient()` and `services.AddMemoryCache()` (if using caching) *before* the factory that registers `IHttpRequestResultService`.
* Adjust the registration lifetimes (`Scoped`, `Singleton`, `Transient`) based on your application's needs. `Scoped` is generally a good default for services involved in a web request.

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

### 4. Using Caching

If you registered the `HttpRequestResultServiceCache` decorator and `IMemoryCache` (as shown in the DI setup), simply set the `CacheDurationMinutes` property on your `HttpRequestResult` object for GET requests.

```csharp
var request = new HttpRequestResult<MyData>
{
    RequestPath = $"https://api.example.com/data/{id}",
    RequestMethod = HttpMethod.Get,
    CacheDurationMinutes = 10 // Cache the result for 10 minutes
};

var result = await _requestService.HttpSendRequestResultAsync(request);
// If a valid, non-expired cached result exists for this exact RequestPath,
// it will be returned instantly without making an actual HTTP call.
```

## Ideal Use Cases

* Building robust API clients for internal or external services.
* Simplifying HTTP interactions in microservices.
* Adding resilience and caching to existing applications with minimal refactoring.
* Any .NET application that needs to make reliable and observable HTTP calls.

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
