# Base Package API Contract

**Package**: WebSpark.HttpClientUtility v2.0.0  
**Assembly**: WebSpark.HttpClientUtility.dll  
**Backward Compatibility**: 100% compatible with v1.x for non-crawler features

---

## Public API Surface

### Root Namespace: `WebSpark.HttpClientUtility`

#### ServiceCollectionExtensions

```csharp
public static class ServiceCollectionExtensions
{
    // Core DI registration methods (PRESERVED from v1.x)
    public static IServiceCollection AddHttpClientUtility(
        this IServiceCollection services);
    
    public static IServiceCollection AddHttpClientUtility(
        this IServiceCollection services,
        Action<HttpClientUtilityOptions> configureOptions);
    
    public static IServiceCollection AddHttpClientUtilityWithCaching(
        this IServiceCollection services);
    
    public static IServiceCollection AddHttpClientUtilityWithCaching(
        this IServiceCollection services,
        Action<HttpClientUtilityOptions> configureOptions);
    
    public static IServiceCollection AddHttpClientUtilityWithAllFeatures(
        this IServiceCollection services);
    
    public static IServiceCollection AddHttpClientUtilityWithAllFeatures(
        this IServiceCollection services,
        Action<HttpClientUtilityOptions> configureOptions);
}
```

#### HttpClientUtilityOptions

```csharp
public class HttpClientUtilityOptions
{
    public bool EnableCaching { get; set; }
    public bool EnableResilience { get; set; }
    public bool EnableTelemetry { get; set; }
    public HttpRequestResultPollyOptions ResilienceOptions { get; set; }
}
```

#### HttpResponse

```csharp
public class HttpResponse
{
    public string Content { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public bool IsSuccessStatusCode { get; set; }
    public Dictionary<string, IEnumerable<string>> Headers { get; set; }
}
```

#### QueryStringParametersList

```csharp
public class QueryStringParametersList
{
    public void Add(string key, string value);
    public string ToQueryString();
    public Dictionary<string, string> ToDictionary();
}
```

---

### Namespace: `WebSpark.HttpClientUtility.RequestResult`

#### IHttpRequestResultService

```csharp
public interface IHttpRequestResultService
{
    Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(
        HttpRequestResult<T> request,
        CancellationToken ct = default);
    
    Task<HttpRequestResult<T>> GetAsync<T>(
        string? requestPath,
        CancellationToken ct = default);
    
    Task<HttpRequestResult<T>> PostAsync<T>(
        string? requestPath,
        object? requestBody,
        CancellationToken ct = default);
}
```

#### HttpRequestResult<T>

```csharp
public class HttpRequestResult<T>
{
    // Request properties (PRESERVED from v1.x)
    public string? RequestPath { get; set; }
    public HttpMethod? RequestMethod { get; set; }
    public object? RequestBody { get; set; }
    public Dictionary<string, string>? RequestHeaders { get; set; }
    public string? ContentType { get; set; }
    public string? CorrelationId { get; set; }
    public int CacheDurationMinutes { get; set; }
    public IAuthenticationProvider? AuthenticationProvider { get; set; }
    
    // Response properties (PRESERVED from v1.x)
    public T? ResponseResults { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public bool IsSuccessStatusCode { get; set; }
    public string? ResponseMessage { get; set; }
    public Dictionary<string, IEnumerable<string>>? ResponseHeaders { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public DateTime RequestTimestamp { get; set; }
    public DateTime ResponseTimestamp { get; set; }
}
```

#### HttpRequestResultService (Base Implementation)

```csharp
public class HttpRequestResultService : IHttpRequestResultService
{
    public HttpRequestResultService(
        ILogger<HttpRequestResultService> logger,
        IHttpClientFactory httpClientFactory);
    
    public Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(
        HttpRequestResult<T> request,
        CancellationToken ct = default);
    
    public Task<HttpRequestResult<T>> GetAsync<T>(
        string? requestPath,
        CancellationToken ct = default);
    
    public Task<HttpRequestResult<T>> PostAsync<T>(
        string? requestPath,
        object? requestBody,
        CancellationToken ct = default);
}
```

#### HttpRequestResultServiceCache (Decorator)

```csharp
public class HttpRequestResultServiceCache : IHttpRequestResultService
{
    public HttpRequestResultServiceCache(
        IHttpRequestResultService decoratedService,
        ILogger<HttpRequestResultServiceCache> logger,
        IMemoryCache memoryCache);
    
    public Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(
        HttpRequestResult<T> request,
        CancellationToken ct = default);
    
    // Other methods wrap decorated service
}
```

#### HttpRequestResultServicePolly (Decorator)

```csharp
public class HttpRequestResultServicePolly : IHttpRequestResultService
{
    public HttpRequestResultServicePolly(
        IHttpRequestResultService decoratedService,
        ILogger<HttpRequestResultServicePolly> logger,
        HttpRequestResultPollyOptions options);
    
    public Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(
        HttpRequestResult<T> request,
        CancellationToken ct = default);
    
    // Other methods wrap decorated service
}
```

#### HttpRequestResultServiceTelemetry (Decorator)

```csharp
public class HttpRequestResultServiceTelemetry : IHttpRequestResultService
{
    public HttpRequestResultServiceTelemetry(
        IHttpRequestResultService decoratedService,
        ILogger<HttpRequestResultServiceTelemetry> logger,
        ActivitySource activitySource);
    
    public Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(
        HttpRequestResult<T> request,
        CancellationToken ct = default);
    
    // Other methods wrap decorated service with telemetry
}
```

#### HttpRequestResultPollyOptions

```csharp
public class HttpRequestResultPollyOptions
{
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    public int CircuitBreakerThreshold { get; set; } = 5;
    public TimeSpan CircuitBreakerDuration { get; set; } = TimeSpan.FromSeconds(30);
}
```

---

### Namespace: `WebSpark.HttpClientUtility.Authentication`

#### IAuthenticationProvider

```csharp
public interface IAuthenticationProvider
{
    Task ApplyAuthenticationAsync(HttpRequestMessage request);
    string GetAuthenticationType();
}
```

#### BearerTokenAuthenticationProvider

```csharp
public class BearerTokenAuthenticationProvider : IAuthenticationProvider
{
    public BearerTokenAuthenticationProvider(string token);
    public Task ApplyAuthenticationAsync(HttpRequestMessage request);
    public string GetAuthenticationType();
}
```

#### BasicAuthenticationProvider

```csharp
public class BasicAuthenticationProvider : IAuthenticationProvider
{
    public BasicAuthenticationProvider(string username, string password);
    public Task ApplyAuthenticationAsync(HttpRequestMessage request);
    public string GetAuthenticationType();
}
```

#### ApiKeyAuthenticationProvider

```csharp
public class ApiKeyAuthenticationProvider : IAuthenticationProvider
{
    public ApiKeyAuthenticationProvider(string apiKey, string headerName = "X-API-Key");
    public Task ApplyAuthenticationAsync(HttpRequestMessage request);
    public string GetAuthenticationType();
}
```

---

### Namespace: `WebSpark.HttpClientUtility.MemoryCache`

#### IHttpRequestMemoryCache

```csharp
public interface IHttpRequestMemoryCache
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan expiration);
    Task RemoveAsync(string key);
    Task ClearAsync();
}
```

#### HttpRequestMemoryCache

```csharp
public class HttpRequestMemoryCache : IHttpRequestMemoryCache
{
    public HttpRequestMemoryCache(IMemoryCache memoryCache);
    public Task<T?> GetAsync<T>(string key);
    public Task SetAsync<T>(string key, T value, TimeSpan expiration);
    public Task RemoveAsync(string key);
    public Task ClearAsync();
}
```

---

### Namespace: `WebSpark.HttpClientUtility.Concurrent`

#### IConcurrentHttpRequestResultService

```csharp
public interface IConcurrentHttpRequestResultService
{
    Task<IEnumerable<HttpRequestResult<T>>> ExecuteConcurrentRequestsAsync<T>(
        IEnumerable<HttpRequestResult<T>> requests,
        int maxConcurrency = 10,
        CancellationToken ct = default);
}
```

#### ConcurrentHttpRequestResultService

```csharp
public class ConcurrentHttpRequestResultService : IConcurrentHttpRequestResultService
{
    public ConcurrentHttpRequestResultService(IHttpRequestResultService httpService);
    
    public Task<IEnumerable<HttpRequestResult<T>>> ExecuteConcurrentRequestsAsync<T>(
        IEnumerable<HttpRequestResult<T>> requests,
        int maxConcurrency = 10,
        CancellationToken ct = default);
}
```

---

### Namespace: `WebSpark.HttpClientUtility.FireAndForget`

#### IFireAndForgetService

```csharp
public interface IFireAndForgetService
{
    void ExecuteFireAndForget(Func<Task> task);
    void ExecuteFireAndForget<T>(Func<Task<T>> task, Action<T>? callback = null);
}
```

#### FireAndForgetService

```csharp
public class FireAndForgetService : IFireAndForgetService
{
    public FireAndForgetService(ILogger<FireAndForgetService> logger);
    public void ExecuteFireAndForget(Func<Task> task);
    public void ExecuteFireAndForget<T>(Func<Task<T>> task, Action<T>? callback = null);
}
```

---

### Namespace: `WebSpark.HttpClientUtility.Streaming`

#### IHttpStreamingService

```csharp
public interface IHttpStreamingService
{
    Task<Stream> GetStreamAsync(string url, CancellationToken ct = default);
    Task DownloadToFileAsync(string url, string filePath, CancellationToken ct = default);
    IAsyncEnumerable<T> StreamJsonLinesAsync<T>(string url, CancellationToken ct = default);
}
```

#### HttpStreamingService

```csharp
public class HttpStreamingService : IHttpStreamingService
{
    public HttpStreamingService(IHttpClientFactory httpClientFactory);
    public Task<Stream> GetStreamAsync(string url, CancellationToken ct = default);
    public Task DownloadToFileAsync(string url, string filePath, CancellationToken ct = default);
    public IAsyncEnumerable<T> StreamJsonLinesAsync<T>(string url, CancellationToken ct = default);
}
```

---

### Namespace: `WebSpark.HttpClientUtility.CurlService`

#### ICurlCommandService

```csharp
public interface ICurlCommandService
{
    string GenerateCurlCommand(HttpRequestMessage request);
    string GenerateCurlCommand<T>(HttpRequestResult<T> request);
}
```

#### CurlCommandService

```csharp
public class CurlCommandService : ICurlCommandService
{
    public string GenerateCurlCommand(HttpRequestMessage request);
    public string GenerateCurlCommand<T>(HttpRequestResult<T> request);
}
```

#### ICurlCommandSaver

```csharp
public interface ICurlCommandSaver
{
    Task SaveCurlCommandAsync(string curlCommand, string fileName);
    Task<IEnumerable<string>> SaveBatchCurlCommandsAsync(
        IEnumerable<string> curlCommands,
        string directoryPath);
}
```

#### CurlCommandSaver

```csharp
public class CurlCommandSaver : ICurlCommandSaver
{
    public Task SaveCurlCommandAsync(string curlCommand, string fileName);
    public Task<IEnumerable<string>> SaveBatchCurlCommandsAsync(
        IEnumerable<string> curlCommands,
        string directoryPath);
}
```

---

### Namespace: `WebSpark.HttpClientUtility.MockService`

#### IMockHttpRequestResultService

```csharp
public interface IMockHttpRequestResultService : IHttpRequestResultService
{
    void SetupMockResponse<T>(string requestPath, T response, HttpStatusCode statusCode = HttpStatusCode.OK);
    void SetupMockException(string requestPath, Exception exception);
    void ClearMocks();
}
```

#### MockHttpRequestResultService

```csharp
public class MockHttpRequestResultService : IMockHttpRequestResultService
{
    public void SetupMockResponse<T>(string requestPath, T response, HttpStatusCode statusCode = HttpStatusCode.OK);
    public void SetupMockException(string requestPath, Exception exception);
    public void ClearMocks();
    
    public Task<HttpRequestResult<T>> HttpSendRequestResultAsync<T>(
        HttpRequestResult<T> request,
        CancellationToken ct = default);
}
```

---

### Namespace: `WebSpark.HttpClientUtility.ObjectPool`

#### IObjectPoolService<T>

```csharp
public interface IObjectPoolService<T> where T : class
{
    T Get();
    void Return(T obj);
}
```

#### ObjectPoolService<T>

```csharp
public class ObjectPoolService<T> : IObjectPoolService<T> where T : class
{
    public ObjectPoolService(IObjectPool<T> pool);
    public T Get();
    public void Return(T obj);
}
```

---

### Namespace: `WebSpark.HttpClientUtility.StringConverter`

#### IStringConverter

```csharp
public interface IStringConverter
{
    string Serialize<T>(T obj);
    T? Deserialize<T>(string json);
}
```

#### SystemTextJsonStringConverter (Default)

```csharp
public class SystemTextJsonStringConverter : IStringConverter
{
    public SystemTextJsonStringConverter(JsonSerializerOptions? options = null);
    public string Serialize<T>(T obj);
    public T? Deserialize<T>(string json);
}
```

#### NewtonsoftStringConverter (Opt-in)

```csharp
public class NewtonsoftStringConverter : IStringConverter
{
    public NewtonsoftStringConverter(JsonSerializerSettings? settings = null);
    public string Serialize<T>(T obj);
    public T? Deserialize<T>(string json);
}
```

---

### Namespace: `WebSpark.HttpClientUtility.OpenTelemetry`

#### ServiceCollectionExtensions

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebSparkOpenTelemetry(
        this IServiceCollection services,
        Action<TracerProviderBuilder>? configureTracing = null);
}
```

#### ActivityNames

```csharp
public static class ActivityNames
{
    public const string HttpRequest = "WebSpark.HttpRequest";
    public const string CachedRequest = "WebSpark.CachedRequest";
    public const string ResilientRequest = "WebSpark.ResilientRequest";
}
```

---

## Breaking Changes: NONE

**All public APIs from v1.x are preserved and backward compatible.**

**Users who only use the above APIs can upgrade from v1.x to v2.0.0 without any code changes.**

---

## Removed APIs

The following APIs **have been removed** from the base package and moved to `WebSpark.HttpClientUtility.Crawler`:

### Crawler-Related Interfaces and Classes (MOVED to Crawler package)

- `ISiteCrawler`
- `SiteCrawler`
- `SimpleSiteCrawler`
- `CrawlerOptions`
- `RobotsTxtParser`
- `CrawlHub`
- `CrawlResult`
- `CrawlException`
- `CrawlerPerformanceTracker`
- `Lock`
- `SiteCrawlerHelpers`
- Crawler-specific `ServiceCollectionExtensions` methods

**Migration Path**: Install `WebSpark.HttpClientUtility.Crawler` package and call `services.AddHttpClientCrawler()`.

---

## Contract Testing

**Verification**: All base package tests must pass without modification when upgrading from v1.5.1 to v2.0.0.

**Test Command**:
```powershell
dotnet test WebSpark.HttpClientUtility.Test/WebSpark.HttpClientUtility.Test.csproj
```

**Expected Result**: ~400-450 tests pass (all non-crawler tests from v1.x).

---

## Deprecation Policy

**v1.x Support**: WebSpark.HttpClientUtility v1.x will receive security and critical bug fixes for 6-12 months after v2.0.0 release.

**Recommendation**: Users are encouraged to upgrade to v2.0.0 for new features and improvements.
