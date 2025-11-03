namespace WebSpark.HttpClientUtility.Crawler;

/// <summary>
/// Options for configuring web crawler behavior.
/// </summary>
public class CrawlerOptions
{
    /// <summary>
    /// Gets or sets the maximum number of pages to crawl.
    /// </summary>
    public int MaxPages { get; set; } = 100;

    /// <summary>
    /// Gets or sets the delay between requests in milliseconds.
    /// </summary>
    public int DelayBetweenRequestsMs { get; set; } = 1000;

    /// <summary>
    /// Gets or sets a value indicating whether to respect robots.txt.
    /// </summary>
    public bool RespectRobotsTxt { get; set; } = true;

    /// <summary>
    /// Gets or sets the user agent string for crawler requests.
    /// </summary>
    public string UserAgent { get; set; } = "WebSparkCrawler/2.0";

    /// <summary>
    /// Gets or sets a value indicating whether to follow external links.
    /// </summary>
    public bool FollowExternalLinks { get; set; } = false;

    /// <summary>
    /// Gets or sets the maximum crawl depth.
    /// </summary>
    public int MaxDepth { get; set; } = 3;
}
