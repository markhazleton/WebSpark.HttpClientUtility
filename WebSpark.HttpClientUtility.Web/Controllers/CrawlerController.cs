using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebSpark.HttpClientUtility.Crawler;

namespace WebSpark.HttpClientUtility.Web.Controllers;

/// <summary>
/// Demonstrates web crawling functionality with the WebSpark.HttpClientUtility.Crawler package
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CrawlerController : ControllerBase
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
    /// Initiates a web crawl with specified options
    /// </summary>
    /// <param name="request">Crawl request with URL and options</param>
    /// <returns>Crawl results including all discovered pages</returns>
    [HttpPost("crawl")]
    [ProducesResponseType(typeof(CrawlResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CrawlResult>> CrawlSite([FromBody] CrawlRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
        {
            return BadRequest("URL is required");
        }

        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uri))
        {
            return BadRequest("Invalid URL format");
        }

        _logger.LogInformation("Starting crawl of {Url}", request.Url);

        var options = new CrawlerOptions
        {
            MaxDepth = request.MaxDepth ?? 2,
            MaxPages = request.MaxPages ?? 50,
            RespectRobotsTxt = request.RespectRobotsTxt ?? true,
            UserAgent = request.UserAgent ?? "WebSpark.HttpClientUtility.Crawler/2.0.0",
            RequestDelayMs = request.DelayMs ?? 1000
        };

        var result = await _crawler.CrawlAsync(request.Url, options);

        _logger.LogInformation(
            "Crawl completed: {PageCount} pages, {ErrorCount} errors",
            result.CrawlResults.Count(r => r.IsSuccessStatusCode),
            result.CrawlResults.Count(r => !r.IsSuccessStatusCode));

        return Ok(result);
    }

    /// <summary>
    /// Gets the current status of active crawls
    /// </summary>
    /// <returns>Status information about active crawls</returns>
    [HttpGet("status")]
    [ProducesResponseType(typeof(CrawlerStatus), StatusCodes.Status200OK)]
    public ActionResult<CrawlerStatus> GetStatus()
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
    [HttpGet("info")]
    [ProducesResponseType(typeof(CrawlerInfo), StatusCodes.Status200OK)]
    public ActionResult<CrawlerInfo> GetInfo()
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
