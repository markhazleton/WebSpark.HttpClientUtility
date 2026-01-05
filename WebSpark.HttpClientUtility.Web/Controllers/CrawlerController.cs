using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebSpark.HttpClientUtility.Crawler;

namespace WebSpark.HttpClientUtility.Web.Controllers;

/// <summary>
/// Demonstrates web crawling functionality with the WebSpark.HttpClientUtility.Crawler package
/// </summary>
public class CrawlerController : Controller
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<CrawlHub> _hubContext;
    private readonly ILogger<CrawlerController> _logger;

    public CrawlerController(
        IServiceScopeFactory scopeFactory,
        IHubContext<CrawlHub> hubContext,
        ILogger<CrawlerController> logger)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Main crawler demonstration page
    /// </summary>
    public IActionResult Index()
    {
        ViewData["Title"] = "Web Crawler";
        ViewData["Description"] = "Demonstrates web crawling with robots.txt compliance, sitemap generation, and real-time progress updates";
        return View();
    }

    /// <summary>
    /// Test page to verify Scripts section rendering
    /// </summary>
    public IActionResult Test()
    {
        return View();
    }

    /// <summary>
    /// Full diagnostic page
    /// </summary>
    public IActionResult Diagnostic()
    {
        return View();
    }

    /// <summary>
    /// Initiates a web crawl with specified options (runs asynchronously in background)
    /// </summary>
    /// <param name="request">Crawl request with URL and options</param>
    /// <returns>Acknowledgment that crawl has started (use SignalR for progress updates)</returns>
    [HttpPost]
    public IActionResult Crawl([FromBody] CrawlRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
        {
            return BadRequest(new { error = "URL is required" });
        }

        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri))
        {
            return BadRequest(new { error = "Invalid URL format" });
        }

        _logger.LogInformation("Starting crawl of {Url}", request.Url);

        var options = new CrawlerOptions
        {
            MaxDepth = request.MaxDepth ?? 2,
            MaxPages = request.MaxPages ?? 50,
            RespectRobotsTxt = request.RespectRobotsTxt ?? true,
            DiscoverFromSitemapAndRss = request.DiscoverFromSitemap ?? true,
            UserAgent = request.UserAgent ?? "WebSpark.HttpClientUtility.Crawler/2.0.0",
            RequestDelayMs = request.DelayMs ?? 1000
        };

        var crawlStartTime = DateTime.UtcNow;
        var successCount = 0;
        var failureCount = 0;

        _ = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var scopedCrawler = scope.ServiceProvider.GetRequiredService<ISiteCrawler>();

            try
            {
                // Send initial progress update
                await _hubContext.Clients.All.SendAsync("CrawlProgress", new CrawlProgressUpdate
                {
                    PagesCrawled = 0,
                    PagesInQueue = 0,
                    SuccessfulPages = 0,
                    FailedPages = 0,
                    MaxPages = options.MaxPages,
                    ProgressPercentage = 0,
                    CurrentUrl = request.Url
                });

                var result = await scopedCrawler.CrawlAsync(request.Url, options);

                var crawlDuration = DateTime.UtcNow - crawlStartTime;
                successCount = result.CrawlResults.Count(r => r.IsSuccessStatusCode);
                failureCount = result.CrawlResults.Count(r => !r.IsSuccessStatusCode);

                _logger.LogInformation(
                    "Crawl completed: {PageCount} pages, {ErrorCount} errors in {Duration}",
                    successCount,
                    failureCount,
                    crawlDuration);

                // Send individual page events for all crawled pages
                foreach (var crawlResult in result.CrawlResults)
                {
                    var pageEvent = new PageCrawledEvent
                    {
                        Url = crawlResult.RequestPath ?? string.Empty,
                        StatusCode = (int)crawlResult.StatusCode,
                        IsSuccess = crawlResult.IsSuccessStatusCode,
                        Title = ExtractPageTitle(crawlResult),
                        Depth = crawlResult.Depth,
                        LinksFound = crawlResult.CrawlLinks?.Count ?? 0,
                        Error = crawlResult.Errors.Any() ? string.Join("; ", crawlResult.Errors) : null
                    };

                    await _hubContext.Clients.All.SendAsync("PageCrawled", pageEvent);
                }

                // Send final statistics
                var statistics = new CrawlStatistics
                {
                    TotalPages = result.CrawlResults.Count,
                    SuccessfulPages = successCount,
                    FailedPages = failureCount,
                    Duration = crawlDuration,
                    PagesPerSecond = crawlDuration.TotalSeconds > 0 
                        ? result.CrawlResults.Count / crawlDuration.TotalSeconds 
                        : 0
                };

                await _hubContext.Clients.All.SendAsync("CrawlStatistics", statistics);

                // Send final results (for backward compatibility)
                var resultDto = new
                {
                    startUrl = request.Url,
                    totalPages = result.CrawlResults.Count,
                    successfulPages = successCount,
                    failedPages = failureCount,
                    duration = crawlDuration.ToString(@"hh\:mm\:ss"),
                    pagesPerSecond = statistics.PagesPerSecond,
                    crawlResults = result.CrawlResults.Select(r => new
                    {
                        requestPath = r.RequestPath ?? string.Empty,
                        isSuccessStatusCode = r.IsSuccessStatusCode,
                        statusCode = (int)r.StatusCode,
                        pageTitle = ExtractPageTitle(r),
                        depth = r.Depth,
                        linksFound = r.CrawlLinks?.Count ?? 0
                    }).ToList()
                };

                await _hubContext.Clients.All.SendAsync("CrawlResults", resultDto);

                _logger.LogInformation("Sent crawl results to SignalR clients: {PageCount} pages", result.CrawlResults.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during background crawl of {Url}", request.Url);

                await _hubContext.Clients.All.SendAsync("CrawlError", new
                {
                    url = request.Url,
                    error = ex.Message,
                    timestamp = DateTime.UtcNow
                });
            }
        });

        // Return immediately with 202 Accepted
        return Accepted(new
        {
            message = "Crawl started successfully. Monitor progress via SignalR connection.",
            url = request.Url,
            maxPages = options.MaxPages,
            maxDepth = options.MaxDepth,
            signalREvents = new[]
            {
                "CrawlProgress - Real-time progress updates",
                "PageCrawled - Individual page crawl events",
                "CrawlStatistics - Final statistics",
                "CrawlResults - Complete results",
                "CrawlError - Error notifications"
            }
        });
    }

    /// <summary>
    /// Extracts the page title from a crawl result
    /// </summary>
    private static string ExtractPageTitle(CrawlResult crawlResult)
    {
        try
        {
            var htmlDoc = crawlResult.ResponseHtmlDocument;
            if (htmlDoc != null)
            {
                var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//title");
                if (titleNode != null)
                {
                    return titleNode.InnerText.Trim();
                }
            }
        }
        catch
        {
            // Ignore errors extracting title
        }
        return string.Empty;
    }

    /// <summary>
    /// Gets the current status of active crawls
    /// </summary>
    /// <returns>Status information about active crawls</returns>
    [HttpGet]
    public ActionResult<CrawlerStatus> Status()
    {
        // In a real application, you would track active crawls in a service
        // For demo purposes, we return a simple status
        return Ok(new CrawlerStatus
        {
            ActiveCrawls = 0,
            Message = "No active crawls. Use POST /api/crawler/crawl to start a new crawl."
        });
    }

    /// <summary>
    /// Gets information about the crawler capabilities
    /// </summary>
    /// <returns>Information about crawler features and configuration</returns>
    [HttpGet]
    public ActionResult<CrawlerInfo> Info()
    {
        return Ok(new CrawlerInfo
        {
            Version = "2.0.0",
            Features = new[]
            {
                "Robots.txt compliance",
                "HTML link extraction",
                "Sitemap generation",
                "SignalR progress updates",
                "CSV export",
                "Depth-limited crawling",
                "Configurable delays"
            },
            MaxRecommendedPages = 1000,
            MaxRecommendedDepth = 5
        });
    }
}

/// <summary>
/// Request model for initiating a crawl
/// </summary>
public class CrawlRequest
{
    /// <summary>
    /// The URL to start crawling from
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Maximum depth to crawl (default: 2)
    /// </summary>
    public int? MaxDepth { get; set; }

    /// <summary>
    /// Maximum number of pages to crawl (default: 50)
    /// </summary>
    public int? MaxPages { get; set; }

    /// <summary>
    /// Whether to respect robots.txt (default: true)
    /// </summary>
    public bool? RespectRobotsTxt { get; set; }

    /// <summary>
    /// User agent string (default: WebSpark.HttpClientUtility.Crawler/2.0.0)
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// Delay between requests in milliseconds (default: 1000)
    /// </summary>
    public int? DelayMs { get; set; }

    /// <summary>
    /// Whether to discover URLs from sitemap.xml and RSS feeds (default: true)
    /// </summary>
    public bool? DiscoverFromSitemap { get; set; }
}

/// <summary>
/// Status of active crawls
/// </summary>
public class CrawlerStatus
{
    /// <summary>
    /// Number of active crawls
    /// </summary>
    public int ActiveCrawls { get; set; }

    /// <summary>
    /// Status message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Information about crawler capabilities
/// </summary>
public class CrawlerInfo
{
    /// <summary>
    /// Crawler version
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Available features
    /// </summary>
    public string[] Features { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Maximum recommended pages
    /// </summary>
    public int MaxRecommendedPages { get; set; }

    /// <summary>
    /// Maximum recommended depth
    /// </summary>
    public int MaxRecommendedDepth { get; set; }
}

/// <summary>
/// Event raised when a single page is crawled
/// </summary>
public class PageCrawledEvent
{
    /// <summary>
    /// URL of the crawled page
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Whether the request was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Page title
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Depth of the page in the crawl hierarchy
    /// </summary>
    public int Depth { get; set; }

    /// <summary>
    /// Number of links found on the page
    /// </summary>
    public int LinksFound { get; set; }

    /// <summary>
    /// Timestamp when the page was crawled
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Error message if the crawl failed
    /// </summary>
    public string? Error { get; set; }
}

/// <summary>
/// Progress update for the crawl operation
/// </summary>
public class CrawlProgressUpdate
{
    /// <summary>
    /// Number of pages crawled so far
    /// </summary>
    public int PagesCrawled { get; set; }

    /// <summary>
    /// Number of pages in the queue
    /// </summary>
    public int PagesInQueue { get; set; }

    /// <summary>
    /// Number of successful pages
    /// </summary>
    public int SuccessfulPages { get; set; }

    /// <summary>
    /// Number of failed pages
    /// </summary>
    public int FailedPages { get; set; }

    /// <summary>
    /// Maximum pages to crawl
    /// </summary>
    public int MaxPages { get; set; }

    /// <summary>
    /// Current progress percentage (0-100)
    /// </summary>
    public int ProgressPercentage { get; set; }

    /// <summary>
    /// The URL being processed
    /// </summary>
    public string? CurrentUrl { get; set; }

    /// <summary>
    /// Timestamp of the update
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Statistics for the crawl operation
/// </summary>
public class CrawlStatistics
{
    /// <summary>
    /// Total pages crawled
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Successful pages
    /// </summary>
    public int SuccessfulPages { get; set; }

    /// <summary>
    /// Failed pages
    /// </summary>
    public int FailedPages { get; set; }

    /// <summary>
    /// Average response time in milliseconds
    /// </summary>
    public double AverageResponseTime { get; set; }

    /// <summary>
    /// Total crawl duration
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Pages per second
    /// </summary>
    public double PagesPerSecond { get; set; }
}

/// <summary>
/// Serializable DTO for crawl results
/// </summary>
public class CrawlResultDto
{
    /// <summary>
    /// The starting URL that was crawled
    /// </summary>
    public string StartUrl { get; set; } = string.Empty;

    /// <summary>
    /// Total number of pages crawled
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Number of successfully crawled pages
    /// </summary>
    public int SuccessfulPages { get; set; }

    /// <summary>
    /// Number of failed pages
    /// </summary>
    public int FailedPages { get; set; }

    /// <summary>
    /// List of crawled pages
    /// </summary>
    public List<CrawlPageDto> CrawlResults { get; set; } = new();
}

/// <summary>
/// Serializable DTO for individual crawled page
/// </summary>
public class CrawlPageDto
{
    /// <summary>
    /// The URL of the page
    /// </summary>
    public string RequestPath { get; set; } = string.Empty;

    /// <summary>
    /// Whether the request was successful
    /// </summary>
    public bool IsSuccessStatusCode { get; set; }

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Page title extracted from HTML
    /// </summary>
    public string PageTitle { get; set; } = string.Empty;

    /// <summary>
    /// Depth level of this page
    /// </summary>
    public int Depth { get; set; }

    /// <summary>
    /// Number of links found on this page
    /// </summary>
    public int LinksFound { get; set; }
}
