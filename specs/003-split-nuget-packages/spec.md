# Feature Specification: Split NuGet Package Architecture

**Feature Branch**: `003-split-nuget-packages`  
**Created**: November 3, 2025  
**Status**: ✅ Implementation Complete (November 5, 2025)  
**Input**: User description: "Refactor the single NuGet package into two separate packages, a base and a crawler, the crawler will depend on the base, but will have all the libraries/dependencies/code for the implementation of a crawler. The goal is to reduce the scope and size of the base library and keep it more focused on optimizing the IHttpClientFactory and IHttpClient libraries in our .Net applications."

## Clarifications

### Session 2025-11-03

- Q: Should the monolithic WebSpark.HttpClientUtility v1.x continue to receive updates, or will it be deprecated in favor of the split architecture? → A: Maintain v1.x for security/critical fixes only (6-12 months), recommend v2.0.0 for new features
- Q: How should version numbers be coordinated across the two packages? → A: Lockstep versioning - Both always have identical version numbers (repo-level versioning)
- Q: How should the test projects be organized for the split package architecture? → A: Two separate test projects - WebSpark.HttpClientUtility.Test and WebSpark.HttpClientUtility.Crawler.Test
- Q: Must both packages be released simultaneously, or can they be released independently? → A: Atomic releases - Both packages must be built, tested, and published together in single GitHub release
- Q: What version constraint should the crawler package specify for its dependency on the base package? → A: Exact version match - [2.0.0] requires exactly base package 2.0.0
- Q: Which users will experience breaking changes in v2.0.0? → A: Only users of crawler features will have breaking changes (must add crawler package and call AddHttpClientCrawler). Users of core HTTP features only will have zero breaking changes
- Q: Should the demo web application include a controller to demonstrate crawler functionality? → A: Yes - Add CrawlerController to WebSpark.HttpClientUtility.Web to demonstrate the new two-package DI registration pattern and showcase crawler features

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Core HTTP Client Package (Priority: P1)

A developer building a REST API client needs reliable HTTP request handling with resilience, caching, and observability features, but does not need web crawling capabilities. They want to install a focused package that includes only the essential HttpClient utilities without unnecessary dependencies.

**Why this priority**: This is the foundation use case that the majority of library consumers need. Most applications require HTTP client functionality but not web crawling. This represents the primary value proposition - optimizing IHttpClientFactory and IHttpClient usage.

**Independent Test**: Can be fully tested by installing the base package, registering services, making HTTP requests with caching/resilience/telemetry enabled, and verifying the package size is reduced by at least 40% compared to the current monolithic package.

**Acceptance Scenarios**:

1. **Given** a new .NET application, **When** developer installs the base WebSpark.HttpClientUtility package, **Then** they can register services and make HTTP requests without any crawler-related dependencies being downloaded
2. **Given** an application using the base package, **When** developer enables caching, resilience, and telemetry features, **Then** all core features work identically to the current implementation
3. **Given** a developer reviewing package dependencies, **When** they inspect the base package, **Then** they see no references to HtmlAgilityPack, SignalR, or CSV processing libraries
4. **Given** a developer comparing package sizes, **When** they measure the base package vs the current monolithic package, **Then** the base package is at least 40% smaller in total size including dependencies

---

### User Story 2 - Crawler Extension Package (Priority: P2)

A developer building a web scraping or site monitoring application needs both core HTTP client functionality and comprehensive web crawling capabilities including robots.txt parsing, sitemap processing, and SignalR progress updates. They want to install an additional package that adds crawler features on top of the base functionality.

**Why this priority**: This addresses the specialized use case of web crawling, which is valuable but required by a smaller subset of users. Making it optional allows the majority of users to avoid the overhead while still providing full functionality for those who need it.

**Independent Test**: Can be fully tested by installing both the base package and the crawler extension package, registering crawler services, initiating a site crawl, and verifying all existing crawler features work identically to the current implementation including robots.txt parsing, sitemap processing, HTML parsing, and SignalR progress reporting.

**Acceptance Scenarios**:

1. **Given** an application with the base package installed, **When** developer installs the WebSpark.HttpClientUtility.Crawler package, **Then** they gain access to all crawler-related services and configuration options
2. **Given** an application using both packages, **When** developer registers crawler services and initiates a crawl, **Then** all current crawler features work identically including robots.txt compliance, HTML link extraction, and performance tracking
3. **Given** a developer reviewing dependencies, **When** they inspect the crawler package, **Then** they see it explicitly depends on the base package and includes HtmlAgilityPack, SignalR hub infrastructure, and CSV export capabilities
4. **Given** an application using the crawler package, **When** the application makes standard HTTP requests alongside crawler operations, **Then** both capabilities coexist without conflicts

---

### User Story 3 - Backward Compatibility for Existing Users (Priority: P3)

A developer with an existing application currently using WebSpark.HttpClientUtility (the monolithic package) wants to upgrade with minimal disruption. Developers using only core HTTP features should experience no breaking changes. Developers using crawler features will need to install an additional package and update their service registration, but all runtime functionality remains identical.

**Why this priority**: While important for user satisfaction, this is lower priority than establishing the new architecture. Existing users can continue using the current version while new users benefit from the split architecture. A clear migration guide can facilitate eventual transitions.

**Independent Test**: Can be fully tested by creating two sample applications using v1.5.1: (1) core HTTP features only, and (2) with crawler features. Upgrade both to v2.0.0 and verify: (1) core-only app requires no code changes, (2) crawler app requires adding crawler package reference and calling `AddHttpClientCrawler()` but runtime behavior is identical.

**Acceptance Scenarios**:

1. **Given** an application using only core HTTP features from the current package (caching, resilience, telemetry, authentication, concurrent requests, streaming), **When** developer upgrades to the new base package v2.0.0, **Then** their existing code continues to work with zero breaking changes - same API surface, same DI registration, same runtime behavior
2. **Given** an application using crawler features from the current package, **When** developer reads the migration guide, **Then** they understand they need to install the WebSpark.HttpClientUtility.Crawler package and add `services.AddHttpClientCrawler()` to their DI registration
3. **Given** an application using crawler features, **When** developer installs both packages and updates their service registration to call both `AddHttpClientUtility()` and `AddHttpClientCrawler()`, **Then** all crawler functionality works identically to v1.5.1 (this is the expected breaking change for crawler users)
4. **Given** the development team, **When** the new architecture is released, **Then** comprehensive documentation exists showing side-by-side migration examples differentiating between core-only users (no changes needed) and crawler users (add package + DI call)

---

### Edge Cases

- What happens when a developer installs only the crawler package without the base package? The package manager should automatically resolve and install the base package as a dependency.
- What happens when a developer has both the old monolithic package and one of the new split packages installed? The package manager should detect version conflicts and require explicit resolution. Documentation should warn against mixing versions.
- Which existing users will experience breaking changes? Only users who use crawler features (SiteCrawler, SimpleSiteCrawler, AddHttpClientCrawler) will experience breaking changes and need to install the crawler package separately. Users who only use core HTTP features (caching, resilience, telemetry, authentication, concurrent requests, streaming) will experience zero breaking changes.
- How does versioning work across the two packages? Both packages maintain identical version numbers (lockstep/repo-level versioning) to eliminate compatibility confusion.
- What happens to strong name signing with multiple packages? Both packages must be signed with the same key to maintain assembly compatibility.
- How are breaking changes handled across packages? Any breaking change in the base package requires a major version bump in both packages. Breaking changes isolated to the crawler package also require a major version bump in both packages due to lockstep versioning.
- What happens when a developer uses the base package with an older version of the crawler package? With lockstep versioning and exact version match constraints, packages with different version numbers are incompatible. The crawler package specifies an exact version match requirement (e.g., [2.0.0]) for the base package, ensuring both must be updated together.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Base package MUST include all core HTTP request/response functionality including HttpRequestResult models, IHttpRequestResultService interface and implementations, authentication providers, and request/response handling
- **FR-002**: Base package MUST include optional features that apply to general HTTP operations: caching, resilience with Polly, telemetry, concurrent requests, fire-and-forget operations, streaming, and OpenTelemetry integration
- **FR-003**: Base package MUST NOT include any crawler-specific code, models, or interfaces including SiteCrawler, SimpleSiteCrawler, ISiteCrawler, CrawlerOptions, RobotsTxtParser, or CrawlHub
- **FR-004**: Base package MUST NOT depend on specialized crawler libraries including HtmlAgilityPack, Markdig, CsvHelper, or SignalR hub abstractions
- **FR-005**: Crawler package MUST depend explicitly on the base package as a package reference with exact version match constraint (e.g., [2.0.0]) to enforce lockstep versioning
- **FR-006**: Crawler package MUST include all crawler-specific code currently in the Crawler directory including SiteCrawler, SimpleSiteCrawler, ISiteCrawler, CrawlerOptions, RobotsTxtParser, CrawlHub, CrawlResult, CrawlException, and supporting classes
- **FR-007**: Crawler package MUST include specialized dependencies only needed for crawling: HtmlAgilityPack for HTML parsing, Markdig for markdown processing, CsvHelper for export functionality, and SignalR for progress reporting
- **FR-008**: Crawler package MUST provide service registration extension methods that build upon the base package's service collection extensions
- **FR-009**: Both packages MUST target the same frameworks (currently .NET 8 LTS and .NET 9) to maintain consistency
- **FR-010**: Both packages MUST be strong-name signed with the same key file to maintain assembly compatibility
- **FR-011**: Both packages MUST include comprehensive XML documentation for all public APIs
- **FR-012**: Both packages MUST include appropriate NuGet metadata: package tags, description, icon, license, and project URL
- **FR-013**: Both packages MUST pass all existing unit tests (530+ tests) without modification to test logic - only test project references and organization may change
- **FR-014**: Package versioning MUST follow semantic versioning with lockstep version numbers - both packages always maintain identical version numbers as they are versioned at the repository level
- **FR-015**: Base package MUST be publishable and functional independently without any crawler package installation
- **FR-016**: Crawler package MUST NOT function without the base package - the dependency must be enforced through package references
- **FR-017**: Both packages MUST follow the existing publishing workflow through GitHub Actions CI/CD pipeline without manual NuGet uploads, with both packages built, tested, and published atomically in a single pipeline execution
- **FR-018**: Documentation MUST be updated to reflect the new two-package architecture including installation instructions, migration guide, and feature mapping
- **FR-019**: Base package MUST preserve the existing decorator chain pattern for composing features (HttpRequestResultService → HttpRequestResultServiceCache → HttpRequestResultServicePolly → HttpRequestResultServiceTelemetry) with the same ordering requirements and all decorator implementations remaining in the base package
- **FR-020**: Demo web application (WebSpark.HttpClientUtility.Web) MUST be updated to include a CrawlerController that demonstrates proper two-package registration pattern (`AddHttpClientUtility()` + `AddHttpClientCrawler()`) and showcases crawler functionality including basic crawling, robots.txt compliance, real-time SignalR progress updates with Crawled/Discovered/Queued counters, sitemap/RSS discovery toggle, and interactive Razor UI for testing

### Package Composition

**Base Package (WebSpark.HttpClientUtility):**
- Core request/response models and interfaces
- Authentication providers (Bearer, Basic, ApiKey)
- Caching infrastructure and implementations
- Resilience with Polly integration
- Telemetry and OpenTelemetry integration
- Concurrent request handling
- Fire-and-forget operations
- Streaming capabilities
- CurlService for command generation
- MockService for testing support
- Object pooling utilities
- String converters for serialization

**Crawler Package (WebSpark.HttpClientUtility.Crawler):**
- SiteCrawler and SimpleSiteCrawler implementations
- ISiteCrawler interface
- CrawlerOptions and configuration
- RobotsTxtParser for compliance
- CrawlHub for SignalR progress reporting
- CrawlResult and CrawlException models
- HTML parsing and link extraction
- Sitemap and RSS/Atom feed discovery (sitemap.xml, rss.xml, feed.xml, atom.xml) for finding pages hidden behind JavaScript navigation
- CSV export for crawl results
- Performance tracking for crawler operations

### Assumptions

- The test suite will be reorganized into two separate test projects: WebSpark.HttpClientUtility.Test for base package tests and WebSpark.HttpClientUtility.Crawler.Test for crawler-specific tests
- The base package will maintain 100% API compatibility with v1.5.1 for all non-crawler features, ensuring zero breaking changes for users who don't use crawler functionality
- The demo web application (WebSpark.HttpClientUtility.Web) will be updated to reference both packages, add a CrawlerController with endpoints demonstrating crawler functionality, and update Program.cs to register both base and crawler services
- Documentation website will remain unified, covering both packages under the same domain
- Existing consumers of v1.5.1 and earlier are willing to make minor changes during upgrade (adding a second package reference if using crawler features)
- The split will be introduced as a new major version (v2.0.0) to signal breaking changes in package structure
- The monolithic v1.x package will be maintained for security and critical bug fixes only for 6-12 months after v2.0.0 release, with clear deprecation notices directing users to the new split architecture
- GitHub Actions workflows will be updated to build, test, and publish both packages atomically in a single CI/CD pipeline execution
- NuGet.org package naming will follow the pattern: WebSpark.HttpClientUtility (base) and WebSpark.HttpClientUtility.Crawler (extension)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Base package size (including all dependencies) is reduced by at least 40% compared to the current monolithic v1.5.1 package
- **SC-002**: Base package has fewer than 10 direct NuGet dependencies (currently 13), with crawler-specific dependencies moved to the crawler package
- **SC-003**: All 530+ existing unit tests pass with only test project organization changes - no test logic modifications required
- **SC-004**: Both packages successfully publish through GitHub Actions CI/CD pipeline without manual intervention, built and released atomically in a single pipeline execution
- **SC-005**: Documentation website is updated with installation instructions for both single-package and two-package scenarios, with migration examples for existing users
- **SC-006**: A sample application can install only the base package and make HTTP requests with caching, resilience, and telemetry enabled, downloading less than 60% of the dependencies compared to the current package
- **SC-007**: A sample application can install both packages and perform web crawling operations with identical functionality to the current v1.5.1 implementation
- **SC-008**: Package search on NuGet.org shows both packages with clear descriptions differentiating their purposes, with download counts accurately tracked separately
- **SC-009**: GitHub releases include both packages with identical version numbers (lockstep versioning) and consistent release notes, published atomically in a single release
- **SC-010**: Zero critical or high-severity security vulnerabilities in either package's dependency chain
