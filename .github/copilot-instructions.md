# WebSpark.HttpClientUtility - AI Coding Agent Instructions

## Project Overview

This is a production-ready **NuGet library** that provides a comprehensive HttpClient wrapper with resilience, caching, telemetry, and web crawling capabilities. Target frameworks: .NET 8 LTS (until Nov 2026), .NET 9, and .NET 10 LTS (until Nov 2028).

**Core Purpose**: Eliminate boilerplate HTTP code while providing enterprise-grade features through simple, fluent dependency injection.

## Architecture Pattern: Decorator Chain

The library uses a **decorator pattern** for composing features. Understanding this is critical:

```csharp
// In ServiceCollectionExtensions.cs, services are wrapped in layers:
IHttpRequestResultService service = HttpRequestResultService; // Base
→ HttpRequestResultServiceCache (if EnableCaching)           // Adds caching
→ HttpRequestResultServicePolly (if EnableResilience)        // Adds retry/circuit breaker
→ HttpRequestResultServiceTelemetry (if EnableTelemetry)     // Adds metrics (outermost)
```

**Key Point**: Order matters. Telemetry wraps everything to track total duration. Cache happens before resilience to avoid caching failed retries.

## Project Structure

- `WebSpark.HttpClientUtility/` - Main library (publishable NuGet package)
  - `ServiceCollectionExtensions.cs` - **Entry point**: All DI registration happens here
  - `RequestResult/` - Core HTTP request/response models and decorator implementations
  - `Authentication/` - Built-in auth providers (Bearer, Basic, ApiKey)
  - `Crawler/` - Web crawling with robots.txt support, SignalR progress updates
  - `MemoryCache/` - Response caching infrastructure
  - `OpenTelemetry/` - Optional OTLP integration
  - `StringConverter/` - JSON serialization abstraction (System.Text.Json default, Newtonsoft opt-in)
- `WebSpark.HttpClientUtility.Test/` - MSTest-based tests (252+ passing)
- `WebSpark.HttpClientUtility.Web/` - Demo ASP.NET Core app

## Developer Workflows

### Building & Testing

```powershell
# Restore and build (multi-targeting: net8.0, net9.0, net10.0)
dotnet restore WebSpark.HttpClientUtility.sln
dotnet build --configuration Release

# Run tests (uses MSTest, Moq for mocking)
dotnet test --configuration Release --logger "trx" --collect:"XPlat Code Coverage"
```

### Publishing Workflow

**CRITICAL**: This is a published NuGet package. Version is controlled in `.csproj` files.

**ALL NuGet package publications MUST go through GitHub Actions CI/CD pipeline. Manual publishing is strictly prohibited.**

#### Standard Release Process (ONLY Way to Publish):

1. Update version in `WebSpark.HttpClientUtility/WebSpark.HttpClientUtility.csproj` (Version property)
2. Update `CHANGELOG.md` following Keep a Changelog format
3. Commit changes: `git commit -m "chore: bump version to X.Y.Z"`
4. Tag release: `git tag vX.Y.Z && git push origin vX.Y.Z`
5. GitHub Actions (`.github/workflows/publish-nuget.yml`) **automatically and exclusively**:
   - Builds and tests
   - Packs `.nupkg` and `.snupkg` (symbol package)
   - Publishes to NuGet.org if tag starts with `v*.*.*`
   - Creates GitHub release with CHANGELOG

**NEVER**:
- ❌ Manually upload packages to NuGet.org
- ❌ Use `dotnet nuget push` locally
- ❌ Bypass the CI/CD pipeline
- ❌ Manually increment version in code changes - separate version bumps from feature work

**The GitHub Actions workflow is the single source of truth for all NuGet package releases. This ensures consistent builds, proper testing, symbol packages, and audit trails.**

## Critical Conventions

### 1. DI Registration Pattern

**Users register with ONE line**: `services.AddHttpClientUtility(options => {...})`

```csharp
// Common patterns users expect:
services.AddHttpClientUtility();                        // Basic setup
services.AddHttpClientUtilityWithCaching();             // Quick caching
services.AddHttpClientUtilityWithAllFeatures();         // Everything enabled
services.AddHttpClientUtility(o => {
    o.EnableCaching = true;
    o.EnableResilience = true;
    o.ResilienceOptions.MaxRetryAttempts = 5;
});
```

**If adding new features**, extend `HttpClientUtilityOptions` and wire up in `ServiceCollectionExtensions.AddHttpClientUtility()`. Follow existing decorator chain pattern.

### 2. Request/Response Model

All HTTP operations use `HttpRequestResult<T>`:

```csharp
var request = new HttpRequestResult<WeatherData>
{
    RequestPath = "https://api.example.com/weather",
    RequestMethod = HttpMethod.Get,
    CacheDurationMinutes = 10,              // Optional caching
    AuthenticationProvider = new BearerTokenAuthenticationProvider("token"), // Optional auth
    RequestHeaders = new Dictionary<string, string> { ... }
};

var result = await _httpService.HttpSendRequestResultAsync(request);
// result.IsSuccessStatusCode, result.ResponseResults, result.StatusCode, etc.
```

**Key properties**: `RequestPath`, `RequestMethod`, `RequestBody`, `RequestHeaders`, `CacheDurationMinutes`, `CorrelationId` (auto-generated), `AuthenticationProvider`.

### 3. Testing Standards

- **Framework**: MSTest (`[TestClass]`, `[TestMethod]`, `[TestInitialize]`)
- **Mocking**: Moq library (`Mock<IInterface>`, `MockBehavior.Loose` preferred)
- **Pattern**: Arrange-Act-Assert with clear sections
- **Naming**: `MethodName_Scenario_ExpectedBehavior`

```csharp
[TestMethod]
public async Task HttpSendRequestResultAsync_WithCaching_ReturnsCachedResult()
{
    // Arrange
    var mockService = new Mock<IHttpRequestResultService>();
    var cache = new MemoryCache(new MemoryCacheOptions());
    var service = new HttpRequestResultServiceCache(mockService.Object, logger, cache);
    
    // Act
    var result = await service.HttpSendRequestResultAsync(request);
    
    // Assert
    Assert.IsTrue(result.IsSuccessStatusCode);
    mockService.Verify(x => x.HttpSendRequestResultAsync(...), Times.Once);
}
```

### 4. Nullable Reference Types

**Enabled globally** (`<Nullable>enable</Nullable>`). All public APIs must handle nullability correctly:

```csharp
public Task<HttpRequestResult<T>?> GetAsync(string? url, CancellationToken ct = default)
{
    ArgumentNullException.ThrowIfNull(url);  // Use this pattern
    // ...
}
```

### 5. XML Documentation

**Required for all public APIs**. Format:

```csharp
/// <summary>
/// Sends an HTTP request with resilience policies applied.
/// </summary>
/// <param name="request">The HTTP request configuration.</param>
/// <param name="ct">Cancellation token.</param>
/// <returns>The HTTP response result with typed data.</returns>
/// <exception cref="ArgumentNullException">When request is null.</exception>
public async Task<HttpRequestResult<T>> SendAsync<T>(HttpRequestResult<T> request, CancellationToken ct = default)
```

### 6. Code Analysis

- **WarningLevel**: 5 (highest)
- **TreatWarningsAsErrors**: false (but keep warnings minimal)
- **EnableNETAnalyzers**: true
- **Suppress CA2007 in tests** (ConfigureAwait not needed)
- **Always review and fix warnings**: After writing new code, run `dotnet build` and address any warnings. Most warnings indicate real issues or non-idiomatic patterns that should be corrected before committing.

## Integration Points

### Polly Resilience

Used in `HttpRequestResultServicePolly`. Configuration via `HttpRequestResultPollyOptions`:
- `MaxRetryAttempts` (default: 3)
- `RetryDelay` (default: 1 second)
- `CircuitBreakerThreshold` (default: 5 failures)
- `CircuitBreakerDuration` (default: 30 seconds)

### SignalR (Crawler Feature)

`Crawler/CrawlHub.cs` broadcasts crawl progress. Requires `services.AddSignalR()` in `Crawler/ServiceCollectionExtensions`.

### OpenTelemetry

**Modern OTLP-based** (Jaeger exporter removed in v1.3.0). Use `AddWebSparkOpenTelemetry()` extension:

```csharp
services.AddWebSparkOpenTelemetry(tracerBuilder => {
    // Custom tracing configuration
});
```

### Memory Cache

Uses `Microsoft.Extensions.Caching.Memory.IMemoryCache`. Cache keys are request paths. Duration controlled by `CacheDurationMinutes` on request object.

## Common Pitfalls

1. **Breaking decorator order**: Don't reorder decorators in `ServiceCollectionExtensions` - telemetry must be outermost.
2. **Forgetting tests**: Every new feature needs test coverage. Run `dotnet test` before committing.
3. **Not multi-targeting**: Code must compile for both net8.0 and net9.0.
4. **Blocking async**: Never use `.Result` or `.Wait()`. Library code = async all the way.
5. **Strong naming**: Assembly is signed with `HttpClientUtility.snk`. Don't change signing configuration.

## AI Agent Output Organization

**CRITICAL**: All AI-generated markdown files (`.md`) must be saved in `/copilot/session-{YYYY-MM-DD}/` folders.

- Use the current date for the session folder (e.g., `/copilot/session-2025-11-02/`)
- Never create `.md` files in the root directory or other locations
- This keeps AI-generated documentation organized and separate from official project documentation
- Exception: Only update existing `.md` files in root (like `README.md`, `CHANGELOG.md`) when explicitly requested

## Key Files for Changes

- Adding features: Start with `ServiceCollectionExtensions.cs`
- Request/response models: `RequestResult/HttpRequestResult<T>.cs`
- Authentication: `Authentication/IAuthenticationProvider.cs` implementations
- Caching logic: `RequestResult/HttpRequestResultServiceCache.cs`
- Resilience: `RequestResult/HttpRequestResultServicePolly.cs`
- Crawling: `Crawler/SiteCrawler.cs`, `Crawler/CrawlerOptions.cs`

## Version History Context

- **v1.4.0** (current): Added .NET 8 LTS support, simplified DI registration
- **v1.3.2**: Fixed test reliability, improved mock patterns
- **v1.3.1**: Enhanced CurlCommandSaver with batch processing, file rotation
- **v1.3.0**: Removed deprecated Jaeger exporter, modernized OpenTelemetry to OTLP

See `CHANGELOG.md` for full history.

## Questions to Ask Before Implementing

1. Does this fit the decorator pattern? (If adding to request pipeline)
2. Should this be opt-in via `HttpClientUtilityOptions`?
3. Does this need both interface and implementation? (Follow existing patterns)
4. What's the backward compatibility impact? (This is a published library)
5. Do I need to update README, CHANGELOG, and XML docs?

## Planned Feature: Package Split (v2.0.0)

**Status**: Specification and planning complete (see `specs/003-split-nuget-packages/`)

**Objective**: Refactor the monolithic NuGet package into two separate packages to reduce size and improve modularity.

### Target Architecture

- **WebSpark.HttpClientUtility** (Base package):
  - Core HTTP utilities: authentication, caching, resilience, telemetry, concurrent requests, streaming, CURL, mock services
  - 9 dependencies (down from 13)
  - 38% smaller for users who don't need crawler features
  - **100% backward compatible** for non-crawler users (zero breaking changes)

- **WebSpark.HttpClientUtility.Crawler** (Crawler extension):
  - All web crawling functionality: SiteCrawler, SimpleSiteCrawler, robots.txt, SignalR progress, CSV export
  - 5 dependencies (base package + 4 crawler-specific deps)
  - Requires base package [2.0.0] (exact version match)
  - **Breaking change** for crawler users: must install crawler package and add `services.AddHttpClientCrawler()` call

### Key Decisions

- **Lockstep Versioning**: Both packages always have identical version numbers
- **Atomic Releases**: Single CI/CD workflow builds, tests, and publishes both packages atomically
- **Shared Signing Key**: Both packages signed with `HttpClientUtility.snk` for assembly compatibility
- **Test Split**: ~400 base tests + ~130 crawler tests in separate MSTest projects
- **Migration Path**: Clear decision tree in `specs/003-split-nuget-packages/quickstart.md` (80% of users need no code changes)

### Implementation Notes

When working on v2.0.0 package split implementation:
1. Preserve decorator chain order (Base → Cache → Polly → Telemetry) - constitutional requirement
2. All base package tests must pass without modification (zero breaking changes)
3. Crawler .csproj must specify exact base version: `<PackageReference Include="WebSpark.HttpClientUtility" Version="[2.0.0]" />`
4. Update GitHub Actions workflow to build/test/publish both packages atomically
5. CHANGELOG.md must differentiate between "no changes needed" (core users) and "migration required" (crawler users)

**Reference Documents**:
- Specification: `specs/003-split-nuget-packages/spec.md`
- Implementation Plan: `specs/003-split-nuget-packages/plan.md`
- Research: `specs/003-split-nuget-packages/research.md`
- Data Model: `specs/003-split-nuget-packages/data-model.md`
- API Contracts: `specs/003-split-nuget-packages/contracts/`
- Migration Guide: `specs/003-split-nuget-packages/quickstart.md`

