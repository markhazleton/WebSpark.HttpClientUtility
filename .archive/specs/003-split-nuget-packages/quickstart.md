# Quickstart: Migrating to v2.0.0

**Target Audience**: Existing WebSpark.HttpClientUtility v1.x users upgrading to v2.0.0

---

## Overview

WebSpark.HttpClientUtility v2.0.0 splits the monolithic package into two packages:

- **WebSpark.HttpClientUtility** (Base): Core HTTP utilities
- **WebSpark.HttpClientUtility.Crawler**: Web crawling extension

**Your migration path depends on which features you use.**

---

## Decision Tree: Do I Need to Migrate?

```
┌─────────────────────────────────────────────────────────────┐
│ Do you use ANY of these crawler features?                   │
│                                                              │
│ • ISiteCrawler, SiteCrawler, SimpleSiteCrawler              │
│ • CrawlerOptions, RobotsTxtParser                           │
│ • CrawlHub, CrawlResult                                     │
│ • Sitemap generation, CSV export of crawl results           │
└─────────────────────────────────────────────────────────────┘
                              │
                              ▼
           ┌──────────────────┴──────────────────┐
           │                                     │
          YES                                   NO
           │                                     │
           ▼                                     ▼
    ┌──────────────┐                    ┌──────────────┐
    │ MIGRATION    │                    │ NO MIGRATION │
    │ REQUIRED     │                    │ REQUIRED     │
    │              │                    │              │
    │ Follow Path  │                    │ Follow Path  │
    │ A below      │                    │ B below      │
    └──────────────┘                    └──────────────┘
```

---

## Path A: Migration Required (Crawler Users)

### Step 1: Install Crawler Package

```powershell
# Using .NET CLI
dotnet add package WebSpark.HttpClientUtility.Crawler --version 2.0.0

# Using Package Manager Console
Install-Package WebSpark.HttpClientUtility.Crawler -Version 2.0.0

# Using PackageReference in .csproj
<PackageReference Include="WebSpark.HttpClientUtility.Crawler" Version="2.0.0" />
```

### Step 2: Update DI Registration

**Before (v1.x)**:
```csharp
// Program.cs or Startup.cs
using WebSpark.HttpClientUtility;

var builder = WebApplication.CreateBuilder(args);

// Crawler features included automatically
builder.Services.AddHttpClientUtility();

var app = builder.Build();
```

**After (v2.0.0)**:
```csharp
// Program.cs or Startup.cs
using WebSpark.HttpClientUtility;
using WebSpark.HttpClientUtility.Crawler; // NEW using directive

var builder = WebApplication.CreateBuilder(args);

// Register base package
builder.Services.AddHttpClientUtility();

// Register crawler package (NEW REQUIRED CALL)
builder.Services.AddHttpClientCrawler();

var app = builder.Build();
```

### Step 3: Verify Using Directives

Your crawler-using code files may need the new namespace:

```csharp
using WebSpark.HttpClientUtility;
using WebSpark.HttpClientUtility.Crawler; // Add this if ISiteCrawler, CrawlerOptions, etc. not found
```

### Step 4: Verify Runtime Behavior

**No other code changes required.** All crawler APIs are identical to v1.x:

```csharp
public class MyCrawlerService
{
    private readonly ISiteCrawler _crawler;
    
    public MyCrawlerService(ISiteCrawler crawler)
    {
        _crawler = crawler; // Injection still works
    }
    
    public async Task<CrawlResult> CrawlSite(string url)
    {
        var options = new CrawlerOptions // Same API as v1.x
        {
            MaxDepth = 3,
            MaxPages = 100,
            RespectRobotsTxt = true
        };
        
        return await _crawler.CrawlAsync(url, options); // Same method signature
    }
}
```

### Step 5: Update Tests

If you have tests that use crawler features, update test project references:

```xml
<!-- YourProject.Test.csproj -->
<ItemGroup>
  <PackageReference Include="WebSpark.HttpClientUtility" Version="2.0.0" />
  <PackageReference Include="WebSpark.HttpClientUtility.Crawler" Version="2.0.0" />
  <!-- Other test packages -->
</ItemGroup>
```

Test setup code:

```csharp
[TestInitialize]
public void Setup()
{
    var services = new ServiceCollection();
    
    // Register both packages
    services.AddHttpClientUtility();
    services.AddHttpClientCrawler(); // NEW
    
    _serviceProvider = services.BuildServiceProvider();
    _crawler = _serviceProvider.GetRequiredService<ISiteCrawler>();
}
```

---

## Path B: No Migration Required (Core HTTP Users)

### Step 1: Update Package Version

```powershell
# Using .NET CLI
dotnet add package WebSpark.HttpClientUtility --version 2.0.0

# Using Package Manager Console
Update-Package WebSpark.HttpClientUtility -Version 2.0.0

# Using PackageReference in .csproj
<PackageReference Include="WebSpark.HttpClientUtility" Version="2.0.0" />
```

### Step 2: Verify - That's It!

**No code changes required.** Your existing v1.x code works identically:

```csharp
// Program.cs or Startup.cs - UNCHANGED from v1.x
using WebSpark.HttpClientUtility;

var builder = WebApplication.CreateBuilder(args);

// Same DI registration as v1.x
builder.Services.AddHttpClientUtility();
// OR
builder.Services.AddHttpClientUtilityWithCaching();
// OR
builder.Services.AddHttpClientUtilityWithAllFeatures();

var app = builder.Build();
```

All your existing usage code remains identical:

```csharp
public class MyApiService
{
    private readonly IHttpRequestResultService _httpService;
    
    public MyApiService(IHttpRequestResultService httpService)
    {
        _httpService = httpService; // Unchanged
    }
    
    public async Task<WeatherData> GetWeather(string city)
    {
        var request = new HttpRequestResult<WeatherData> // Unchanged
        {
            RequestPath = $"https://api.weather.com/v1/weather/{city}",
            RequestMethod = HttpMethod.Get,
            CacheDurationMinutes = 10,
            AuthenticationProvider = new ApiKeyAuthenticationProvider("my-api-key")
        };
        
        var result = await _httpService.HttpSendRequestResultAsync(request);
        return result.ResponseResults;
    }
}
```

---

## Side-by-Side Comparison

### Authentication Example

**v1.x (Before)**:
```csharp
using WebSpark.HttpClientUtility;
using WebSpark.HttpClientUtility.Authentication;

services.AddHttpClientUtility();

var authProvider = new BearerTokenAuthenticationProvider("my-token");
var request = new HttpRequestResult<ApiResponse>
{
    RequestPath = "https://api.example.com/data",
    AuthenticationProvider = authProvider
};
```

**v2.0.0 (After - Core HTTP User)**:
```csharp
using WebSpark.HttpClientUtility;
using WebSpark.HttpClientUtility.Authentication;

services.AddHttpClientUtility(); // IDENTICAL

var authProvider = new BearerTokenAuthenticationProvider("my-token");
var request = new HttpRequestResult<ApiResponse>
{
    RequestPath = "https://api.example.com/data",
    AuthenticationProvider = authProvider
};
```

**Result**: ✅ **No changes needed**

---

### Caching Example

**v1.x (Before)**:
```csharp
using WebSpark.HttpClientUtility;

services.AddHttpClientUtilityWithCaching();

var request = new HttpRequestResult<ProductData>
{
    RequestPath = "https://api.example.com/products",
    CacheDurationMinutes = 15
};

var result = await _httpService.HttpSendRequestResultAsync(request);
```

**v2.0.0 (After - Core HTTP User)**:
```csharp
using WebSpark.HttpClientUtility;

services.AddHttpClientUtilityWithCaching(); // IDENTICAL

var request = new HttpRequestResult<ProductData>
{
    RequestPath = "https://api.example.com/products",
    CacheDurationMinutes = 15
};

var result = await _httpService.HttpSendRequestResultAsync(request);
```

**Result**: ✅ **No changes needed**

---

### Resilience (Polly) Example

**v1.x (Before)**:
```csharp
using WebSpark.HttpClientUtility;

services.AddHttpClientUtility(options =>
{
    options.EnableResilience = true;
    options.ResilienceOptions.MaxRetryAttempts = 5;
    options.ResilienceOptions.RetryDelay = TimeSpan.FromSeconds(2);
});

var request = new HttpRequestResult<OrderData>
{
    RequestPath = "https://api.example.com/orders"
};
```

**v2.0.0 (After - Core HTTP User)**:
```csharp
using WebSpark.HttpClientUtility;

services.AddHttpClientUtility(options => // IDENTICAL
{
    options.EnableResilience = true;
    options.ResilienceOptions.MaxRetryAttempts = 5;
    options.ResilienceOptions.RetryDelay = TimeSpan.FromSeconds(2);
});

var request = new HttpRequestResult<OrderData>
{
    RequestPath = "https://api.example.com/orders"
};
```

**Result**: ✅ **No changes needed**

---

### Crawler Example (Breaking Change)

**v1.x (Before)**:
```csharp
using WebSpark.HttpClientUtility;

// DI registration
services.AddHttpClientUtility(); // Crawler included

// Usage
public class CrawlerService
{
    private readonly ISiteCrawler _crawler;
    
    public CrawlerService(ISiteCrawler crawler)
    {
        _crawler = crawler;
    }
    
    public async Task<CrawlResult> Crawl(string url)
    {
        return await _crawler.CrawlAsync(url, new CrawlerOptions
        {
            MaxDepth = 3,
            MaxPages = 100
        });
    }
}
```

**v2.0.0 (After - Crawler User)**:
```csharp
using WebSpark.HttpClientUtility;
using WebSpark.HttpClientUtility.Crawler; // NEW using directive

// DI registration - TWO calls now
services.AddHttpClientUtility();   // Base
services.AddHttpClientCrawler();   // Crawler (NEW)

// Usage - IDENTICAL to v1.x
public class CrawlerService
{
    private readonly ISiteCrawler _crawler;
    
    public CrawlerService(ISiteCrawler crawler)
    {
        _crawler = crawler; // Same injection
    }
    
    public async Task<CrawlResult> Crawl(string url)
    {
        return await _crawler.CrawlAsync(url, new CrawlerOptions
        {
            MaxDepth = 3,
            MaxPages = 100
        });
    }
}
```

**Result**: ⚠️ **Changes required: Install package + add DI call + using directive**

---

## Benefits of v2.0.0

### For Core HTTP Users (No Crawler Features)

**Package Size Reduction**:
- v1.x: Downloads 1.6 MB (13 dependencies including HtmlAgilityPack, Markdig, CsvHelper, SignalR)
- v2.0.0: Downloads 1.0 MB (9 dependencies, removed 4 crawler-specific packages)
- **Savings**: 640 KB (38% reduction)

**Faster Restore and Build**:
- Fewer dependencies = faster `dotnet restore`
- Smaller NuGet cache footprint
- Reduced build time for CI/CD pipelines

**Clearer Intent**:
- Base package focused on HTTP concerns only
- No confusion about crawler features you don't use

---

### For Crawler Users

**Same Functionality, Better Organization**:
- All crawler features preserved
- Explicit dependency (know you're using crawler package)
- Better long-term maintainability

**Independent Versioning** (Future):
- Crawler package can evolve independently
- Base package stability not affected by crawler changes

---

## Troubleshooting

### Issue: "ISiteCrawler not found" after upgrade

**Solution**: Install crawler package and add using directive:
```csharp
using WebSpark.HttpClientUtility.Crawler;
```

---

### Issue: "No service for type ISiteCrawler" at runtime

**Solution**: Add crawler DI registration:
```csharp
services.AddHttpClientUtility();
services.AddHttpClientCrawler(); // Don't forget this!
```

---

### Issue: "Package not found: WebSpark.HttpClientUtility.Crawler"

**Solution**: Ensure you're using v2.0.0+ of the base package. Crawler package requires v2.0.0+.

---

### Issue: "Version conflict between base and crawler packages"

**Solution**: Both packages must have the same version. Update both:
```powershell
dotnet add package WebSpark.HttpClientUtility --version 2.0.0
dotnet add package WebSpark.HttpClientUtility.Crawler --version 2.0.0
```

---

## Testing Your Migration

### Integration Test Template

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebSpark.HttpClientUtility;
using WebSpark.HttpClientUtility.Crawler; // If using crawler

[TestClass]
public class MigrationTests
{
    private IServiceProvider _serviceProvider;
    
    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();
        
        // Register packages
        services.AddHttpClientUtility();
        
        // If using crawler:
        services.AddHttpClientCrawler();
        
        _serviceProvider = services.BuildServiceProvider();
    }
    
    [TestMethod]
    public void BaseServices_ShouldResolve()
    {
        // Verify base services registered correctly
        var httpService = _serviceProvider.GetService<IHttpRequestResultService>();
        Assert.IsNotNull(httpService, "IHttpRequestResultService should be registered");
    }
    
    [TestMethod]
    public void CrawlerServices_ShouldResolve()
    {
        // Verify crawler services registered correctly (if using crawler)
        var crawler = _serviceProvider.GetService<ISiteCrawler>();
        Assert.IsNotNull(crawler, "ISiteCrawler should be registered");
    }
}
```

### Smoke Test Checklist

- [ ] Application builds successfully
- [ ] No compiler warnings about missing types
- [ ] DI container resolves all required services
- [ ] Existing HTTP requests work identically
- [ ] (If using crawler) Crawler operations work identically
- [ ] Tests pass without modification (or with minimal DI changes)
- [ ] Package restore completes successfully

---

## Migration Timeline

**Recommended Timeline**:
1. **Week 1**: Update local development environment, run tests
2. **Week 2**: Update CI/CD pipelines, deploy to staging
3. **Week 3**: Monitor staging, deploy to production
4. **Week 4+**: Monitor production, decommission v1.x after validation

**v1.x Support Window**: 6-12 months of security and critical bug fixes

---

## Support and Resources

- **CHANGELOG**: [View full changelog](../../CHANGELOG.md)
- **API Contracts**: [Base Package Contract](./contracts/base-package-contract.md) | [Crawler Package Contract](./contracts/crawler-package-contract.md)
- **GitHub Issues**: [Report migration issues](https://github.com/MarkHazleton/HttpClientUtility/issues)
- **Documentation**: [Full documentation](https://markhazleton.github.io/WebSpark.HttpClientUtility/)

---

## Summary

| User Type | Package to Install | Code Changes Required | Migration Effort |
|-----------|-------------------|-----------------------|------------------|
| **Core HTTP Only** | WebSpark.HttpClientUtility 2.0.0 | ✅ **None** | 5 minutes (version bump only) |
| **Crawler User** | WebSpark.HttpClientUtility 2.0.0 + Crawler 2.0.0 | ⚠️ Install package + add DI call + using directive | 15-30 minutes |

**Key Takeaway**: 80%+ of users (core HTTP only) have **zero breaking changes**. Crawler users have a straightforward migration path with no runtime behavior changes.
