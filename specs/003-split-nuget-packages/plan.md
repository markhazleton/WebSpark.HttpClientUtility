# Implementation Plan: Split NuGet Package Architecture

**Branch**: `003-split-nuget-packages` | **Date**: November 3, 2025 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/003-split-nuget-packages/spec.md`

## Summary

Refactor the monolithic WebSpark.HttpClientUtility NuGet package into two separate packages:

1. **WebSpark.HttpClientUtility (Base)**: Core HTTP client utilities focused on IHttpClientFactory/IHttpClient optimization, including authentication, caching, resilience, telemetry, and streaming - with zero breaking changes for existing users of these features.

2. **WebSpark.HttpClientUtility.Crawler (Extension)**: Web crawling capabilities with robots.txt compliance, sitemap generation, SignalR progress updates, and HTML parsing - requiring users to install additional package and call `AddHttpClientCrawler()`.

**Goals**: Reduce base package size by 40%+, decrease direct dependencies from 13 to <10, enable users to pay only for features they need, while maintaining 100% backward compatibility for non-crawler features.

## Technical Context

**Language/Version**: C# / .NET 8 (LTS) and .NET 9  
**Primary Dependencies**:
- Base Package: Microsoft.Extensions.* (DI, Http, Caching), Polly 8.6.4, OpenTelemetry 1.13.1, Newtonsoft.Json 13.0.4
- Crawler Package: HtmlAgilityPack 1.12.4, Markdig 0.43.0, CsvHelper 33.1.0, Microsoft.AspNetCore.SignalR (framework reference)

**Storage**: Not applicable (stateless library)  
**Testing**: MSTest with Moq - maintain 530+ passing tests across both packages  
**Target Platform**: Cross-platform .NET applications (Windows, Linux, macOS) targeting .NET 8+ runtimes  
**Project Type**: Multi-project NuGet library solution (2 library projects + 2 test projects + 1 demo web app)  
**Performance Goals**: 
- Package size reduction: Base package 40%+ smaller than current v1.5.1 monolithic package
- Dependency reduction: Base package <10 direct NuGet dependencies (currently 13)
- Test execution: All 530+ tests pass in <5 minutes
- Build time: Full solution build (both packages) <2 minutes

**Constraints**:
- Zero breaking changes for non-crawler features (backward compatibility critical)
- Lockstep versioning: Both packages always have identical version numbers
- Atomic releases: Both packages built, tested, and published together in single CI/CD pipeline
- Strong name signing: Both packages signed with same key (HttpClientUtility.snk)
- Decorator pattern preservation: Base package must maintain existing decorator chain architecture
- Multi-targeting: Both packages compile for .NET 8 LTS and .NET 9

**Scale/Scope**:
- Current: 1 NuGet package, 530+ tests, 13 direct dependencies, ~50 public APIs
- Target: 2 NuGet packages (base + crawler), 530+ tests split across 2 test projects, base <10 dependencies, crawler ~4 additional dependencies
- Code split: ~15-20% of codebase moves to crawler package (Crawler/ directory + related tests)
- User impact: 100% of non-crawler users (estimated 80%+ of user base) experience zero breaking changes

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### ✅ **PASS**: Library-First Design
- Both packages remain self-contained, independently testable NuGet libraries
- Interface-based design preserved (IHttpRequestResultService, ISiteCrawler)
- Decorator pattern explicitly preserved in base package (FR-019)
- Opt-in features maintained via configuration

### ✅ **PASS**: Test Coverage and Quality
- All 530+ existing tests preserved with only organizational changes
- MSTest framework continues across both test projects
- Moq patterns maintained
- No test logic modifications required (FR-013)

### ✅ **PASS**: Multi-Targeting and Compatibility
- Both packages target .NET 8 LTS and .NET 9 (FR-009)
- MAJOR version bump (v2.0.0) signals package structure changes
- Nullable reference types remain enabled
- Base package maintains 100% API compatibility for non-crawler features

### ✅ **PASS**: One-Line Developer Experience
- Base package users: `services.AddHttpClientUtility()` unchanged
- Crawler users: Add `services.AddHttpClientCrawler()` (expected breaking change)
- Configuration model preserved
- No additional boilerplate required

### ✅ **PASS**: Observability and Diagnostics
- Correlation IDs, structured logging, telemetry remain in base package
- No changes to observability features
- OpenTelemetry integration stays in base package

### ✅ **PASS**: Versioning and Release Discipline
- Lockstep versioning enforced (both packages always same version)
- Atomic releases via single GitHub Actions pipeline (FR-017)
- CHANGELOG.md updated to document v2.0.0 breaking changes
- Git tagging triggers automated publishing
- Manual publishing remains prohibited

### ✅ **PASS**: Decorator Pattern Architecture
- FR-019 explicitly requires preserving decorator chain in base package
- All decorators remain in base: Cache → Polly → Telemetry
- Order and composition unchanged
- Crawler package uses base package decorators via dependency

### 🟡 **JUSTIFIED VIOLATION**: Project Structure Complexity

**Violation**: Constitution assumes simpler project structures; this splits into 4 projects (2 libraries + 2 test projects).

**Why Needed**: 
- Current monolithic package forces all users to download crawler dependencies (HtmlAgilityPack, SignalR, CSV)
- Package size bloat (40%+ overhead) affects majority of users who don't need crawling
- Dependency reduction (13 → <10 for base) critical for lightweight HttpClient scenarios

**Simpler Alternative Rejected Because**:
- Single package with conditional dependencies: NuGet doesn't support runtime conditional dependencies
- Feature flags: Would still require all dependencies to be downloaded
- Separate repo: Would break shared code, versioning, and release coordination

**Mitigation**:
- Lockstep versioning eliminates version mismatch concerns
- Atomic releases ensure both packages tested together
- Strong name signing with same key maintains assembly compatibility
- Shared test patterns and infrastructure

### **Constitution Compliance Summary**: ✅ APPROVED

All constitutional requirements met. Project structure complexity is justified by concrete user pain (40% package bloat) and technical limitations (NuGet dependency model). Mitigation strategies ensure architectural integrity maintained.

## Project Structure

### Documentation (this feature)

```text
specs/003-split-nuget-packages/
├── spec.md              # Feature specification (completed)
├── plan.md              # This file (implementation plan)
├── research.md          # Phase 0 output (architecture decisions, dependency analysis)
├── data-model.md        # Phase 1 output (package metadata, dependency graph)
├── quickstart.md        # Phase 1 output (migration guide)
├── contracts/           # Phase 1 output (package contracts, API surface)
│   ├── base-package-contract.md      # Base package public API contract
│   └── crawler-package-contract.md   # Crawler package public API contract
└── tasks.md             # Phase 2 output (NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
WebSpark.HttpClientUtility/          # BASE PACKAGE - Core HTTP utilities
├── ServiceCollectionExtensions.cs   # DI registration (no changes for base features)
├── RequestResult/                    # Core request/response models & decorators
│   ├── HttpRequestResult.cs
│   ├── IHttpRequestResultService.cs
│   ├── HttpRequestResultService.cs  # Base implementation
│   ├── HttpRequestResultServiceCache.cs     # Decorator
│   ├── HttpRequestResultServicePolly.cs     # Decorator
│   └── HttpRequestResultServiceTelemetry.cs # Decorator
├── Authentication/                   # Auth providers (Bearer, Basic, ApiKey)
├── MemoryCache/                      # Caching infrastructure
├── OpenTelemetry/                    # OTLP integration
├── Concurrent/                       # Parallel request handling
├── FireAndForget/                    # Fire-and-forget operations
├── Streaming/                        # Streaming capabilities
├── CurlService/                      # CURL command generation
├── MockService/                      # Testing support
├── ObjectPool/                       # Object pooling utilities
├── StringConverter/                  # JSON serialization abstraction
├── Utilities/                        # Shared utilities
└── WebSpark.HttpClientUtility.csproj # Project file (update dependencies)

WebSpark.HttpClientUtility.Crawler/  # NEW CRAWLER PACKAGE
├── ServiceCollectionExtensions.cs   # NEW - AddHttpClientCrawler() registration
├── ISiteCrawler.cs                  # MOVED from base
├── SiteCrawler.cs                   # MOVED from base/Crawler/
├── SimpleSiteCrawler.cs             # MOVED from base/Crawler/
├── CrawlerOptions.cs                # MOVED from base/Crawler/
├── RobotsTxtParser.cs               # MOVED from base/Crawler/
├── CrawlHub.cs                      # MOVED from base/Crawler/ (SignalR hub)
├── CrawlResult.cs                   # MOVED from base/Crawler/
├── CrawlException.cs                # MOVED from base/Crawler/
├── CrawlerPerformanceTracker.cs     # MOVED from base/Crawler/
├── Lock.cs                          # MOVED from base/Crawler/
├── SiteCrawlerHelpers.cs            # MOVED from base/Crawler/
└── WebSpark.HttpClientUtility.Crawler.csproj # NEW project file

WebSpark.HttpClientUtility.Test/     # BASE PACKAGE TESTS (reorganized)
├── ServiceCollectionExtensionsTests.cs    # Keep base DI tests
├── HttpGetCallServiceTelemetryTests.cs    # Keep telemetry tests
├── Authentication/                         # Keep auth tests
├── RequestResult/                          # Keep core request/response tests
├── MemoryCache/                            # Keep caching tests
├── OpenTelemetry/                          # Keep telemetry tests
├── Concurrent/                             # Keep concurrent tests
├── FireAndForget/                          # Keep fire-and-forget tests
├── Streaming/                              # Keep streaming tests
├── CurlService/                            # Keep CURL tests
├── MockService/                            # Keep mock tests
├── ObjectPool/                             # Keep pooling tests
├── StringConverter/                        # Keep converter tests
└── WebSpark.HttpClientUtility.Test.csproj # Update to reference base package only

WebSpark.HttpClientUtility.Crawler.Test/  # NEW CRAWLER TESTS
├── ServiceCollectionExtensionsTests.cs    # NEW - Test AddHttpClientCrawler()
├── SiteCrawlerTests.cs                    # MOVED from base test
├── SimpleSiteCrawlerTests.cs              # MOVED from base test
├── RobotsTxtParserTests.cs                # MOVED from base test
├── CrawlerOptionsTests.cs                 # MOVED from base test
├── CrawlHubTests.cs                       # MOVED from base test
└── WebSpark.HttpClientUtility.Crawler.Test.csproj  # NEW test project

WebSpark.HttpClientUtility.Web/      # DEMO WEB APP (update with crawler demo)
├── Controllers/
│   └── CrawlerController.cs          # NEW - Demonstrates two-package DI pattern & crawler features
├── Program.cs                        # UPDATE - Add services.AddHttpClientCrawler()
└── WebSpark.HttpClientUtility.Web.csproj # UPDATE - Add both package references
└── WebSpark.HttpClientUtility.Web.csproj # Update package references

.github/workflows/
└── publish-nuget.yml                # UPDATE - Build & publish both packages atomically
```

**Structure Decision**: 

This is a multi-project NuGet library solution requiring:
1. **Two Library Projects**: Base package (existing, trimmed) + new Crawler package (extracted code)
2. **Two Test Projects**: Separate test suites for base and crawler to validate independent functionality
3. **Shared Infrastructure**: Both packages use same signing key, version number, and CI/CD pipeline

The split follows the **modular library pattern** where:
- Base package is self-contained and functional independently
- Crawler package extends base via explicit package reference
- Test projects mirror library structure for clear separation of concerns
- Demo app demonstrates both standalone and combined usage

## Complexity Tracking

> **Filled per Constitution Check violations requiring justification**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| 4-project structure (2 libraries + 2 tests) | Current monolithic package forces 100% of users to download crawler dependencies (HtmlAgilityPack 1.12.4, Markdig 0.43.0, CsvHelper 33.1.0, SignalR) resulting in 40%+ package bloat. Estimated 80%+ of users don't need crawling features. | **Single package with conditional dependencies**: NuGet doesn't support runtime conditional dependencies - all dependencies download regardless. **Feature flags**: Would still require all dependencies to be downloaded, defeating the purpose. **Separate repository**: Would break shared code (RequestResult base classes), complicate versioning coordination, and fragment the HttpClient ecosystem. |
| Lockstep versioning across packages | Ensures compatibility between base and crawler packages. Prevents users from mixing incompatible versions (e.g., base 2.0.0 with crawler 2.1.0). Simplifies support and documentation. | **Independent versioning**: Increases complexity for users who must understand compatibility matrices. Creates support burden. NuGet doesn't clearly signal which versions work together. **Floating version ranges**: Violates constitution's version pinning principle and creates unpredictable behavior. |
| Atomic releases (both packages in single CI/CD run) | Both packages must be tested together to ensure crawler package's base dependency is valid. Prevents partially released versions from reaching NuGet.org. | **Separate release pipelines**: Creates window where base 2.0.0 is published but crawler 2.0.0 fails, leaving users unable to upgrade. Complicates rollback. Increases operational risk. |

**Mitigation Strategies**:
1. **Shared Infrastructure**: Both packages use same HttpClientUtility.snk signing key, same CI/CD pipeline, same CHANGELOG.md
2. **Test Coverage**: Both test projects run in single pipeline before any publishing occurs
3. **Documentation**: Migration guide clearly differentiates core-only users (no changes) from crawler users (add package + DI call)
4. **Version Enforcement**: Crawler .csproj specifies exact version match `[2.0.0]` for base package dependency
5. **Release Validation**: GitHub Actions workflow validates both packages before publishing either one

---

## Phase 0: Outline & Research ✅

**Status**: COMPLETE

### Research Questions Resolved

See **[research.md](./research.md)** for complete analysis with alternatives considered and rationale. All 6 research questions answered:

1. **Package Dependency Split**: Which current dependencies belong in base vs. crawler?
2. **Strong Name Signing**: How to sign both packages with shared key?
3. **GitHub Actions Pipeline**: How to build, test, and publish both packages atomically?
4. **Test Project Organization**: How to split 530+ tests across two test projects?
5. **Breaking Change Communication**: What CHANGELOG format and migration guide structure?
6. **Package Metadata**: How to differentiate packages on NuGet.org (descriptions, tags, icons)?

### Research Findings Summary

*Key decisions extracted from research.md:*

#### Decision 1: Package Dependency Split

**Decision**: Base package retains 9 dependencies, crawler package adds 4 new dependencies.

**Base Package Dependencies** (9 total):
- Microsoft.Extensions.Caching.Abstractions 8.0.0 (caching)
- Microsoft.Extensions.Http 8.0.1 (HttpClientFactory)
- Microsoft.SourceLink.GitHub 8.0.0 (debugging, PrivateAssets)
- Newtonsoft.Json 13.0.4 (JSON serialization option)
- OpenTelemetry 1.13.1 (core telemetry)
- OpenTelemetry.Instrumentation.Http 1.13.0 (HTTP instrumentation)
- OpenTelemetry.Extensions.Hosting 1.13.1 (hosting integration)
- OpenTelemetry.Exporter.Console 1.13.1 (console exporter)
- OpenTelemetry.Exporter.OpenTelemetryProtocol 1.13.1 (OTLP exporter)
- Polly 8.6.4 (resilience)

**Removed from Base** (moved to crawler or eliminated):
- HtmlAgilityPack 1.12.4 → Crawler package
- Markdig 0.43.0 → Crawler package
- CsvHelper 33.1.0 → Crawler package
- OpenTelemetry.Exporter.InMemory 1.13.1 → Test projects only (not in published package)

**Crawler Package Additional Dependencies** (4 total, plus base package reference):
- WebSpark.HttpClientUtility [2.0.0] (exact version match)
- HtmlAgilityPack 1.12.4 (HTML parsing)
- Markdig 0.43.0 (markdown processing)
- CsvHelper 33.1.0 (CSV export)
- Microsoft.AspNetCore.App (framework reference for SignalR)

**Rationale**: Achieves <10 base dependencies (9 vs. current 13), removes 3 crawler-specific packages. Base package focuses on HTTP concerns; crawler adds HTML/markdown/CSV processing.

#### Decision 2: Strong Name Signing

**Decision**: Both packages signed with existing `HttpClientUtility.snk` key using identical .csproj configuration.

**Implementation**:
```xml
<!-- Both .csproj files -->
<SignAssembly>true</SignAssembly>
<AssemblyOriginatorKeyFile>../HttpClientUtility.snk</AssemblyOriginatorKeyFile>
<PublicSign Condition="'$(OS)' == 'Windows_NT'">false</PublicSign>
<PublicSign Condition="'$(OS)' != 'Windows_NT'">true</PublicSign>
<EnableStrongNaming>true</EnableStrongNaming>
<DelaySign>false</DelaySign>
```

**Rationale**: Maintains assembly compatibility, enables GAC scenarios, preserves existing signing infrastructure.

#### Decision 3: GitHub Actions Atomic Publishing

**Decision**: Extend existing `publish-nuget.yml` workflow to build/test/publish both packages in single execution.

**Workflow Changes**:
1. **Build Stage**: `dotnet build` builds entire solution (both packages)
2. **Test Stage**: `dotnet test` runs both test projects (530+ tests)
3. **Pack Stage**: `dotnet pack` creates .nupkg for both packages
4. **Publish Stage**: `dotnet nuget push` uploads both packages to NuGet.org (only if tests pass)
5. **GitHub Release**: Creates single release with both .nupkg files attached

**Rationale**: Atomic behavior ensures both packages tested together, prevents partial releases, maintains constitutional compliance.

#### Decision 4: Test Project Split

**Decision**: Reorganize 530+ tests into two MSTest projects based on feature ownership.

**Split Strategy**:
- **Base Tests (~400-450 tests)**: Authentication, caching, resilience, telemetry, concurrent, streaming, CURL, mock, pooling, converters
- **Crawler Tests (~80-130 tests)**: SiteCrawler, SimpleSiteCrawler, RobotsTxtParser, CrawlerOptions, CrawlHub, performance tracking

**Migration Process**:
1. Create new `WebSpark.HttpClientUtility.Crawler.Test` project
2. Copy crawler-related test files from base test project
3. Update namespaces to match new project
4. Update project references (crawler tests reference both base + crawler packages)
5. Remove crawler tests from base test project
6. Verify both test projects run independently and pass

**Rationale**: Mirrors package structure, enables independent testing, maintains MSTest + Moq patterns.

#### Decision 5: Breaking Change Communication

**Decision**: CHANGELOG.md follows Keep a Changelog format with clear migration guidance.

**CHANGELOG Structure for v2.0.0**:
```markdown
## [2.0.0] - 2025-MM-DD

### BREAKING CHANGES

**Package Split**: WebSpark.HttpClientUtility split into two packages:
- **WebSpark.HttpClientUtility**: Base HTTP utilities (NO breaking changes if you DON'T use crawler features)
- **WebSpark.HttpClientUtility.Crawler**: Web crawling capabilities (NEW package)

**Migration Required If You Use Crawler Features**:
1. Install crawler package: `dotnet add package WebSpark.HttpClientUtility.Crawler`
2. Add to DI: `services.AddHttpClientCrawler();`
3. Runtime behavior identical to v1.5.1

**No Migration Required If You Only Use**:
- Caching, resilience, telemetry, authentication, concurrent requests, streaming, CURL generation

### Added
- New `WebSpark.HttpClientUtility.Crawler` package with all crawler functionality
- Lockstep versioning across both packages

### Changed
- Base package size reduced by 40%+ (removed HtmlAgilityPack, Markdig, CsvHelper, SignalR)
- Base package dependencies reduced from 13 to 9

### Removed
- Crawler features removed from base package (moved to new Crawler package)

### Fixed
- None

### Deprecated
- WebSpark.HttpClientUtility v1.x will receive security/critical fixes only for 6-12 months
```

**Rationale**: Clear breaking change callout, two migration paths (no change vs. add package), links to detailed migration guide.

#### Decision 6: Package Metadata Differentiation

**Decision**: Update NuGet metadata to clearly differentiate packages.

**Base Package Metadata**:
```xml
<PackageId>WebSpark.HttpClientUtility</PackageId>
<Summary>Lightweight HttpClient wrapper with resilience, caching, and telemetry for .NET 8+ applications.</Summary>
<Description>
Core HTTP client utilities for .NET applications, including authentication, caching, 
resilience with Polly, telemetry, concurrent requests, and streaming. Lightweight and 
focused on IHttpClientFactory optimization.
</Description>
<PackageTags>
httpclient;utility;web;api;rest;polly;cache;telemetry;concurrent;dotnet;aspnetcore;
http-resilience;retry;circuit-breaker;caching;curl;fire-and-forget;opentelemetry;
streaming;observability;tracing;net8;net9
</PackageTags>
```

**Crawler Package Metadata**:
```xml
<PackageId>WebSpark.HttpClientUtility.Crawler</PackageId>
<Summary>Web crawling extension for WebSpark.HttpClientUtility with robots.txt support and SignalR progress updates.</Summary>
<Description>
Web crawling capabilities for WebSpark.HttpClientUtility, including robots.txt compliance, 
sitemap generation, HTML parsing, SignalR progress updates, and CSV export. Requires 
WebSpark.HttpClientUtility base package.
</Description>
<PackageTags>
crawler;web-scraping;sitemap;robots-txt;signalr;html-parsing;htmlagilitypack;
web-crawler;site-crawler;csv-export;dotnet;aspnetcore;net8;net9
</PackageTags>
<PackageDependencies>
WebSpark.HttpClientUtility [2.0.0] (exact match)
</PackageDependencies>
```

**Rationale**: Base emphasizes "lightweight" and "focused"; crawler emphasizes "extension" and "requires base". Tags differentiate search results.

---

## Phase 1: Design Artifacts ✅

**Status**: COMPLETE

### Generated Artifacts

1. **data-model.md** ✅
   - Package relationship diagram
   - Dependency graph (9 base + 5 crawler deps)
   - Version constraints (lockstep versioning)
   - Public API surface (base + crawler)
   - Assembly metadata
   - Package size comparison (38% reduction for core users)
   - Multi-targeting configuration
   - NuGet package metadata

2. **contracts/base-package-contract.md** ✅
   - Complete public API surface for base package
   - All preserved v1.x APIs (100% backward compatible)
   - Removed APIs (moved to crawler package)
   - Zero breaking changes for core HTTP users

3. **contracts/crawler-package-contract.md** ✅
   - Complete public API surface for crawler package
   - Breaking change migration steps
   - Dependencies specification
   - Usage examples (basic crawling, SignalR progress)

4. **quickstart.md** ✅
   - Decision tree (crawler user vs. core HTTP user)
   - Path A: Migration required (crawler users)
   - Path B: No migration required (core HTTP users)
   - Side-by-side code comparisons (v1.x vs. v2.0.0)
   - Benefits breakdown (38% package size reduction)
   - Troubleshooting guide
   - Migration timeline recommendation

### AI Agent Context Update

Run the following command to update Copilot agent context with new specifications:

```powershell
.\.specify\scripts\powershell\update-agent-context.ps1 -AgentType copilot
```

This will update `.github/copilot-instructions.md` with:
- Reference to spec 003-split-nuget-packages
- Package split architecture
- v2.0.0 migration guidance
- Lockstep versioning strategy

---

## Phase 2: Task Decomposition

**Status**: PENDING (Requires separate `/speckit.tasks` command)

This phase will decompose the implementation into specific, testable, and trackable tasks. Per the speckit workflow, Phase 2 is NOT part of the planning phase.

**Next Step**: Run `/speckit.tasks` command to generate task breakdown.

---

## Implementation Notes

### Critical Success Factors

1. **Zero Breaking Changes for Core Users**
   - All base package tests must pass without modification
   - Decorator chain order must be preserved
   - Assembly signing key must be unchanged

2. **Atomic Releases**
   - Both packages build and test in single CI/CD run
   - Either both publish or neither publish

3. **Clear Migration Path**
   - Decision tree in quickstart.md guides users
   - Side-by-side code examples for both user types
   - Troubleshooting section addresses common issues

4. **Version Discipline**
   - Directory.Build.props as single source of truth
   - Lockstep versioning enforced at build and runtime
   - Exact version match in crawler .csproj

### Implementation Sequence (Recommended)

**Stage 1: Repository Structure**
1. Create new crawler package project directory
2. Create new crawler test project directory
3. Update solution file (.sln) to include new projects

**Stage 2: Code Migration**
1. Copy crawler code from base to new crawler package
2. Update namespaces in crawler code
3. Copy crawler tests from base to new crawler test project
4. Update test namespaces and project references

**Stage 3: Dependency Configuration**
1. Update base .csproj to remove crawler dependencies
2. Create crawler .csproj with base package reference + crawler deps
3. Update Directory.Build.props for versioning
4. Update signing configuration in both projects

**Stage 4: DI Registration**
1. Create new ServiceCollectionExtensions in crawler package
2. Move crawler DI logic from base to crawler extensions
3. Update demo app Program.cs to call both DI methods (`AddHttpClientUtility()` + `AddHttpClientCrawler()`)
4. Create CrawlerController in demo app with endpoints demonstrating crawler functionality

**Stage 5: Demo Application Enhancement**
1. Create Controllers/CrawlerController.cs in WebSpark.HttpClientUtility.Web
2. Add endpoints: POST /api/crawler/crawl (basic crawl), GET /api/crawler/status (progress), GET /api/crawler/results (crawl results)
3. Implement SignalR hub registration for real-time progress updates
4. Add Swagger documentation for crawler endpoints
5. Update demo app UI to showcase crawler features

**Stage 6: CI/CD**
1. Update publish-nuget.yml workflow
2. Add pack steps for both packages
3. Add publish steps for both packages
4. Update GitHub release steps

**Stage 7: Documentation**
1. Update CHANGELOG.md with v2.0.0 breaking changes
2. Update README.md with migration guide link
3. Create package-specific README files
4. Update documentation website

**Stage 8: Validation**
1. Run all tests (530+ must pass)
2. Verify package sizes (base <350KB, crawler <80KB)
3. Test local NuGet feed installation
4. Smoke test demo app with both packages and CrawlerController endpoints

**Stage 9: Release**
1. Tag repository with v2.0.0
2. CI/CD pipeline builds and publishes both packages
3. Create GitHub release with both .nupkg files
4. Announce release with link to migration guide

---

## Risk Mitigation

### Risk 1: Partial Release

**Risk**: Base package publishes but crawler fails, leaving users unable to upgrade.

**Mitigation**:
- Single GitHub Actions job with sequential steps
- Test failure prevents any publishing
- NuGet API errors fail entire workflow

**Verification**: Test CI/CD with intentional failure in crawler tests.

### Risk 2: Version Mismatch

**Risk**: Users install base 2.0.0 with crawler 2.1.0, causing runtime errors.

**Mitigation**:
- Exact version match in crawler .csproj: `[2.0.0]`
- NuGet enforces version constraint at install time
- Documentation emphasizes lockstep versioning

**Verification**: Attempt to install mismatched versions locally.

### Risk 3: Breaking Base Package

**Risk**: Refactoring accidentally breaks decorator chain or core APIs.

**Mitigation**:
- FR-019 requires decorator pattern preservation
- All base tests must pass unmodified
- Contract testing validates API surface

**Verification**: Run base tests after code extraction.

### Risk 4: Confusion During Migration

**Risk**: Users unsure which package to install or how to migrate.

**Mitigation**:
- Decision tree in quickstart.md (clear yes/no path)
- Segment-specific CHANGELOG messaging
- Package metadata cross-references counterpart

**Verification**: Internal user testing with migration guide.
