---
layout: layouts/base.njk
title: Crawler Harvested Scenarios
description: Harvested crawler progress and site-analysis examples from the legacy HttpClientDecoratorPattern demo.
permalink: /examples/crawler-harvested-scenarios/
---

# Crawler Harvested Scenarios

This page captures crawler scenarios moved from the legacy demo into the current documentation set.

## Register Crawler Services

```csharp
builder.Services.AddHttpClientUtility();
builder.Services.AddHttpClientCrawler();
builder.Services.AddSignalR();
```

## Run a Site Analysis Crawl

```csharp
var crawler = serviceProvider.GetRequiredService<ISiteCrawler>();

var options = new CrawlerOptions
{
    BaseUrl = "https://example.com",
    MaxDepth = 3,
    MaxPages = 250,
    RespectRobotsTxt = true,
    CrawlDelayMs = 200
};

var result = await crawler.CrawlAsync(options.BaseUrl, options);
```

## Stream Progress to SignalR

```csharp
public class CrawlHub : Hub
{
}

// During crawl processing
await hubContext.Clients.All.SendAsync("UrlFound", pagesDiscovered, cancellationToken);
```

## Operational Guidance

- Keep crawl depth/page limits conservative in shared or production environments.
- Respect robots.txt and identify your crawler user agent.
- Use progress broadcasts for operator feedback rather than blocking UI calls.
- Persist summary output (CSV or structured logs) for auditability.