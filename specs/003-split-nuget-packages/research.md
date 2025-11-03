# Research: Split NuGet Package Architecture

**Feature**: 003-split-nuget-packages  
**Date**: November 3, 2025  
**Status**: Complete

## Overview

This document captures research findings and architectural decisions for splitting WebSpark.HttpClientUtility into two NuGet packages: a base package for core HTTP utilities and a crawler extension package.

---

## Research Question 1: Package Dependency Split

### Context
Current v1.5.1 package has 13 direct NuGet dependencies. Goal is to reduce base package to <10 dependencies while preserving all functionality through the crawler extension.

### Analysis

**Current Dependencies (v1.5.1)**:
1. CsvHelper 33.1.0 - CSV export for crawl results
2. HtmlAgilityPack 1.12.4 - HTML parsing for crawler
3. Markdig 0.43.0 - Markdown processing for crawler
4. Microsoft.Extensions.Caching.Abstractions 8.0.0 - Caching interface
5. Microsoft.Extensions.Http 8.0.1 - HttpClientFactory
6. Microsoft.SourceLink.GitHub 8.0.0 - Source debugging (PrivateAssets)
7. Newtonsoft.Json 13.0.4 - JSON serialization option
8. OpenTelemetry 1.13.1 - Telemetry core
9. OpenTelemetry.Instrumentation.Http 1.13.0 - HTTP instrumentation
10. OpenTelemetry.Extensions.Hosting 1.13.1 - Hosting integration
11. OpenTelemetry.Exporter.Console 1.13.1 - Console exporter
12. OpenTelemetry.Exporter.OpenTelemetryProtocol 1.13.1 - OTLP exporter
13. Polly 8.6.4 - Resilience (retry, circuit breaker)
14. Microsoft.AspNetCore.App (framework reference) - SignalR, ObjectPool

**Dependency Classification**:
- **Crawler-Specific** (3): CsvHelper, HtmlAgilityPack, Markdig - only used in Crawler/ directory
- **Core HTTP** (10): Microsoft.Extensions.*, OpenTelemetry.*, Polly, Newtonsoft.Json
- **Testing-Only** (1): OpenTelemetry.Exporter.InMemory - only used in tests, not shipped
- **Framework** (1): Microsoft.AspNetCore.App - needed for SignalR (crawler) and ObjectPool (base)

### Decision

**Base Package Dependencies (9 total)**:
- Microsoft.Extensions.Caching.Abstractions 8.0.0
- Microsoft.Extensions.Http 8.0.1
- Microsoft.SourceLink.GitHub 8.0.0 (PrivateAssets=All)
- Newtonsoft.Json 13.0.4
- OpenTelemetry 1.13.1
- OpenTelemetry.Instrumentation.Http 1.13.0
- OpenTelemetry.Extensions.Hosting 1.13.1
- OpenTelemetry.Exporter.Console 1.13.1
- OpenTelemetry.Exporter.OpenTelemetryProtocol 1.13.1
- Polly 8.6.4

**Crawler Package Additional Dependencies (5 total)**:
- WebSpark.HttpClientUtility [2.0.0] (exact version match)
- CsvHelper 33.1.0
- HtmlAgilityPack 1.12.4
- Markdig 0.43.0
- Microsoft.AspNetCore.App (framework reference for SignalR)

**Note**: ObjectPool utilities in base package can use in-memory implementation without framework reference, or continue using framework reference for both packages.

### Rationale

- Achieves <10 base dependencies (9 vs. 13)
- Removes 3 crawler-specific packages from base (24% dependency reduction)
- OpenTelemetry suite stays in base (observability is core concern)
- Polly stays in base (resilience is core concern)
- Newtonsoft.Json stays in base (serialization option for non-crawler users)

### Alternatives Considered

**Alternative A**: Move OpenTelemetry to separate package
- **Rejected**: Observability is constitutional requirement; forcing users to install separate package for telemetry violates "one-line DI" principle

**Alternative B**: Move Polly to separate package
- **Rejected**: Resilience (retry, circuit breaker) is core HTTP concern; most production apps need retry logic

**Alternative C**: Keep all dependencies in base, use feature flags
- **Rejected**: NuGet downloads all dependencies regardless of feature flags; doesn't achieve package size reduction goal

---

## Research Question 2: Strong Name Signing Strategy

### Context
Constitution requires both packages to be strong-name signed. Current package uses `HttpClientUtility.snk`. Need to determine if both packages use same key or separate keys.

### Analysis

**Same Key Approach**:
- ✅ Maintains assembly compatibility between packages
- ✅ Simplifies signing infrastructure (one key to manage)
- ✅ Enables GAC scenarios where both packages coexist
- ✅ No additional key generation or management overhead
- ⚠️ If key compromised, both packages affected

**Separate Keys Approach**:
- ✅ Isolates security risk (key compromise affects one package)
- ❌ Complicates infrastructure (two keys to manage)
- ❌ May cause assembly binding issues if types cross package boundaries
- ❌ Requires additional documentation and key distribution
- ❌ Violates constitution's "simplicity" principle

### Decision

**Use the same `HttpClientUtility.snk` key for both packages.**

**Implementation**:
```xml
<!-- Both WebSpark.HttpClientUtility.csproj and WebSpark.HttpClientUtility.Crawler.csproj -->
<PropertyGroup>
    <SignAssembly>true</SignAssembly>
    <AssemblyOriginatorKeyFile>../HttpClientUtility.snk</AssemblyOriginatorKeyFile>
    <PublicSign Condition="'$(OS)' == 'Windows_NT'">false</PublicSign>
    <PublicSign Condition="'$(OS)' != 'Windows_NT'">true</PublicSign>
    <EnableStrongNaming>true</EnableStrongNaming>
    <DelaySign>false</DelaySign>
</PropertyGroup>
```

### Rationale

- Maintains assembly compatibility (types from base package can be referenced in crawler without issues)
- Simplifies key management (single key in repository root)
- Preserves existing signing infrastructure
- Aligns with lockstep versioning strategy (both packages are tightly coupled)
- Key security mitigated by GitHub secret management and access controls

### Alternatives Considered

**Alternative A**: Generate new key for crawler package
- **Rejected**: Adds complexity without material security benefit; both packages in same repo with same access controls

**Alternative B**: Don't sign crawler package
- **Rejected**: Violates constitution requirement; creates inconsistency; breaks GAC scenarios

---

## Research Question 3: GitHub Actions Atomic Publishing

### Context
Constitution requires atomic releases via CI/CD pipeline. Current workflow builds and publishes single package. Need to extend for two packages while maintaining atomic behavior.

### Analysis

**Current Workflow** (`.github/workflows/publish-nuget.yml`):
1. Trigger on `v*.*.*` tag push
2. Build solution
3. Run tests
4. Pack package
5. Publish to NuGet.org
6. Create GitHub release

**Requirements for Two Packages**:
- Both packages must build successfully before either publishes
- Both test suites must pass before either publishes
- Both packages must publish to NuGet.org or neither
- GitHub release must include both .nupkg files
- Maintain existing trigger mechanism (Git tag)

### Decision

**Extend existing workflow to build/pack/publish both packages atomically.**

**Workflow Structure**:
```yaml
name: Publish NuGet Packages

on:
  push:
    tags:
      - 'v*.*.*'

jobs:
  publish:
    runs-on: ubuntu-latest
    steps:
      - name: Checkout
        uses: actions/checkout@v4
        
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: |
            8.0.x
            9.0.x
      
      - name: Restore dependencies
        run: dotnet restore WebSpark.HttpClientUtility.sln
      
      - name: Build solution (both packages)
        run: dotnet build --configuration Release --no-restore WebSpark.HttpClientUtility.sln
      
      - name: Run all tests (base + crawler)
        run: dotnet test --configuration Release --no-build --verbosity normal
      
      - name: Pack base package
        run: dotnet pack WebSpark.HttpClientUtility/WebSpark.HttpClientUtility.csproj --configuration Release --no-build --output ./artifacts
      
      - name: Pack crawler package
        run: dotnet pack WebSpark.HttpClientUtility.Crawler/WebSpark.HttpClientUtility.Crawler.csproj --configuration Release --no-build --output ./artifacts
      
      - name: Publish base package to NuGet
        run: dotnet nuget push ./artifacts/WebSpark.HttpClientUtility.*.nupkg --api-key ${{ secrets.NUGET_API_KEY }} --source https://api.nuget.org/v3/index.json --skip-duplicate
      
      - name: Publish crawler package to NuGet
        run: dotnet nuget push ./artifacts/WebSpark.HttpClientUtility.Crawler.*.nupkg --api-key ${{ secrets.NUGET_API_KEY }} --source https://api.nuget.org/v3/index.json --skip-duplicate
      
      - name: Create GitHub Release
        uses: softprops/action-gh-release@v1
        with:
          files: ./artifacts/*.nupkg
          generate_release_notes: true
```

**Key Properties**:
- Single job executes all steps sequentially
- Test failure prevents any publishing (atomic behavior)
- Both packages packed and published in same workflow run
- GitHub release includes both .nupkg files
- Existing tag trigger mechanism preserved

### Rationale

- Atomic behavior: Any failure (build, test, pack, or first publish) prevents second publish
- Maintains constitutional compliance (no manual publishing)
- Simplicity: Single workflow file, single job
- Existing infrastructure: Reuses current secrets, runners, and triggers

### Alternatives Considered

**Alternative A**: Separate workflows for each package
- **Rejected**: Breaks atomic release requirement; creates window where one package published but other fails

**Alternative B**: Manual approval step between packages
- **Rejected**: Introduces human delay; violates constitution's "no manual publishing" rule; adds operational complexity

**Alternative C**: Matrix build for packages
- **Rejected**: Matrix runs in parallel, harder to enforce atomic behavior; unnecessary complexity for two packages

---

## Research Question 4: Test Project Split Strategy

### Context
Current test suite has 530+ tests in single MSTest project. Need to split tests across two projects (base + crawler) while maintaining test patterns, coverage, and organization.

### Analysis

**Current Test Distribution** (estimated from codebase structure):
- Authentication tests: ~40 tests
- Caching tests: ~35 tests
- Concurrent request tests: ~25 tests
- CURL service tests: ~30 tests
- Fire-and-forget tests: ~20 tests
- Mock service tests: ~25 tests
- Object pool tests: ~15 tests
- OpenTelemetry tests: ~40 tests
- Query string tests: ~20 tests
- Request/result tests: ~80 tests
- Streaming tests: ~30 tests
- String converter tests: ~35 tests
- Crawler tests: ~80-130 tests

**Test Organization Patterns**:
- MSTest framework with `[TestClass]`, `[TestMethod]`, `[TestInitialize]`
- Moq for mocking dependencies
- Arrange-Act-Assert pattern
- Naming: `MethodName_Scenario_ExpectedBehavior`

### Decision

**Create two test projects mirroring package structure**:

**WebSpark.HttpClientUtility.Test** (Base package tests, ~400-450 tests):
- ServiceCollectionExtensionsTests.cs (base DI tests)
- HttpGetCallServiceTelemetryTests.cs
- Authentication/ directory
- MemoryCache/ directory
- RequestResult/ directory
- OpenTelemetry/ directory
- Concurrent/ directory
- FireAndForget/ directory
- Streaming/ directory
- CurlService/ directory
- MockService/ directory
- ObjectPool/ directory
- StringConverter/ directory
- QueryString/ directory
- HttpResponse/ directory

**WebSpark.HttpClientUtility.Crawler.Test** (Crawler package tests, ~80-130 tests):
- ServiceCollectionExtensionsTests.cs (crawler DI tests)
- SiteCrawlerTests.cs
- SimpleSiteCrawlerTests.cs
- RobotsTxtParserTests.cs
- CrawlerOptionsTests.cs
- CrawlHubTests.cs
- CrawlerPerformanceTrackerTests.cs
- Additional crawler-specific test files

**Migration Process**:
1. Create `WebSpark.HttpClientUtility.Crawler.Test` project with same MSTest + Moq setup
2. Identify all crawler-related test files (grep for "Crawler", "ISiteCrawler", "RobotsTxt", etc.)
3. Copy identified files to new test project
4. Update namespaces from `WebSpark.HttpClientUtility.Test` to `WebSpark.HttpClientUtility.Crawler.Test`
5. Update project references (crawler tests reference both base + crawler packages)
6. Remove crawler test files from base test project
7. Run both test projects independently to verify no regressions
8. Verify total test count remains 530+

**Test Project Configuration**:
```xml
<!-- WebSpark.HttpClientUtility.Test.csproj -->
<ItemGroup>
  <PackageReference Include="Microsoft.NET.Test.Sdk" />
  <PackageReference Include="MSTest.TestAdapter" />
  <PackageReference Include="MSTest.TestFramework" />
  <PackageReference Include="Moq" />
  <ProjectReference Include="..\WebSpark.HttpClientUtility\WebSpark.HttpClientUtility.csproj" />
</ItemGroup>

<!-- WebSpark.HttpClientUtility.Crawler.Test.csproj -->
<ItemGroup>
  <PackageReference Include="Microsoft.NET.Test.Sdk" />
  <PackageReference Include="MSTest.TestAdapter" />
  <PackageReference Include="MSTest.TestFramework" />
  <PackageReference Include="Moq" />
  <ProjectReference Include="..\WebSpark.HttpClientUtility\WebSpark.HttpClientUtility.csproj" />
  <ProjectReference Include="..\WebSpark.HttpClientUtility.Crawler\WebSpark.HttpClientUtility.Crawler.csproj" />
</ItemGroup>
```

### Rationale

- Mirrors package structure for clarity
- Enables independent test execution (base tests don't require crawler package)
- Maintains all existing test patterns (MSTest, Moq, naming conventions)
- No test logic modifications required (move files only)
- Clear separation of concerns

### Alternatives Considered

**Alternative A**: Keep single test project referencing both packages
- **Rejected**: Doesn't enable independent testing; base tests would unnecessarily depend on crawler package

**Alternative B**: Three test projects (base, crawler, integration)
- **Rejected**: Adds unnecessary complexity; integration tests not needed for library packages where contracts are clear

**Alternative C**: Shared test utilities project
- **Rejected**: Current test suite doesn't have significant shared utilities; premature abstraction

---

## Research Question 5: Breaking Change Communication Strategy

### Context
v2.0.0 is a MAJOR version bump per semantic versioning due to package split. Need clear communication strategy for existing users, distinguishing between those affected (crawler users) and unaffected (core HTTP users).

### Analysis

**User Segments**:
1. **Core HTTP Only** (~80% estimated): Use caching, resilience, telemetry, auth, concurrent, streaming
   - Impact: ZERO breaking changes
   - Action: None (upgrade seamlessly)

2. **Crawler Users** (~20% estimated): Use SiteCrawler, SimpleSiteCrawler, ISiteCrawler
   - Impact: Breaking change (package not found)
   - Action: Install crawler package + add DI call

**Communication Channels**:
- CHANGELOG.md (primary)
- NuGet.org package description
- GitHub release notes
- README.md (migration guide section)
- Documentation website

### Decision

**Multi-channel communication with segment-specific guidance**:

**CHANGELOG.md Format** (Keep a Changelog style):
```markdown
## [2.0.0] - 2025-MM-DD

### BREAKING CHANGES

**Package Split**: WebSpark.HttpClientUtility split into two packages for improved modularity and reduced package size.

**✅ NO BREAKING CHANGES if you use ONLY these features**:
- HTTP request/response handling
- Authentication (Bearer, Basic, ApiKey)
- Caching
- Resilience (Polly retry, circuit breaker)
- Telemetry and OpenTelemetry integration
- Concurrent requests
- Fire-and-forget operations
- Streaming
- CURL command generation
- Mock services for testing

**⚠️ BREAKING CHANGE if you use Crawler features**:

The web crawling functionality has been moved to a new package: `WebSpark.HttpClientUtility.Crawler`.

**Migration Steps for Crawler Users**:
1. Install the crawler package:
   ```bash
   dotnet add package WebSpark.HttpClientUtility.Crawler
   ```

2. Update your DI registration (in `Program.cs` or `Startup.cs`):
   ```csharp
   // Before (v1.x):
   services.AddHttpClientUtility(); // Crawler features included

   // After (v2.0.0):
   services.AddHttpClientUtility();   // Base features
   services.AddHttpClientCrawler();   // Crawler features (NEW)
   ```

3. Update using directives if needed:
   ```csharp
   // Add this using:
   using WebSpark.HttpClientUtility.Crawler;
   ```

4. Runtime behavior is identical to v1.5.1

**Migration Steps for Core HTTP Users**:
- No changes required. Upgrade package version and rebuild.

### Added
- New `WebSpark.HttpClientUtility.Crawler` package with all web crawling functionality
- Lockstep versioning across both packages (both always same version)
- Improved package modularity and size

### Changed
- **Base package size reduced by 40%+** (removed HtmlAgilityPack, Markdig, CsvHelper dependencies)
- **Base package dependencies reduced from 13 to 9**
- Crawler functionality moved to separate package

### Removed
- Crawler features removed from base package (available in new Crawler package)
- HtmlAgilityPack, Markdig, CsvHelper removed from base package dependencies

### Deprecated
- **WebSpark.HttpClientUtility v1.x** will receive security and critical bug fixes only for 6-12 months after v2.0.0 release. All new features will target v2.0.0+. Users are encouraged to migrate to v2.0.0.

### Fixed
- None (this is an architectural refactoring release)

---

For detailed migration guidance, see [Migration Guide](./MIGRATION.md).
```

**GitHub Release Notes**:
- Link to CHANGELOG.md
- Highlight two-tier impact (no change for core users, simple migration for crawler users)
- Include package download links

**README.md Update**:
Add "Migration from v1.x" section with quick links and examples.

**NuGet.org Package Descriptions**:
- Base package: "Lightweight core HTTP utilities (v2.0+)"
- Crawler package: "Web crawling extension (requires base package)"

### Rationale

- Segment-specific messaging reduces confusion (core users see "no changes")
- Step-by-step migration guide for affected users
- Multiple communication channels ensure visibility
- CHANGELOG follows industry standard (Keep a Changelog format)
- Deprecation notice gives v1.x users 6-12 month runway

### Alternatives Considered

**Alternative A**: Single "breaking change" message for all users
- **Rejected**: Creates unnecessary alarm for core-only users who aren't affected

**Alternative B**: Separate CHANGELOGs for each package
- **Rejected**: Complicates version history; users need to cross-reference; violates lockstep versioning principle

**Alternative C**: Blog post announcement
- **Rejected**: Adds communication channel burden; CHANGELOG + release notes sufficient for package ecosystem

---

## Research Question 6: Package Metadata Differentiation

### Context
Both packages will appear in NuGet.org search results. Need clear differentiation to help users choose the correct package(s) and understand the relationship.

### Analysis

**NuGet.org Search Behavior**:
- Users search for "HttpClient", "web crawler", "resilience", etc.
- Package summary appears in search results (80 characters max)
- Package description visible on detail page
- Tags influence search ranking and filtering

**Differentiation Requirements**:
1. Base package should emphasize "core" and "lightweight"
2. Crawler package should emphasize "extension" and "requires base"
3. Tags should be distinct but related
4. Icons should be visually related (same base design)
5. Descriptions should cross-reference each other

### Decision

**Base Package Metadata** (`WebSpark.HttpClientUtility.csproj`):
```xml
<PropertyGroup>
  <PackageId>WebSpark.HttpClientUtility</PackageId>
  <Version>2.0.0</Version>
  <Authors>MarkHazleton</Authors>
  <Company>MarkHazleton</Company>
  <Copyright>Copyright © MarkHazleton 2025</Copyright>
  
  <Summary>Lightweight HttpClient wrapper with resilience, caching, and telemetry for .NET 8+ applications.</Summary>
  
  <Description>
    Core HTTP client utilities for .NET applications, including authentication providers 
    (Bearer, Basic, ApiKey), response caching, resilience with Polly (retry, circuit breaker), 
    OpenTelemetry integration, concurrent request handling, fire-and-forget operations, and 
    streaming capabilities. Lightweight and focused on IHttpClientFactory optimization.
    
    For web crawling capabilities, see WebSpark.HttpClientUtility.Crawler package.
    
    Supports .NET 8 (LTS) and .NET 9.
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
</PropertyGroup>
```

**Crawler Package Metadata** (`WebSpark.HttpClientUtility.Crawler.csproj`):
```xml
<PropertyGroup>
  <PackageId>WebSpark.HttpClientUtility.Crawler</PackageId>
  <Version>2.0.0</Version>
  <Authors>MarkHazleton</Authors>
  <Company>MarkHazleton</Company>
  <Copyright>Copyright © MarkHazleton 2025</Copyright>
  
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
    
    Supports .NET 8 (LTS) and .NET 9.
  </Description>
  
  <PackageTags>
    crawler;web-crawler;site-crawler;web-scraping;scraping;spider;
    robots-txt;robotstxt;sitemap;sitemapxml;html-parsing;htmlagilitypack;
    signalr;csv;export;dotnet;aspnetcore;net8;net9;
    httpclient-extension;webspark
  </PackageTags>
  
  <PackageProjectUrl>https://markhazleton.github.io/WebSpark.HttpClientUtility/</PackageProjectUrl>
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <PackageReadmeFile>README.md</PackageReadmeFile>
  <PackageIcon>icon-crawler.png</PackageIcon>
  <RepositoryUrl>https://github.com/MarkHazleton/HttpClientUtility</RepositoryUrl>
  <RepositoryType>git</RepositoryType>
</PropertyGroup>
```

**Icon Strategy**:
- Base package: Use existing `icon.png` (HTTP-focused)
- Crawler package: Create `icon-crawler.png` (same design with spider/web element)

**README Files**:
- Both packages include README.md with package-specific quick start
- Cross-reference each other in README

### Rationale

- Summary emphasizes key differentiators (lightweight vs. extension, core vs. crawler)
- Description cross-references counterpart package
- Tags are distinct but related (users searching "crawler" find crawler package, "httpclient" finds base)
- Same license, authors, URL maintain brand consistency
- Different icons provide visual distinction in search results

### Alternatives Considered

**Alternative A**: Identical tags for both packages
- **Rejected**: Users searching "crawler" would see both packages; confusing for those who only need base

**Alternative B**: No cross-references in descriptions
- **Rejected**: Users finding one package might not discover the other; reduces discoverability

**Alternative C**: Separate websites for each package
- **Rejected**: Increases maintenance burden; unified documentation is simpler

---

## Summary of Key Decisions

1. **Dependencies**: Base 9, Crawler +5 (including base reference)
2. **Signing**: Same key (`HttpClientUtility.snk`) for both packages
3. **CI/CD**: Single atomic workflow building/testing/publishing both packages
4. **Testing**: Two MSTest projects mirroring package structure
5. **Communication**: Segment-specific breaking change messaging (core vs. crawler users)
6. **Metadata**: Differentiated summaries/tags, cross-referencing descriptions

All decisions align with constitutional requirements and achieve stated goals:
- ✅ Base package <10 dependencies (9)
- ✅ Base package size reduced 40%+ (removed 3 heavy dependencies)
- ✅ Zero breaking changes for core HTTP users
- ✅ Atomic releases via CI/CD
- ✅ Lockstep versioning
- ✅ Strong naming preserved
- ✅ Test coverage maintained (530+ tests)
