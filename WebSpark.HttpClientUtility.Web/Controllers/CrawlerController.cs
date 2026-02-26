using Microsoft.AspNetCore.Mvc;
using WebSpark.HttpClientUtility.Crawler;

namespace WebSpark.HttpClientUtility.Web.Controllers;

/// <summary>
/// Demonstrates web crawling functionality using the WebSpark.HttpClientUtility.Crawler package
/// </summary>
public class CrawlerController : Controller
{
    private readonly ISiteCrawler _crawler;
    private readonly ILogger<CrawlerController> _logger;

    public CrawlerController(ISiteCrawler crawler, ILogger<CrawlerController> logger)
    {
        _crawler = crawler;
        _logger = logger;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Web Crawler";
        return View();
    }

    /// <summary>
    /// Crawls a site and returns results as JSON.
    /// Capped at 25 pages / depth 3 to keep the demo fast.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Crawl([FromBody] CrawlRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request?.Url))
        {
            return BadRequest(new { error = "URL is required." });
        }

        if (!Uri.TryCreate(request.Url, UriKind.Absolute, out _))
        {
            return BadRequest(new { error = "URL must be an absolute URL (e.g. https://markhazleton.com)." });
        }

        var options = new CrawlerOptions
        {
            MaxPages = Math.Clamp(request.MaxPages ?? 20, 1, 25),
            MaxDepth = Math.Clamp(request.MaxDepth ?? 2, 1, 3),
            RespectRobotsTxt = request.RespectRobotsTxt ?? true,
            DiscoverFromSitemapAndRss = request.DiscoverFromSitemap ?? true,
            UserAgent = "WebSpark.HttpClientUtility.Crawler/2.0.0",
            RequestDelayMs = 300,
            MaxConcurrentRequests = 2,
            GenerateSitemap = false
        };

        _logger.LogInformation("Starting demo crawl: {Url} MaxPages={MaxPages} MaxDepth={MaxDepth}",
            request.Url, options.MaxPages, options.MaxDepth);

        var sw = System.Diagnostics.Stopwatch.StartNew();

        CrawlDomainViewModel result;
        try
        {
            result = await _crawler.CrawlAsync(request.Url, options, ct);
        }
        catch (OperationCanceledException)
        {
            return StatusCode(499, new { error = "Crawl was cancelled." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Crawl failed for {Url}", request.Url);
            return StatusCode(500, new { error = ex.Message });
        }

        sw.Stop();

        var pages = result.CrawlResults
            .OrderBy(r => r.Depth)
            .ThenBy(r => r.RequestPath)
            .Select(r => new
            {
                url = r.RequestPath ?? string.Empty,
                title = ExtractTitle(r),
                statusCode = (int)r.StatusCode,
                isSuccess = r.IsSuccessStatusCode,
                depth = r.Depth,
                linksFound = r.CrawlLinks?.Count ?? 0,
                elapsedMs = r.ElapsedMilliseconds,
                errors = r.Errors.Any() ? string.Join("; ", r.Errors) : null
            })
            .ToList();

        _logger.LogInformation("Crawl complete: {Count} pages in {Ms}ms", pages.Count, sw.ElapsedMilliseconds);

        return Ok(new
        {
            startUrl = request.Url,
            totalPages = pages.Count,
            successCount = pages.Count(p => p.isSuccess),
            failureCount = pages.Count(p => !p.isSuccess),
            durationMs = sw.ElapsedMilliseconds,
            maxPagesLimit = options.MaxPages,
            maxDepth = options.MaxDepth,
            robotsTxtRespected = options.RespectRobotsTxt,
            sitemapDiscovery = options.DiscoverFromSitemapAndRss,
            pages
        });
    }

    private static string ExtractTitle(CrawlResult r)
    {
        try
        {
            return r.ResponseHtmlDocument?
                .DocumentNode
                .SelectSingleNode("//title")?
                .InnerText
                .Trim() ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}

public class CrawlRequest
{
    public string Url { get; set; } = string.Empty;
    public int? MaxDepth { get; set; }
    public int? MaxPages { get; set; }
    public bool? RespectRobotsTxt { get; set; }
    public bool? DiscoverFromSitemap { get; set; }
}
