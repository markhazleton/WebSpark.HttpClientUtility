# Changelog

All notable changes to WebSpark.HttpClientUtility will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2025-01-25

### Improved

- **Dependency Updates**: Updated all NuGet package dependencies to their latest stable versions for improved compatibility and security
- **Package Maintenance**: Comprehensive maintenance release focusing on keeping dependencies current
- **Security Enhancements**: Leveraged security improvements and bug fixes from updated package versions
- **Compatibility Improvements**: Enhanced compatibility with latest .NET ecosystem packages
- **Stability Improvements**: General stability improvements from dependency updates

### Technical Details

- **Updated Dependencies**: All PackageReference entries updated to latest stable versions
- **Target Frameworks**: Continues to support both .NET 8.0 and .NET 9.0
- **Backward Compatibility**: No breaking API changes; maintains full backward compatibility
- **Build Process**: Verified successful compilation and testing across all target frameworks

### Package Updates

- All NuGet dependencies refreshed to latest stable versions
- Security patches included through updated dependencies
- Performance improvements inherited from updated packages
- Enhanced ecosystem compatibility

## [1.1.0] - 2025-07-01

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
