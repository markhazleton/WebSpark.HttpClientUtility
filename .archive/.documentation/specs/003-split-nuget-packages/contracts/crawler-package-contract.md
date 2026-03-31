# Crawler Package API Contract

**Package**: WebSpark.HttpClientUtility.Crawler v2.0.0  
**Assembly**: WebSpark.HttpClientUtility.Crawler.dll  
**Dependencies**: WebSpark.HttpClientUtility [2.0.0] (exact version match)

---

## Public API Surface

### Root Namespace: `WebSpark.HttpClientUtility.Crawler`

#### ServiceCollectionExtensions

```csharp
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers web crawler services in the dependency injection container.
    /// Requires base HttpClientUtility services to be registered first.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHttpClientCrawler(
        this IServiceCollection services);
    
    /// <summary>
    /// Registers web crawler services with custom configuration.
    /// Requires base HttpClientUtility services to be registered first.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureOptions">Action to configure crawler options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddHttpClientCrawler(
        this IServiceCollection services,
        Action<CrawlerOptions> configureOptions);
}
```

**Required Setup** (Breaking Change for Crawler Users):
```csharp
// v1.x (old way):
services.AddHttpClientUtility(); // Crawler included

// v2.0 (new way):
services.AddHttpClientUtility();   // Base features
services.AddHttpClientCrawler();   // Crawler features (NEW)
```

---

### Crawler Implementations

#### ISiteCrawler

```csharp
/// <summary>
/// Defines the contract for a website crawler.
/// </summary>
public interface ISiteCrawler
{
    /// <summary>
    /// Crawls a website starting from the specified URL.
    /// </summary>
    /// <param name="startUrl">The starting URL to crawl.</param>
    /// <param name="options">Crawler configuration options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The crawl result containing visited pages and statistics.</returns>
    Task<CrawlResult> CrawlAsync(
        string startUrl,
        CrawlerOptions options,
        CancellationToken ct = default);
    
    /// <summary>
    /// Crawls a website with real-time progress updates via SignalR.
    /// </summary>
    /// <param name="startUrl">The starting URL to crawl.</param>
    /// <param name="options">Crawler configuration options.</param>
    /// <param name="hubContext">SignalR hub context for progress updates.</param>
    /// <param name="connectionId">Client connection ID for targeted updates.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The crawl result containing visited pages and statistics.</returns>
    Task<CrawlResult> CrawlWithProgressAsync(
        string startUrl,
        CrawlerOptions options,
        IHubContext<CrawlHub> hubContext,
        string connectionId,
        CancellationToken ct = default);
}
```

#### SiteCrawler

```csharp
/// <summary>
/// Full-featured website crawler with robots.txt support, rate limiting, and performance tracking.
/// </summary>
public class SiteCrawler : ISiteCrawler
{
    public SiteCrawler(
        ILogger<SiteCrawler> logger,
        IHttpRequestResultService httpService,
        CrawlerPerformanceTracker? performanceTracker = null);
    
    public Task<CrawlResult> CrawlAsync(
        string startUrl,
        CrawlerOptions options,
        CancellationToken ct = default);
    
    public Task<CrawlResult> CrawlWithProgressAsync(
        string startUrl,
        CrawlerOptions options,
        IHubContext<CrawlHub> hubContext,
        string connectionId,
        CancellationToken ct = default);
}
```

#### SimpleSiteCrawler

```csharp
/// <summary>
/// Simplified website crawler for basic crawling scenarios without advanced features.
/// </summary>
public class SimpleSiteCrawler : ISiteCrawler
{
    public SimpleSiteCrawler(
        ILogger<SimpleSiteCrawler> logger,
        IHttpRequestResultService httpService);
    
    public Task<CrawlResult> CrawlAsync(
        string startUrl,
        CrawlerOptions options,
        CancellationToken ct = default);
    
    public Task<CrawlResult> CrawlWithProgressAsync(
        string startUrl,
        CrawlerOptions options,
        IHubContext<CrawlHub> hubContext,
        string connectionId,
        CancellationToken ct = default);
}
```

---

### Configuration

#### CrawlerOptions

```csharp
/// <summary>
/// Configuration options for website crawling.
/// </summary>
public class CrawlerOptions
{
    /// <summary>
    /// Maximum depth to crawl (default: 3).
    /// </summary>
    public int MaxDepth { get; set; } = 3;
    
    /// <summary>
    /// Maximum number of pages to crawl (default: 100).
    /// </summary>
    public int MaxPages { get; set; } = 100;
    
    /// <summary>
    /// Delay between requests in milliseconds (default: 500ms).
    /// </summary>
    public int DelayBetweenRequestsMs { get; set; } = 500;
    
    /// <summary>
    /// Whether to respect robots.txt (default: true).
    /// </summary>
    public bool RespectRobotsTxt { get; set; } = true;
    
    /// <summary>
    /// User agent string to use (default: "WebSparkCrawler/2.0").
    /// </summary>
    public string UserAgent { get; set; } = "WebSparkCrawler/2.0";
    
    /// <summary>
    /// Whether to follow external links (default: false).
    /// </summary>
    public bool FollowExternalLinks { get; set; } = false;
    
    /// <summary>
    /// Whether to enable adaptive rate limiting (default: true).
    /// </summary>
    public bool EnableAdaptiveRateLimiting { get; set; } = true;
    
    /// <summary>
    /// Timeout for each request in seconds (default: 30).
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Whether to generate sitemap.xml (default: false).
    /// </summary>
    public bool GenerateSitemap { get; set; } = false;
    
    /// <summary>
    /// Whether to export results to CSV (default: false).
    /// </summary>
    public bool ExportToCsv { get; set; } = false;
    
    /// <summary>
    /// CSV export file path (used if ExportToCsv is true).
    /// </summary>
    public string? CsvExportPath { get; set; }
}
```

---

### Robots.txt Support

#### RobotsTxtParser

```csharp
/// <summary>
/// Parses and interprets robots.txt files.
/// </summary>
public class RobotsTxtParser
{
    /// <summary>
    /// Parses robots.txt content.
    /// </summary>
    /// <param name="robotsTxtContent">The raw robots.txt content.</param>
    /// <param name="userAgent">The user agent to check rules for.</param>
    /// <returns>Parsed robots.txt rules.</returns>
    public RobotsTxtRules Parse(string robotsTxtContent, string userAgent);
    
    /// <summary>
    /// Checks if a URL is allowed to be crawled.
    /// </summary>
    /// <param name="url">The URL to check.</param>
    /// <param name="rules">The parsed robots.txt rules.</param>
    /// <returns>True if allowed, false if disallowed.</returns>
    public bool IsAllowed(string url, RobotsTxtRules rules);
}
```

#### RobotsTxtRules

```csharp
/// <summary>
/// Represents parsed robots.txt rules for a user agent.
/// </summary>
public class RobotsTxtRules
{
    public List<string> DisallowedPaths { get; set; }
    public List<string> AllowedPaths { get; set; }
    public int? CrawlDelay { get; set; }
    public string? SitemapUrl { get; set; }
}
```

#### Lock

```csharp
/// <summary>
/// Thread-safe lock for crawler operations.
/// </summary>
public class Lock
{
    public void Acquire();
    public void Release();
    public Task<bool> TryAcquireAsync(TimeSpan timeout, CancellationToken ct = default);
}
```

---

### SignalR Progress Updates

#### CrawlHub

```csharp
/// <summary>
/// SignalR hub for real-time crawler progress updates.
/// </summary>
public class CrawlHub : Hub
{
    /// <summary>
    /// Sends crawl progress update to connected clients.
    /// </summary>
    /// <param name="progress">Progress information.</param>
    public Task SendProgress(CrawlProgress progress);
    
    /// <summary>
    /// Notifies clients that crawl has completed.
    /// </summary>
    /// <param name="result">The final crawl result.</param>
    public Task SendComplete(CrawlResult result);
    
    /// <summary>
    /// Notifies clients of crawl error.
    /// </summary>
    /// <param name="error">Error message.</param>
    public Task SendError(string error);
}
```

#### CrawlProgress

```csharp
/// <summary>
/// Represents real-time crawl progress information.
/// </summary>
public class CrawlProgress
{
    public int PagesVisited { get; set; }
    public int PagesRemaining { get; set; }
    public int CurrentDepth { get; set; }
    public string? CurrentUrl { get; set; }
    public double PercentComplete { get; set; }
    public TimeSpan ElapsedTime { get; set; }
}
```

---

### Results

#### CrawlResult

```csharp
/// <summary>
/// Represents the result of a website crawl operation.
/// </summary>
public class CrawlResult
{
    /// <summary>
    /// The starting URL of the crawl.
    /// </summary>
    public string StartUrl { get; set; }
    
    /// <summary>
    /// List of successfully visited pages.
    /// </summary>
    public List<CrawledPage> VisitedPages { get; set; }
    
    /// <summary>
    /// List of pages that failed to crawl.
    /// </summary>
    public List<CrawledPage> FailedPages { get; set; }
    
    /// <summary>
    /// Total number of pages visited.
    /// </summary>
    public int TotalPagesVisited { get; set; }
    
    /// <summary>
    /// Total number of pages failed.
    /// </summary>
    public int TotalPagesFailed { get; set; }
    
    /// <summary>
    /// Total time taken for the crawl.
    /// </summary>
    public TimeSpan TotalTime { get; set; }
    
    /// <summary>
    /// Average response time per page.
    /// </summary>
    public TimeSpan AverageResponseTime { get; set; }
    
    /// <summary>
    /// Timestamp when crawl started.
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// Timestamp when crawl completed.
    /// </summary>
    public DateTime EndTime { get; set; }
    
    /// <summary>
    /// Generated sitemap XML (if GenerateSitemap option enabled).
    /// </summary>
    public string? SitemapXml { get; set; }
    
    /// <summary>
    /// Exports crawl results to CSV file.
    /// </summary>
    /// <param name="filePath">The output CSV file path.</param>
    public Task ExportToCsvAsync(string filePath);
    
    /// <summary>
    /// Generates sitemap.xml content from crawled pages.
    /// </summary>
    /// <returns>Sitemap XML string.</returns>
    public string GenerateSitemapXml();
}
```

#### CrawledPage

```csharp
/// <summary>
/// Represents a single crawled page.
/// </summary>
public class CrawledPage
{
    public string Url { get; set; }
    public int Depth { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public bool IsSuccessful { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public DateTime CrawledAt { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> OutboundLinks { get; set; }
    public string? Title { get; set; }
    public string? MetaDescription { get; set; }
}
```

#### CrawlException

```csharp
/// <summary>
/// Exception thrown during crawler operations.
/// </summary>
public class CrawlException : Exception
{
    public CrawlException(string message);
    public CrawlException(string message, Exception innerException);
    
    public string? Url { get; set; }
    public int? Depth { get; set; }
}
```

---

### Performance Tracking

#### CrawlerPerformanceTracker

```csharp
/// <summary>
/// Tracks performance metrics during crawling operations.
/// </summary>
public class CrawlerPerformanceTracker
{
    public CrawlerPerformanceTracker(ILogger<CrawlerPerformanceTracker> logger);
    
    /// <summary>
    /// Records a successful page crawl.
    /// </summary>
    public void RecordSuccess(string url, TimeSpan responseTime);
    
    /// <summary>
    /// Records a failed page crawl.
    /// </summary>
    public void RecordFailure(string url, string errorMessage);
    
    /// <summary>
    /// Gets current performance statistics.
    /// </summary>
    public CrawlerPerformanceStats GetStats();
    
    /// <summary>
    /// Resets all performance statistics.
    /// </summary>
    public void Reset();
}
```

#### CrawlerPerformanceStats

```csharp
/// <summary>
/// Performance statistics for crawler operations.
/// </summary>
public class CrawlerPerformanceStats
{
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public TimeSpan MinResponseTime { get; set; }
    public TimeSpan MaxResponseTime { get; set; }
    public double RequestsPerSecond { get; set; }
}
```

---

### Utilities

#### SiteCrawlerHelpers

```csharp
/// <summary>
/// Helper methods for site crawling operations.
/// </summary>
public static class SiteCrawlerHelpers
{
    /// <summary>
    /// Extracts links from HTML content.
    /// </summary>
    public static List<string> ExtractLinks(string htmlContent, string baseUrl);
    
    /// <summary>
    /// Normalizes a URL (removes fragments, query params if needed).
    /// </summary>
    public static string NormalizeUrl(string url);
    
    /// <summary>
    /// Checks if URL is internal to the given domain.
    /// </summary>
    public static bool IsInternalUrl(string url, string baseUrl);
    
    /// <summary>
    /// Extracts page title from HTML.
    /// </summary>
    public static string? ExtractTitle(string htmlContent);
    
    /// <summary>
    /// Extracts meta description from HTML.
    /// </summary>
    public static string? ExtractMetaDescription(string htmlContent);
}
```

---

## Breaking Changes for Crawler Users

### Migration Required

**v1.x Code**:
```csharp
// Program.cs or Startup.cs
services.AddHttpClientUtility(); // Crawler included automatically

// Usage
public class MyService
{
    private readonly ISiteCrawler _crawler;
    
    public MyService(ISiteCrawler crawler)
    {
        _crawler = crawler;
    }
    
    public async Task CrawlSite(string url)
    {
        var result = await _crawler.CrawlAsync(url, new CrawlerOptions());
    }
}
```

**v2.0 Code**:
```csharp
// Program.cs or Startup.cs
services.AddHttpClientUtility();   // Base features
services.AddHttpClientCrawler();   // Crawler features (NEW REQUIRED CALL)

// Add using directive (may be needed)
using WebSpark.HttpClientUtility.Crawler;

// Usage - identical to v1.x
public class MyService
{
    private readonly ISiteCrawler _crawler;
    
    public MyService(ISiteCrawler crawler)
    {
        _crawler = crawler;
    }
    
    public async Task CrawlSite(string url)
    {
        var result = await _crawler.CrawlAsync(url, new CrawlerOptions());
    }
}
```

**Required Changes**:
1. Install `WebSpark.HttpClientUtility.Crawler` NuGet package
2. Add `services.AddHttpClientCrawler()` call after `services.AddHttpClientUtility()`
3. Add `using WebSpark.HttpClientUtility.Crawler;` if types not resolved
4. No runtime behavior changes - identical API surface

---

## Dependencies

### Required NuGet Packages

```xml
<ItemGroup>
  <!-- Exact base package version -->
  <PackageReference Include="WebSpark.HttpClientUtility" Version="[2.0.0]" />
  
  <!-- HTML parsing -->
  <PackageReference Include="HtmlAgilityPack" Version="1.12.4" />
  
  <!-- Markdown processing -->
  <PackageReference Include="Markdig" Version="0.43.0" />
  
  <!-- CSV export -->
  <PackageReference Include="CsvHelper" Version="33.1.0" />
</ItemGroup>

<!-- SignalR support (framework reference) -->
<ItemGroup>
  <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>
```

---

## Contract Testing

**Verification**: All crawler tests must pass when upgrading from v1.5.1 to v2.0.0 after migration steps.

**Test Command**:
```powershell
dotnet test WebSpark.HttpClientUtility.Crawler.Test/WebSpark.HttpClientUtility.Crawler.Test.csproj
```

**Expected Result**: ~80-130 crawler tests pass (all crawler tests from v1.x).

---

## Usage Examples

### Basic Crawling

```csharp
public class CrawlerService
{
    private readonly ISiteCrawler _crawler;
    
    public CrawlerService(ISiteCrawler crawler)
    {
        _crawler = crawler;
    }
    
    public async Task<CrawlResult> CrawlWebsite(string url)
    {
        var options = new CrawlerOptions
        {
            MaxDepth = 3,
            MaxPages = 100,
            RespectRobotsTxt = true,
            ExportToCsv = true,
            CsvExportPath = "crawl-results.csv"
        };
        
        return await _crawler.CrawlAsync(url, options);
    }
}
```

### Real-Time Progress with SignalR

```csharp
public class CrawlerController : ControllerBase
{
    private readonly ISiteCrawler _crawler;
    private readonly IHubContext<CrawlHub> _hubContext;
    
    public CrawlerController(ISiteCrawler crawler, IHubContext<CrawlHub> hubContext)
    {
        _crawler = crawler;
        _hubContext = hubContext;
    }
    
    [HttpPost("crawl")]
    public async Task<CrawlResult> CrawlWithProgress(
        [FromBody] CrawlRequest request,
        [FromQuery] string connectionId)
    {
        var options = new CrawlerOptions
        {
            MaxDepth = request.MaxDepth,
            MaxPages = request.MaxPages
        };
        
        return await _crawler.CrawlWithProgressAsync(
            request.Url,
            options,
            _hubContext,
            connectionId);
    }
}
```

---

## Support and Migration

**Documentation**: See [Migration Guide](../quickstart.md) for detailed migration instructions.

**Support**: v1.x users migrating to v2.0.0 can reference the quickstart for side-by-side code examples.
