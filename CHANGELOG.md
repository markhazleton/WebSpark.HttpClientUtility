# Changelog

All notable changes to the WebSpark.HttpClientUtility project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [2.1.1] - 2025-11-12

### Fixed
- GitHub Actions: Fixed package verification step failing with exit code 1
- Added error handling to prevent grep failures
- Changed wildcard pattern to avoid matching .snupkg files

### Verified
- All workflow steps complete successfully
- Package contents verified for net8.0, net9.0, and net10.0

## [2.1.0] - 2025-11-12

### ✨ Added
- **Multi-Targeting for .NET 10 (Preview)**: All projects now target net8.0, net9.0, and net10.0
  - WebSpark.HttpClientUtility (base library)
  - WebSpark.HttpClientUtility.Crawler (crawler extension)
  - WebSpark.HttpClientUtility.Test (tests)
  - WebSpark.HttpClientUtility.Crawler.Test (crawler tests)
  - WebSpark.HttpClientUtility.Web (demo application)

### 🔧 Changed
- Updated Microsoft.Extensions.Caching.Abstractions from 8.0.0 to 10.0.0
- Updated Microsoft.Extensions.Caching.Memory from 8.0.1 to 10.0.0
- Updated Microsoft.Extensions.Http from 8.0.1 to 10.0.0
- Updated global.json to use .NET 10 SDK (10.0.100) with allowPrerelease
- Bumped package version from 2.0.0 to 2.1.0 (minor version for new framework support)

### ✅ Verified
- All 291 unique tests pass on net8.0, net9.0, and net10.0 (873 total test runs, 0 failures)
- NuGet packages correctly include all three framework targets (lib/net8.0, lib/net9.0, lib/net10.0)
- Strong-name signing intact for all targets
- No breaking changes for existing consumers

### 📝 Notes
- .NET 10 is in Preview - requires .NET 10 SDK for development
- Consumers on .NET 8 or .NET 9 are unaffected (continues using existing builds)
- This is a non-breaking, additive change
- Both base and crawler packages use lockstep versioning (both are 2.1.0)

## [2.0.0] - 2025-11-05

### 🎯 MAJOR RELEASE - Package Architecture Split

This release splits the monolithic package into two focused packages for better modularity and reduced package size.

### 📦 New Package Structure

**WebSpark.HttpClientUtility** (Base Package) - 163 KB
- Core HTTP utilities: authentication, caching, resilience, telemetry
- Supports .NET 8 LTS and .NET 9
- **10 dependencies** (down from 13 in v1.x)
- **Zero breaking changes** for users of core HTTP features

**WebSpark.HttpClientUtility.Crawler** (Extension Package) - 75 KB
- Web crawling functionality: SiteCrawler, SimpleSiteCrawler
- Robots.txt compliance, sitemap generation, HTML parsing
- SignalR progress updates, CSV export
- Requires base package [2.0.0] with exact version match
- **Breaking change**: Must install separately and call `AddHttpClientCrawler()`

### 🚀 NO BREAKING CHANGES - Core HTTP Users

If you use **only** these features, your code continues to work with zero changes:
- ✅ HTTP request/response handling (`IHttpRequestResultService`)
- ✅ Authentication providers (Bearer, Basic, API Key)
- ✅ Caching (`HttpRequestResultServiceCache`)
- ✅ Resilience with Polly (`HttpRequestResultServicePolly`)
- ✅ Telemetry (`HttpRequestResultServiceTelemetry`)
- ✅ Concurrent requests (`IConcurrentHttpRequestResultService`)
- ✅ Fire-and-forget operations (`IFireAndForgetService`)
- ✅ Streaming (`IHttpStreamingService`)
- ✅ CURL command generation (`ICurlCommandService`)
- ✅ Mock services for testing (`IMockHttpRequestResultService`)

**Migration**: Simply upgrade to v2.0.0 - no code changes required!

### ⚠️ BREAKING CHANGES - Crawler Users

If you use **web crawling features**, follow this 3-step migration:

**Step 1**: Install the crawler package
```bash
dotnet add package WebSpark.HttpClientUtility.Crawler
```

**Step 2**: Add using directive
```csharp
using WebSpark.HttpClientUtility.Crawler;
```

**Step 3**: Update DI registration
```csharp
// Before (v1.x)
services.AddHttpClientUtility();

// After (v2.0.0)
services.AddHttpClientUtility();
services.AddHttpClientCrawler();  // NEW - Required for crawler features
```

That's it! All crawler functionality (ISiteCrawler, SiteCrawler, SimpleSiteCrawler, CrawlerOptions, etc.) works identically after these changes.

### Added

- **Crawler Package**: New `WebSpark.HttpClientUtility.Crawler` package with all web crawling features
- **Atomic Releases**: Both packages always released together with identical version numbers (lockstep versioning)
- **JSON Lines Format**: CurlCommandSaver now uses `.jsonl` format instead of CSV for better streaming and parsing

### Changed

- **Package Dependencies**: Base package reduced from 13 to 10 dependencies
- **CurlCommandSaver Format**: Switched from CSV to JSON Lines format for improved debugging and tooling support
- **Lockstep Versioning**: Both packages maintain identical version numbers (2.0.0)

### Removed

- **Crawler Code from Base**: Moved SiteCrawler, SimpleSiteCrawler, ISiteCrawler, CrawlerOptions, RobotsTxtParser, CrawlHub to separate package
- **Crawler Dependencies from Base**: Removed HtmlAgilityPack, Markdig, CsvHelper dependencies from base package (now only in Crawler package)

### Technical Details

- **Base Package**: 163.23 KB, 10 runtime dependencies
- **Crawler Package**: 75.39 KB, 4 crawler-specific dependencies + base package reference
- **Total Tests**: 474 tests passing (420 base + 54 crawler) across .NET 8 and .NET 9
- **Build Quality**: Zero compiler warnings with `TreatWarningsAsErrors=true`
- **Strong Name Signing**: Both packages signed with same key for assembly compatibility

### Migration Resources

- **Documentation**: See [Migration Guide](https://markhazleton.github.io/WebSpark.HttpClientUtility/getting-started/migration-v2/)
- **Support**: For questions, open an issue on [GitHub](https://github.com/MarkHazleton/HttpClientUtility/issues)

---

## [1.5.0] - 2025-11-02

### Added

- **Comprehensive Documentation Website**: Launched complete documentation site at https://markhazleton.github.io/WebSpark.HttpClientUtility/
  - **Getting Started Guide**: Step-by-step instructions for installation and configuration
  - **Feature Documentation**: Detailed guides for caching, resilience, telemetry, authentication, and web crawling
  - **API Reference**: Complete API documentation with examples
  - **Code Examples**: Real-world usage scenarios and best practices
  - **Live NuGet Stats**: Real-time package version and download counts from NuGet API
- **Static Site Generator**: Eleventy 3.0-based documentation with sub-second build times
- **Syntax Highlighting**: Prism.js integration supporting C#, JavaScript, JSON, PowerShell, Bash, and Markup
- **Responsive Design**: Mobile-first design supporting 320px to 1920px+ viewports
- **Progressive Enhancement**: Core functionality works without JavaScript
- **Automated Deployment**: GitHub Actions workflow for continuous documentation deployment

### Improved

- **Package Metadata**: Updated NuGet package to reference new documentation website
- **Developer Experience**: Centralized documentation improves discoverability and learning
- **Performance**: Documentation site achieves 95+ Lighthouse scores for Performance, Accessibility, and SEO
- **Build Pipeline**: Automated documentation builds on every commit to main branch

### Technical Details

- **Technology Stack**: Eleventy 3.0, Node.js 20.x LTS, custom CSS with CSS custom properties
- **Build Time**: ~0.4 seconds for complete site generation (6+ pages)
- **Deployment**: GitHub Pages from /docs folder with automated GitHub Actions workflow
- **Cache Strategy**: NuGet API data cached with fallback for offline builds
- **Path Strategy**: Relative paths for maximum portability across environments

## [1.4.0] - 2025-10-15

### Added

- **.NET 8 LTS Support**: Added multi-targeting for .NET 8 (LTS) alongside .NET 9 for broader compatibility
  - Supports .NET 8.0 (LTS - Long Term Support until November 2026)
  - Supports .NET 9.0 (Standard Term Support until May 2025)
  - Allows teams on LTS versions to adopt the library immediately
- **Simplified DI Registration**: New extension methods for common configuration patterns
  - `AddHttpClientUtilityWithCaching()` - Quick setup with caching enabled
  - `AddHttpClientUtilityWithResilience()` - Quick setup with Polly resilience
  - `AddHttpClientUtilityWithAllFeatures()` - One-line registration with all features
- **Enhanced Documentation Structure**: Reorganized documentation for better discoverability
  - Separated getting started from configuration guides
  - Added migration guide for teams moving from raw HttpClient
  - Expanded examples for common scenarios

### Improved

- **Developer Experience**: Reduced setup time with convenience extension methods
- **Compatibility**: Broader ecosystem support with .NET 8 LTS targeting
- **Documentation**: Clearer organization and more comprehensive examples

### Technical Details

- **Multi-Targeting**: `<TargetFrameworks>net8.0;net9.0</TargetFrameworks>`
- **Package Validation**: Enabled baseline validation against version 1.3.2
- **Build Configuration**: Deterministic builds for reproducibility

## [1.3.2] - 2025-01-15

### Fixed

- **Test Reliability**: Fixed CurlCommandSaver tests that were failing due to incorrect mock expectations
- **Mock Setup**: Improved test mock patterns by switching from strict to loose mocking for better maintainability
- **Configuration Handling**: Tests now correctly validate graceful degradation when configuration is missing

### Technical Details

- Corrected test expectation for missing CsvOutputFolder configuration - now properly validates warning logging instead of exception throwing
- Simplified mock repository setup to use `MockBehavior.Loose` for more reliable test execution
- Added comprehensive test for configuration-less scenario to ensure curl commands are still logged even when file output is disabled
- All 252 tests now passing with improved test stability

## [1.3.1] - 2025-01-14

### Added

- **CurlCommandSaver Enhancements**: Comprehensive improvements to the CurlCommandSaver functionality
  - **Batch Processing**: Added configurable batch processing for improved performance when saving multiple cURL commands
  - **File Rotation**: Automatic file rotation based on configurable maximum file size (default: 10MB)
  - **Sensitive Data Sanitization**: Automatic redaction of sensitive headers (Authorization, API keys, tokens) and JSON content
  - **Retry Logic**: Built-in retry mechanism with exponential backoff for file operations
  - **Configuration Options**: New `CurlCommandSaverOptions` class with comprehensive configuration properties
- **Improved Resource Management**: Enhanced disposal patterns in CurlCommandSaver with proper cleanup of timers and pending records

### Improved

- **CurlCommandSaver Performance**: Batch processing can handle up to 100 commands per batch with configurable flush intervals
- **Error Handling**: More robust error handling with detailed logging for file operations
- **File Operations**: Thread-safe file operations using SemaphoreSlim for concurrent access protection
- **Logging**: Enhanced logging with correlation IDs and detailed operation metrics

### Technical Details

- **Configuration Support**: Full support for configuration via IConfiguration with fallback to defaults
- **Batch Processing Timer**: Configurable timer-based batch flushing (default: 5 seconds)
- **Sensitive Headers List**: Configurable list of headers to redact (Authorization, Api-Key, X-Api-Key, Password, Token)
- **File Rotation**: Timestamp-based file naming for rotated files (yyyyMMdd_HHmmss format)

## [1.3.0] - 2025-07-01

### Breaking Changes

- **OpenTelemetry Modernization**: Removed deprecated `OpenTelemetry.Exporter.Jaeger` package in favor of the modern OTLP exporter
- **API Changes**: Updated `AddWebSparkOpenTelemetryWithExporters` method signature:
  - Removed `jaegerEndpoint` parameter
  - Kept `otlpEndpoint` parameter (supports Jaeger, Zipkin, and other OTLP-compatible systems)

### Improved

- **Modern OpenTelemetry**: Migrated to OTLP (OpenTelemetry Protocol) exporter which provides better compatibility
- **Reduced Dependencies**: Eliminated deprecated package dependencies for a cleaner dependency tree
- **Enhanced Documentation**: Updated OpenTelemetry configuration documentation to reflect OTLP-first approach
- **Build Stability**: Resolved compilation issues related to deprecated Jaeger exporter

### Technical Details

- **OTLP Support**: The OTLP exporter supports multiple tracing backends including:
  - Jaeger (via OTLP endpoint)
  - Zipkin (via OTLP endpoint)
  - Other OTLP-compatible systems
  - Cloud-native observability platforms
- **Configuration**: Simplified configuration with single `otlpEndpoint` parameter that works with all supported backends
- **Future-Proof**: Using the recommended modern approach for OpenTelemetry exporters

## [1.0.10] - 2025-05-24

### Added

- **Streaming Support**: Implemented comprehensive streaming functionality for large HTTP responses with configurable thresholds
- **OpenTelemetry Integration**: Added full OpenTelemetry support with multiple exporters (Console, Jaeger, OTLP, InMemory)
- **OpenTelemetry Test Suite**: Complete MSTest-based integration tests for OpenTelemetry functionality
- **StreamingHelper**: New utility class for efficient processing of large responses over configurable size thresholds

### Improved

- **Build Stability**: Fixed all compilation errors and warnings for both .NET 8.0 and .NET 9.0 targets
- **Resource Management**: Completed comprehensive audit of IDisposable usage and resource cleanup patterns
- **Test Framework Consistency**: Standardized all tests to use MSTest framework instead of mixed frameworks
- **Memory Efficiency**: Enhanced processing of large HTTP responses through streaming implementation
- **HttpRequestMessagePoolPolicy**: Fixed .NET compatibility issues with HttpVersion and HttpRequestOptions

### Fixed

- **Compilation Errors**: Resolved HttpVersion reference issues in HttpRequestMessagePoolPolicy.cs
- **Duplicate Dispose**: Fixed duplicate Dispose method implementation in MemoryCacheManager.cs
- **Empty StreamingHelper**: Replaced empty StreamingHelper.cs file with complete implementation
- **Nullable Reference Warnings**: Fixed nullable reference warnings in test files
- **Package Dependencies**: Added missing OpenTelemetry exporter packages for complete telemetry support

### Technical Improvements

- **HttpRequestResultService**: Integrated StreamingHelper for efficient large response processing
- **Configuration Support**: Added streaming threshold configuration support (10MB default)
- **Async Patterns**: Proper async/await patterns with ConfigureAwait(false) throughout streaming implementation
- **Error Handling**: Maintained proper error handling and logging patterns in streaming functionality
- **Test Coverage**: All 75+ tests passing with comprehensive coverage of new features

## [1.0.9] - 2025-05-19

- Latest release.
- See previous entries for details.

## [1.0.6] - 2025-05-01

### Added

- Support for .NET 9.0
- Enhanced SignalR integration for web crawling progress updates
- Support for exporting crawl results as CSV

### Improved

- Performance optimizations for concurrent HTTP processing
- Memory efficiency for large crawling operations
- Better robots.txt parsing with caching

### Fixed

- Connection pooling issue with high-volume requests
- Race condition in the FireAndForgetUtility
- Serialization errors with complex nested objects

## [1.0.5] - 2024-12-15

### Added

- Support for custom serialization options
- Option to limit crawling to specific domains
- Additional telemetry data points

### Improved

- Performance of concurrent processor
- Error handling for transient HTTP failures
- Memory usage during large concurrent operations

### Fixed

- Issue with cache expiration timing
- Thread safety in ConcurrentProcessor
- Rare deadlock in WebCrawler implementation

## [1.0.4] - 2024-09-10

### Added

- Robots.txt parsing and compliance
- Improved sitemap generation with priority and changefreq support
- Ability to save crawled pages to disk

### Improved

- HTML parsing efficiency
- Link extraction algorithm
- Crawler politeness with adaptive rate limiting

## [1.0.3] - 2024-07-20

### Added

- Enhanced telemetry with detailed request duration tracking
- Support for custom correlation IDs
- Query parameter builder utility

### Improved

- Documentation and examples
- Performance of HTTP response handling
- Error reporting detail

## [1.0.2] - 2024-05-05

### Added

- Support for Polly resilience patterns
- Circuit breaker implementation
- Exponential backoff retry strategy

### Improved

- Exception handling
- Logging with structured data
- Test coverage

## [1.0.1] - 2024-03-15

### Added

- Initial release with core HTTP client functionality
- Basic caching support
- Simple telemetry
- Concurrent request processing
- Web crawling capabilities
