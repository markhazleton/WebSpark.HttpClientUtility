# WebSpark.HttpClientUtility.Crawler

Web crawling extension for [WebSpark.HttpClientUtility](https://www.nuget.org/packages/WebSpark.HttpClientUtility).

## Overview

This package provides enterprise-grade web crawling capabilities with robots.txt compliance, HTML parsing, sitemap generation, and real-time progress tracking via SignalR.

**Important**: This is an extension package. You must also install the base package `WebSpark.HttpClientUtility` version 2.0.0.

## Features

- **SiteCrawler**: Full-featured web crawler with configurable depth and URL filtering
- **SimpleSiteCrawler**: Lightweight crawler for basic crawling needs
- **Robots.txt Compliance**: Automatic parsing and enforcement of robots.txt rules
- **HTML Parsing**: Extract links, images, and metadata using HtmlAgilityPack
- **Sitemap Generation**: Generate sitemaps in XML and Markdown formats
- **CSV Export**: Export crawl results to CSV for analysis
- **SignalR Progress**: Real-time crawl progress updates via SignalR hub
- **Performance Tracking**: Built-in metrics for crawl operations

## Installation

Install both packages:

```bash
dotnet add package WebSpark.HttpClientUtility
dotnet add package WebSpark.HttpClientUtility.Crawler
```

## Quick Start

### 1. Register Services

```csharp
using WebSpark.HttpClientUtility;
using WebSpark.HttpClientUtility.Crawler;

var builder = WebApplication.CreateBuilder(args);

// Register base package (required)
builder.Services.AddHttpClientUtility();

// Register crawler package
builder.Services.AddHttpClientCrawler();

var app = builder.Build();

// Optional: Register SignalR hub for progress updates
app.MapHub<CrawlHub>("/crawlHub");

app.Run();
```

### 2. Basic Crawling

```csharp
public class CrawlerService
{
    private readonly ISiteCrawler _crawler;

    public CrawlerService(ISiteCrawler crawler)
    {
        _crawler = crawler;
    }

    public async Task<CrawlResult> CrawlWebsiteAsync(string url)
    {
        var options = new CrawlerOptions
        {
            StartUrl = url,
            MaxDepth = 3,
            MaxPages = 100,
            RespectRobotsTxt = true
        };

        var result = await _crawler.CrawlAsync(options);
        
        Console.WriteLine($"Crawled {result.TotalPages} pages in {result.Duration}");
        return result;
    }
}
```

### 3. SimpleSiteCrawler

For lightweight crawling without full recursion:

```csharp
public class SimpleCrawlerService
{
    private readonly SimpleSiteCrawler _simpleCrawler;

    public SimpleCrawlerService(SimpleSiteCrawler simpleCrawler)
    {
        _simpleCrawler = simpleCrawler;
    }

    public async Task<List<string>> GetAllLinksAsync(string url)
    {
        var result = await _simpleCrawler.CrawlAsync(url);
        return result.DiscoveredUrls;
    }
}
```

### 4. Advanced Configuration

```csharp
builder.Services.AddHttpClientCrawler(options =>
{
    options.DefaultMaxDepth = 5;
    options.DefaultMaxPages = 500;
    options.DefaultTimeout = TimeSpan.FromSeconds(30);
    options.UserAgent = "MyBot/1.0";
});
```

### 5. CSV Export

```csharp
var result = await _crawler.CrawlAsync(options);

await result.ExportToCsvAsync("crawl-results.csv");
```

### 6. SignalR Progress Updates

```javascript
// Client-side JavaScript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/crawlHub")
    .build();

connection.on("CrawlProgress", (progress) => {
    console.log(`Progress: ${progress.pagesProcessed}/${progress.totalPages}`);
});

await connection.start();
```

## CrawlerOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| StartUrl | string | required | The URL to start crawling from |
| MaxDepth | int | 3 | Maximum depth to crawl (0 = no limit) |
| MaxPages | int | 100 | Maximum number of pages to crawl |
| RespectRobotsTxt | bool | true | Honor robots.txt directives |
| Timeout | TimeSpan | 30s | Request timeout per page |
| UserAgent | string | "WebSpark.Crawler" | User agent string |
| AllowedDomains | List<string> | null | Restrict crawling to specific domains |
| ExcludedPaths | List<string> | null | Paths to exclude from crawling |

## CrawlResult Properties

- **TotalPages**: Number of pages successfully crawled
- **FailedPages**: Number of pages that failed to crawl
- **Duration**: Total time taken for the crawl
- **DiscoveredUrls**: List of all URLs discovered
- **SitemapXml**: Generated sitemap in XML format
- **SitemapMarkdown**: Generated sitemap in Markdown format
- **Errors**: List of errors encountered during crawl

## Robots.txt Support

The crawler automatically:
- Downloads and parses robots.txt from target sites
- Respects `Disallow` directives
- Honors `Crawl-delay` settings
- Supports wildcard patterns

Disable with:
```csharp
options.RespectRobotsTxt = false; // Not recommended
```

## Performance Tips

1. **Limit Scope**: Use `MaxDepth` and `MaxPages` to avoid overwhelming servers
2. **Respect Crawl-Delay**: The crawler automatically honors robots.txt delays
3. **Use AllowedDomains**: Restrict crawling to prevent following external links
4. **Monitor Progress**: Use SignalR hub to track crawl status in real-time

## Requirements

- **Base Package**: WebSpark.HttpClientUtility 2.0.0 (exact match required)
- **.NET Version**: .NET 8 LTS or .NET 9
- **ASP.NET Core**: Required for SignalR features

## Migration from v1.x

If you're upgrading from WebSpark.HttpClientUtility v1.x:

1. Install the crawler package: `dotnet add package WebSpark.HttpClientUtility.Crawler`
2. Add using directive: `using WebSpark.HttpClientUtility.Crawler;`
3. Add DI registration: `services.AddHttpClientCrawler();`

All crawler APIs remain unchanged - only the registration is different.

## Documentation

- [Full Documentation](https://markhazleton.github.io/WebSpark.HttpClientUtility/)
- [Migration Guide](https://markhazleton.github.io/WebSpark.HttpClientUtility/getting-started/migration-v2/)
- [API Reference](https://markhazleton.github.io/WebSpark.HttpClientUtility/api-reference/)

## License

MIT License - see [LICENSE](https://github.com/MarkHazleton/WebSpark.HttpClientUtility/blob/main/LICENSE)

## Support

- [GitHub Issues](https://github.com/MarkHazleton/WebSpark.HttpClientUtility/issues)
- [NuGet Package](https://www.nuget.org/packages/WebSpark.HttpClientUtility.Crawler)
