# Changelog

All notable changes to WebSpark.HttpClientUtility will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
