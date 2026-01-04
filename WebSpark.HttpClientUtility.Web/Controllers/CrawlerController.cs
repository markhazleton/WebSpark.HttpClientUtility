using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebSpark.HttpClientUtility.Crawler;

namespace WebSpark.HttpClientUtility.Web.Controllers;

/// <summary>
/// Demonstrates web crawling functionality with the WebSpark.HttpClientUtility.Crawler package
/// </summary>
public class CrawlerController : Controller
{
    private readonly ISiteCrawler _crawler;
    private readonly IHubContext<CrawlHub> _hubContext;
    private readonly ILogger<CrawlerController> _logger;

    public CrawlerController(
        ISiteCrawler crawler,
        IHubContext<CrawlHub> hubContext,
        ILogger<CrawlerController> logger)
    {
        _crawler = crawler;
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

        // Start crawl in background - don't await!
        _ = Task.Run(async () =>
        {
            try
            {
                var result = await _crawler.CrawlAsync(request.Url, options);

                _logger.LogInformation(
                    "Crawl completed: {PageCount} pages, {ErrorCount} errors",
                    result.CrawlResults.Count(r => r.IsSuccessStatusCode),
                    result.CrawlResults.Count(r => !r.IsSuccessStatusCode));

                // Send results via SignalR
                var resultDto = new
                {
                    startUrl = request.Url,
                    totalPages = result.CrawlResults.Count,
                    successfulPages = result.CrawlResults.Count(r => r.IsSuccessStatusCode),
                    failedPages = result.CrawlResults.Count(r => !r.IsSuccessStatusCode),
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

                // Send complete results to all connected clients
                await _hubContext.Clients.All.SendAsync("CrawlResults", resultDto);
                
                _logger.LogInformation("Sent crawl results to SignalR clients: {PageCount} pages", result.CrawlResults.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during background crawl of {Url}", request.Url);
                
                // Send error via SignalR
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
            note = "Connect to SignalR hub '/crawlHub' with event 'UrlFound' for real-time updates"
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
    /// Number of currently active crawls
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
    /// List of supported features
    /// </summary>
    public string[] Features { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Maximum recommended pages to crawl
    /// </summary>
    public int MaxRecommendedPages { get; set; }

    /// <summary>
    /// Maximum recommended crawl depth
    /// </summary>
    public int MaxRecommendedDepth { get; set; }
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
