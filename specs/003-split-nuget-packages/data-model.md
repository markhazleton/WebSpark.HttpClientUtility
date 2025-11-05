# Data Model: Package Architecture

**Feature**: 003-split-nuget-packages  
**Date**: November 3, 2025

## Overview

This document defines the package architecture, dependency relationships, version constraints, and public API surface areas for the split NuGet package structure.

---

## Package Relationship Diagram

```
┌─────────────────────────────────────────┐
│  WebSpark.HttpClientUtility (Base)      │
│  Version: 2.0.0                          │
│  ─────────────────────────────────────   │
│  Core HTTP utilities:                    │
│  • Authentication providers              │
│  • Request/response handling             │
│  • Caching (memory cache)                │
│  • Resilience (Polly)                    │
│  • Telemetry (OpenTelemetry)             │
│  • Concurrent requests                   │
│  • Fire-and-forget                       │
│  • Streaming                             │
│  • CURL command generation               │
│  • Mock services                         │
│  • Object pooling                        │
│  • String converters                     │
│                                          │
│  Dependencies: 9                         │
└─────────────────────────────────────────┘
                     ▲
                     │
                     │ Depends on (exact [2.0.0])
                     │
┌─────────────────────────────────────────┐
│  WebSpark.HttpClientUtility.Crawler     │
│  Version: 2.0.0 (lockstep)               │
│  ─────────────────────────────────────   │
│  Web crawling capabilities:              │
│  • ISiteCrawler, SiteCrawler             │
│  • SimpleSiteCrawler                     │
│  • Robots.txt parser                     │
│  • Sitemap.xml generation                │
│  • SignalR progress updates (CrawlHub)   │
│  • CSV export of results                 │
│  • Performance tracking                  │
│  • Crawler options/configuration         │
│                                          │
│  Dependencies: 5 (base + 4 new)          │
└─────────────────────────────────────────┘
```

---

## Package Dependency Graph

### Base Package Dependencies (9 total)

```
WebSpark.HttpClientUtility (2.0.0)
├── Microsoft.Extensions.Caching.Abstractions (8.0.0)
│   └── Used by: MemoryCache/HttpRequestResultServiceCache.cs
├── Microsoft.Extensions.Http (8.0.1)
│   └── Used by: ServiceCollectionExtensions.cs, ClientService/
├── Microsoft.SourceLink.GitHub (8.0.0) [PrivateAssets=All]
│   └── Debugging support (not in published package)
├── Newtonsoft.Json (13.0.4)
│   └── Used by: StringConverter/NewtonsoftStringConverter.cs (opt-in)
├── OpenTelemetry (1.13.1)
│   └── Used by: OpenTelemetry/ActivityNames.cs, RequestResult/HttpRequestResultServiceTelemetry.cs
├── OpenTelemetry.Instrumentation.Http (1.13.0)
│   └── Used by: OpenTelemetry/ServiceCollectionExtensions.cs
├── OpenTelemetry.Extensions.Hosting (1.13.1)
│   └── Used by: OpenTelemetry/ServiceCollectionExtensions.cs
├── OpenTelemetry.Exporter.Console (1.13.1)
│   └── Used by: OpenTelemetry/ServiceCollectionExtensions.cs (opt-in)
├── OpenTelemetry.Exporter.OpenTelemetryProtocol (1.13.1)
│   └── Used by: OpenTelemetry/ServiceCollectionExtensions.cs (OTLP export)
└── Polly (8.6.4)
    └── Used by: RequestResult/HttpRequestResultServicePolly.cs (retry, circuit breaker)
```

### Crawler Package Dependencies (5 total)

```
WebSpark.HttpClientUtility.Crawler (2.0.0)
├── WebSpark.HttpClientUtility [2.0.0] (exact version match)
│   └── Base functionality (authentication, caching, resilience, telemetry)
├── HtmlAgilityPack (1.12.4)
│   └── Used by: SiteCrawler.cs, SimpleSiteCrawler.cs (HTML parsing)
├── Markdig (0.43.0)
│   └── Used by: SiteCrawler.cs (markdown processing for sitemap)
├── CsvHelper (33.1.0)
│   └── Used by: CrawlResult.cs (CSV export of crawl results)
└── Microsoft.AspNetCore.App (framework reference)
    └── Used by: CrawlHub.cs (SignalR real-time progress updates)
```

---

## Version Constraints

### Lockstep Versioning Strategy

Both packages **always have identical version numbers** (repo-level versioning):

| Release | Base Package Version | Crawler Package Version | Notes |
|---------|---------------------|-------------------------|-------|
| Initial split | 2.0.0 | 2.0.0 | Breaking change (package split) |
| Bug fix | 2.0.1 | 2.0.1 | Both packages released even if bug only in one |
| Minor feature | 2.1.0 | 2.1.0 | Both packages released even if feature only in one |
| Breaking change | 3.0.0 | 3.0.0 | Both packages released |

**Versioning Rules**:
1. Version numbers specified in `Directory.Build.props` (single source of truth)
2. Both `.csproj` files inherit version from `Directory.Build.props`
3. Crawler package specifies exact base dependency: `<PackageReference Include="WebSpark.HttpClientUtility" Version="[2.0.0]" />`
4. Version bumps always affect both packages simultaneously
5. Git tag format: `v2.0.0` (triggers CI/CD for both packages)

**Rationale**: Simplifies version management, prevents incompatible combinations, reduces support complexity.

### NuGet Dependency Specification

**Crawler .csproj dependency on base**:
```xml
<ItemGroup>
  <PackageReference Include="WebSpark.HttpClientUtility" Version="[2.0.0]" />
  <!-- Exact version match using [2.0.0] notation -->
  <!-- Prevents users from mixing incompatible versions -->
</ItemGroup>
```

**Why exact version match?**
- Crawler package tightly coupled to base package (uses decorator pattern, internal APIs)
- Prevents runtime errors from version mismatches
- Enforces lockstep versioning at consumption time
- If user installs Crawler 2.1.0, NuGet automatically installs Base 2.1.0 (not 2.0.0 or 2.2.0)

---

## Public API Surface

### Base Package Public APIs (PRESERVED from v1.x)

**Namespaces**:
- `WebSpark.HttpClientUtility` (root)
- `WebSpark.HttpClientUtility.Authentication`
- `WebSpark.HttpClientUtility.ClientService`
- `WebSpark.HttpClientUtility.Concurrent`
- `WebSpark.HttpClientUtility.CurlService`
- `WebSpark.HttpClientUtility.FireAndForget`
- `WebSpark.HttpClientUtility.MemoryCache`
- `WebSpark.HttpClientUtility.MockService`
- `WebSpark.HttpClientUtility.ObjectPool`
- `WebSpark.HttpClientUtility.OpenTelemetry`
- `WebSpark.HttpClientUtility.RequestResult`
- `WebSpark.HttpClientUtility.Streaming`
- `WebSpark.HttpClientUtility.StringConverter`
- `WebSpark.HttpClientUtility.Utilities`

**Key Public Types** (100% backward compatible):

1. **Service Registration** (ServiceCollectionExtensions.cs):
   - `AddHttpClientUtility()`
   - `AddHttpClientUtilityWithCaching()`
   - `AddHttpClientUtilityWithAllFeatures()`
   - `AddWebSparkOpenTelemetry()`

2. **Request/Response Handling**:
   - `IHttpRequestResultService`
   - `HttpRequestResult<T>`
   - `HttpRequestResultService` (base implementation)
   - `HttpRequestResultServiceCache` (decorator)
   - `HttpRequestResultServicePolly` (decorator)
   - `HttpRequestResultServiceTelemetry` (decorator)

3. **Authentication**:
   - `IAuthenticationProvider`
   - `BearerTokenAuthenticationProvider`
   - `BasicAuthenticationProvider`
   - `ApiKeyAuthenticationProvider`

4. **Caching**:
   - `IHttpRequestMemoryCache`
   - `HttpRequestMemoryCache`

5. **Configuration**:
   - `HttpClientUtilityOptions`
   - `HttpRequestResultPollyOptions`

6. **Other Features**:
   - `IConcurrentHttpRequestResultService`
   - `IFireAndForgetService`
   - `IHttpStreamingService`
   - `ICurlCommandService`
   - `IMockHttpRequestResultService`
   - String converters, query string utilities, object pooling

**Breaking Changes**: NONE for base package users

---

### Crawler Package Public APIs (NEW in v2.0.0)

**Namespaces**:
- `WebSpark.HttpClientUtility.Crawler` (root)

**Key Public Types**:

1. **Service Registration** (Crawler/ServiceCollectionExtensions.cs):
   - `AddHttpClientCrawler()` - **NEW** required DI method

2. **Crawler Implementations**:
   - `ISiteCrawler` - Interface for site crawling
   - `SiteCrawler` - Full-featured crawler with robots.txt support
   - `SimpleSiteCrawler` - Simplified crawler for basic scenarios

3. **Configuration**:
   - `CrawlerOptions` - Configuration (depth, page limit, rate limiting, etc.)

4. **Robots.txt Support**:
   - `RobotsTxtParser` - Parses robots.txt files
   - `Lock` - Thread-safe locking for crawler

5. **Progress Tracking**:
   - `CrawlHub` - SignalR hub for real-time progress updates
   - `CrawlerPerformanceTracker` - Performance metrics

6. **Results**:
   - `CrawlResult` - Crawl results with CSV export capability
   - `CrawlException` - Crawler-specific exceptions

7. **Helpers**:
   - `SiteCrawlerHelpers` - Utility methods for crawling

**Breaking Changes for Crawler Users**:
- Must install `WebSpark.HttpClientUtility.Crawler` package (NEW package)
- Must call `services.AddHttpClientCrawler()` in DI registration (NEW method)
- Runtime behavior identical to v1.5.1 after migration

---

## Assembly Metadata

### Base Package (`WebSpark.HttpClientUtility.dll`)

```xml
<AssemblyName>WebSpark.HttpClientUtility</AssemblyName>
<AssemblyVersion>2.0.0.0</AssemblyVersion>
<FileVersion>2.0.0.0</FileVersion>
<InformationalVersion>2.0.0</InformationalVersion>
<PackageId>WebSpark.HttpClientUtility</PackageId>
<RootNamespace>WebSpark.HttpClientUtility</RootNamespace>
<SignAssembly>true</SignAssembly>
<AssemblyOriginatorKeyFile>../HttpClientUtility.snk</AssemblyOriginatorKeyFile>
<PublicKeyToken>SAME_AS_V1</PublicKeyToken> <!-- Unchanged - preserves assembly binding -->
```

### Crawler Package (`WebSpark.HttpClientUtility.Crawler.dll`)

```xml
<AssemblyName>WebSpark.HttpClientUtility.Crawler</AssemblyName>
<AssemblyVersion>2.0.0.0</AssemblyVersion>
<FileVersion>2.0.0.0</FileVersion>
<InformationalVersion>2.0.0</InformationalVersion>
<PackageId>WebSpark.HttpClientUtility.Crawler</PackageId>
<RootNamespace>WebSpark.HttpClientUtility.Crawler</RootNamespace>
<SignAssembly>true</SignAssembly>
<AssemblyOriginatorKeyFile>../../HttpClientUtility.snk</AssemblyOriginatorKeyFile>
<PublicKeyToken>SAME_AS_BASE</PublicKeyToken> <!-- Same key = same token -->
```

**Key Properties**:
- Both assemblies signed with same key → same PublicKeyToken
- Base package PublicKeyToken unchanged from v1.x → preserves assembly binding redirects
- Both assemblies target `net8.0` and `net9.0` (multi-targeting)

---

## Package Size Comparison

### v1.5.1 (Current Monolithic Package)

| Package | Size | Dependencies | Files |
|---------|------|--------------|-------|
| WebSpark.HttpClientUtility | ~450 KB | 13 | ~120 files |

**Dependency Total Size** (downloaded by all users):
- HtmlAgilityPack: ~150 KB
- Markdig: ~280 KB
- CsvHelper: ~180 KB
- Other dependencies: ~600 KB
- **Total**: ~1,660 KB (1.6 MB)

### v2.0.0 (Split Packages)

| Package | Size | Dependencies | Files |
|---------|------|--------------|-------|
| WebSpark.HttpClientUtility (Base) | ~250 KB | 9 | ~80 files |
| WebSpark.HttpClientUtility.Crawler | ~60 KB | 5 (incl. base) | ~15 files |

**Base Package Dependency Total Size** (core HTTP users):
- OpenTelemetry suite: ~350 KB
- Polly: ~120 KB
- Other dependencies: ~300 KB
- **Total**: ~1,020 KB (1.0 MB)

**Crawler Package Dependency Total Size** (crawler users):
- Base package + dependencies: ~1,020 KB
- HtmlAgilityPack: ~150 KB
- Markdig: ~280 KB
- CsvHelper: ~180 KB
- **Total**: ~1,690 KB (1.65 MB - same as v1.5.1)

**Size Reduction for Core HTTP Users**: ~640 KB (38% reduction)

---

## Multi-Targeting Configuration

Both packages support .NET 8 LTS and .NET 9:

```xml
<TargetFrameworks>net8.0;net9.0</TargetFrameworks>
<LangVersion>latest</LangVersion>
<Nullable>enable</Nullable>
<ImplicitUsings>enable</ImplicitUsings>
```

**Framework-Specific Code**:
- Crawler package uses `Microsoft.AspNetCore.App` framework reference (available in both .NET 8 and .NET 9)
- No conditional compilation required (APIs consistent across target frameworks)

---

## NuGet Package Metadata

### Base Package Metadata

```xml
<PropertyGroup>
  <PackageId>WebSpark.HttpClientUtility</PackageId>
  <Title>WebSpark HttpClient Utility (Base)</Title>
  <Summary>Lightweight HttpClient wrapper with resilience, caching, and telemetry for .NET 8+ applications.</Summary>
  <Description>
    Core HTTP client utilities for .NET applications, including authentication providers 
    (Bearer, Basic, ApiKey), response caching, resilience with Polly (retry, circuit breaker), 
    OpenTelemetry integration, concurrent request handling, fire-and-forget operations, and 
    streaming capabilities. Lightweight and focused on IHttpClientFactory optimization.
    
    For web crawling capabilities, see WebSpark.HttpClientUtility.Crawler package.
  </Description>
  <PackageTags>
    httpclient;http;utility;web;api;rest;authentication;bearer;basic;apikey;
    polly;resilience;retry;circuit-breaker;cache;caching;telemetry;opentelemetry;
    concurrent;streaming;fire-and-forget;curl;observability;tracing;
    dotnet;aspnetcore;net8;net9;lts
  </PackageTags>
  <PackageProjectUrl>https://markhazleton.github.io/WebSpark.HttpClientUtility/</PackageProjectUrl>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <PackageReadmeFile>README.md</PackageReadmeFile>
  <PackageIcon>icon.png</PackageIcon>
  <RepositoryUrl>https://github.com/MarkHazleton/HttpClientUtility</RepositoryUrl>
  <RepositoryType>git</RepositoryType>
  <PackageReleaseNotes>https://github.com/MarkHazleton/HttpClientUtility/blob/main/CHANGELOG.md</PackageReleaseNotes>
</PropertyGroup>
```

### Crawler Package Metadata

```xml
<PropertyGroup>
  <PackageId>WebSpark.HttpClientUtility.Crawler</PackageId>
  <Title>WebSpark HttpClient Utility - Crawler Extension</Title>
  <Summary>Web crawling extension for WebSpark.HttpClientUtility with robots.txt support and SignalR progress updates.</Summary>
  <Description>
    Web crawling capabilities for WebSpark.HttpClientUtility, including robots.txt compliance, 
    sitemap generation, HTML parsing with HtmlAgilityPack, SignalR real-time progress updates, 
    CSV export of crawl results, and performance tracking. Requires WebSpark.HttpClientUtility 
    base package.
    
    Features:
    - SiteCrawler and SimpleSiteCrawler implementations
    - Robots.txt parsing and compliance
    - Sitemap.xml generation
    - Configurable depth and page limits
    - Adaptive rate limiting
    - SignalR Hub for real-time progress
    - CSV export of results
  </Description>
  <PackageTags>
    crawler;web-crawler;site-crawler;web-scraping;scraping;spider;
    robots-txt;robotstxt;sitemap;sitemapxml;html-parsing;htmlagilitypack;
    signalr;csv;export;dotnet;aspnetcore;net8;net9;
    httpclient-extension;webspark
  </PackageTags>
  <PackageProjectUrl>https://markhazleton.github.io/WebSpark.HttpClientUtility/</PackageProjectUrl>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <PackageReadmeFile>README-Crawler.md</PackageReadmeFile>
  <PackageIcon>icon-crawler.png</PackageIcon>
  <RepositoryUrl>https://github.com/MarkHazleton/HttpClientUtility</RepositoryUrl>
  <RepositoryType>git</RepositoryType>
  <PackageReleaseNotes>https://github.com/MarkHazleton/HttpClientUtility/blob/main/CHANGELOG.md</PackageReleaseNotes>
</PropertyGroup>
```

---

## Summary

**Key Architectural Decisions**:
1. **Lockstep Versioning**: Both packages always have identical version numbers
2. **Exact Dependency Match**: Crawler specifies exact base version `[2.0.0]`
3. **Shared Signing Key**: Both packages signed with `HttpClientUtility.snk`
4. **Namespace Preservation**: Base package retains all v1.x namespaces
5. **38% Size Reduction**: Core HTTP users download 640 KB less
6. **Zero Breaking Changes**: Core HTTP users upgrade seamlessly
7. **Clear Migration Path**: Crawler users add package + DI call

This architecture achieves the stated goals while maintaining constitutional compliance and backward compatibility for non-crawler users.
