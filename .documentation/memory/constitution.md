<!--
Sync Impact Report:
Version: 1.0.2 → 1.1.0
Modified Principles:
  - Principle III: Added .NET 10 LTS target; added demo-app single-TFM rule
  - Compliance Checklist: Updated to require .NET 8, .NET 9, AND .NET 10
Added Sections:
  - Repository Outputs (three primary deliverables)
  - Technical Standards: Frontend Build Tooling (Vite manifest hidden-dir rule)
  - Development Workflow: Test Project Dependency Conditioning
Removed Sections: None
Templates Status:
  ✅ plan-template.md - No TFM-specific references; aligned
  ✅ spec-template.md - No TFM-specific references; aligned
  ✅ tasks-template.md - No TFM-specific references; aligned
Follow-up TODOs: None
-->

# WebSpark.HttpClientUtility Constitution

## Core Principles

### I. Library-First Design

WebSpark.HttpClientUtility is a production-ready NuGet library. Every feature MUST be:

- **Self-contained**: Independently testable and deployable
- **Interface-based**: All services implement interfaces for testability and extensibility
- **Decorator-composable**: Features layer through the decorator pattern (base → cache → resilience → telemetry)
- **Opt-in by default**: Core functionality included; optional features enabled via configuration

**Rationale**: Library consumers need minimal setup overhead while maintaining flexibility. The decorator pattern ensures features compose cleanly without interdependencies.

### II. Test Coverage and Quality (NON-NEGOTIABLE)

Testing is mandatory before any code ships:

- **Framework**: MSTest (`[TestClass]`, `[TestMethod]`, `[TestInitialize]`)
- **Mocking**: Moq library with `MockBehavior.Loose` preferred for maintainability
- **Pattern**: Arrange-Act-Assert with clear sections
- **Naming**: `MethodName_Scenario_ExpectedBehavior`
- **Coverage Target**: Maintain 252+ passing tests; new features require corresponding test coverage
- **Red-Green-Refactor**: Tests written → Tests fail → Implement → Tests pass

**Rationale**: As a published library with 252+ tests and production users, regression prevention and quality assurance are non-negotiable. Consistent patterns ensure maintainability.

### III. Multi-Targeting and Compatibility

Code MUST compile and run on ALL target frameworks:

- **Library Targets**: .NET 8 (LTS, until Nov 2026), .NET 9, and .NET 10 (LTS, until May 2028)
- **Demo App Targeting**: `WebSpark.HttpClientUtility.Web` MUST target only the **latest stable .NET version** (currently `net10.0`). It is a demo app, not a NuGet package, and MUST NOT multi-target. Clear `<TargetFrameworks>` from `Directory.Build.props` by setting `<TargetFrameworks></TargetFrameworks>` alongside the explicit `<TargetFramework>net10.0</TargetFramework>`.
- **Test Project Conditioning**: When a test project references a web/demo project that doesn't multi-target, the `<ProjectReference>` and web-dependent test files MUST be conditioned with `Condition="'$(TargetFramework)' == 'net10.0'"` to avoid `NU1201` errors on older TFMs.
- **Breaking Changes**: Follow semantic versioning strictly (MAJOR.MINOR.PATCH)
- **Nullable Reference Types**: Enabled globally (`<Nullable>enable</Nullable>`)
- **API Stability**: Backward compatibility is critical; breaking changes require MAJOR version bump

**Rationale**: .NET 8 and .NET 10 are LTS releases serving enterprise users; .NET 9 bridges the gap. Multi-targeting the library ensures broad adoption. The demo app pins to latest to showcase current capabilities without the overhead of maintaining older TFMs for a non-packaged project.

### IV. One-Line Developer Experience

Service registration MUST be achievable in a single, intuitive line:

- **Primary API**: `services.AddHttpClientUtility(options => {...})`
- **Quick Presets**: `AddHttpClientUtilityWithCaching()`, `AddHttpClientUtilityWithAllFeatures()`
- **Configuration Model**: Fluent options pattern with sensible defaults
- **No Boilerplate**: Library handles HttpClient factory, correlation IDs, structured logging automatically

**Rationale**: Developers should spend time building features, not configuring infrastructure. Complexity is hidden behind simple APIs while advanced users can customize via options.

### V. Observability and Diagnostics

Every HTTP operation MUST be observable and traceable:

- **Correlation IDs**: Auto-generated for all requests, propagated through distributed calls
- **Structured Logging**: Rich context in all log messages (request/response details, timing, errors)
- **Telemetry**: Request duration tracking with OpenTelemetry integration support
- **Error Context**: Exceptions include request details for debugging production issues

**Rationale**: Production debugging requires visibility. Correlation IDs enable tracing requests across services; structured logging makes log aggregation effective; telemetry enables performance monitoring.

### VI. Versioning and Release Discipline

Version management follows strict processes:

- **Semantic Versioning**: MAJOR.MINOR.PATCH with clear breaking change communication
- **Version Bump Separation**: Version increments are separate commits from feature work
- **Changelog Discipline**: Every release documented in `CHANGELOG.md` following Keep a Changelog format
- **Git Tagging**: Tags trigger automated NuGet publishing via GitHub Actions
- **Package Validation**: Baseline validation enabled to detect breaking changes
- **CI/CD Enforcement**: **ALL NuGet package publications MUST go through GitHub Actions CI/CD pipeline. Manual publishing is strictly prohibited.**

**Rationale**: As a published NuGet package, version chaos breaks consumer builds. Automated publishing reduces human error; changelog provides upgrade guidance. The GitHub Actions workflow is the single source of truth for all NuGet package releases, ensuring consistent builds, proper testing, symbol packages, and audit trails.

### VII. Decorator Pattern Architecture

Feature composition MUST follow the decorator chain:

1. **Base**: `HttpRequestResultService` (core HTTP functionality)
2. **Cache**: `HttpRequestResultServiceCache` (optional caching layer)
3. **Resilience**: `HttpRequestResultServicePolly` (optional retry/circuit breaker)
4. **Telemetry**: `HttpRequestResultServiceTelemetry` (outermost - tracks total duration)

**Order is critical**:

- Telemetry wraps everything to capture total request time
- Cache happens before resilience to avoid caching failed retries
- Each decorator independently testable

**Rationale**: Decorator pattern enables feature composition without tight coupling. Order matters for correctness (e.g., don't cache failures). New features integrate by adding decorators.

## Technical Standards

### Code Analysis and Warnings

All code MUST meet high-quality standards:

- **Warning Level**: 5 (highest sensitivity)
- **Analyzers**: `EnableNETAnalyzers=true`, `AnalysisLevel=latest`
- **Warning Policy**: `TreatWarningsAsErrors=false` BUT warnings MUST be addressed before commit
- **Code Style**: `EnforceCodeStyleInBuild=true`
- **Test Exception**: CA2007 (ConfigureAwait) suppressed in test projects only

**Process**: After writing new code, run `dotnet build` and address all warnings. Warnings indicate real issues or non-idiomatic patterns.

### Frontend Build Tooling

The demo web app uses Vite for front-end asset compilation. The following rules apply:

- **Manifest Path**: The Vite manifest MUST NOT be written to a hidden directory (any directory starting with `.`). Use `manifest: 'vite-manifest.json'` in `vite.config.js` to output to `wwwroot/dist/vite-manifest.json`.
- **Reason**: ASP.NET Core's static web assets pipeline excludes directories starting with `.` from publish output. Using `manifest: true` (default) outputs to `.vite/manifest.json` which is silently excluded, causing runtime failures.
- **Publish Targets**: The `NpmBuild` MSBuild target MUST declare `BeforeTargets="Build;BeforePublish"` so that Vite assets are compiled in both standard build and `--no-build` publish scenarios.
- **ViteAssetTagHelper**: Must read from `Path.Combine(env.WebRootPath, "dist", "vite-manifest.json")`.

### XML Documentation

All public APIs MUST have XML documentation:

```csharp
/// <summary>
/// Brief description of what the method does.
/// </summary>
/// <param name="request">Description of parameter.</param>
/// <param name="ct">Cancellation token.</param>
/// <returns>Description of return value.</returns>
/// <exception cref="ArgumentNullException">When thrown.</exception>
```

**Rationale**: Generates IntelliSense documentation in consuming projects. Required for professional library experience.

### Async/Await Discipline

Asynchronous code MUST follow best practices:

- **Never block**: No `.Result`, `.Wait()`, or `Task.Run()` to wrap sync code
- **ConfigureAwait(false)**: Use in library code (not required in tests)
- **CancellationToken**: Accept `CancellationToken ct = default` on all async methods
- **Async all the way**: Public async methods call async dependencies

**Rationale**: Blocking async code causes deadlocks. Libraries should not capture SynchronizationContext unnecessarily.

### Dependency Management

Dependencies MUST be carefully managed:

- **Minimize Dependencies**: Evaluate necessity before adding new packages
- **Version Pinning**: Use specific versions, not ranges (avoid unexpected breaks)
- **Strong Naming**: Assembly signed with `HttpClientUtility.snk` for GAC compatibility
- **Framework References**: Use `Microsoft.AspNetCore.App` for SignalR/ObjectPool without explicit versions

**Rationale**: Each dependency increases supply chain risk and package size. Strong naming enables enterprise scenarios.

## Development Workflow

### Feature Implementation Process

1. **Specification**: Create feature spec in `specs/[###-feature-name]/spec.md`
2. **Planning**: Generate implementation plan via `/speckit.plan` command
3. **Design**: Document data models, contracts, and architecture
4. **Test Creation**: Write failing tests first (Red)
5. **Implementation**: Implement feature (Green)
6. **Refactoring**: Improve code quality (Refactor)
7. **Documentation**: Update README.md, XML docs, and examples
8. **Review**: All tests pass, warnings addressed, documentation complete

### Pull Request Requirements

Every PR MUST include:

- **Tests**: Corresponding test coverage for new functionality
- **Documentation**: XML docs for public APIs, README updates if user-facing
- **Changelog**: Entry in CHANGELOG.md if releasing
- **Constitution Check**: Verify decorator pattern maintained, no breaking changes in MINOR/PATCH
- **Build Validation**: `dotnet build` and `dotnet test` both succeed on ALL target frameworks
- **Warning Cleanup**: No new warnings introduced

### Release Process

Version bumps follow this workflow:

1. **Decide Version**: MAJOR (breaking), MINOR (feature), or PATCH (bugfix)
2. **Update .csproj**: Modify `<Version>1.X.Y</Version>` in `WebSpark.HttpClientUtility.csproj`
3. **Update Changelog**: Add release section to `CHANGELOG.md` with date and changes
4. **Commit**: `git commit -m "chore: bump version to 1.X.Y"`
5. **Tag**: `git tag v1.X.Y && git push origin v1.X.Y`
6. **Automation**: GitHub Actions builds, tests, packs, publishes to NuGet.org

**NEVER**:

- ❌ Manually upload packages to NuGet.org
- ❌ Use `dotnet nuget push` locally
- ❌ Bypass the CI/CD pipeline
- ❌ Combine version bumps with feature work

**Never combine version bumps with feature work** - separate concerns for clean history. The GitHub Actions workflow is the ONLY way to publish NuGet packages.

### AI Agent Output Organization

AI-generated documentation MUST follow these rules:

- **Session Folders**: Save all AI-generated `.md` files to `/.documentation/copilot/session-{YYYY-MM-DD}/`
- **No Root Clutter**: Never create `.md` files in repository root (except updating existing files)
- **Date Format**: Use ISO format `YYYY-MM-DD` for session folders (e.g., `/.documentation/copilot/session-2025-11-02/`)
- **Exceptions**: Only update existing root-level docs (`README.md`, `CHANGELOG.md`) when explicitly requested
- **Public Static Site Boundary (MANDATORY)**: `/docs` is the published static website and is treated as website content/source, not archival documentation. `/docs` MUST NOT be targeted by harvest/archive/memory cleanup workflows.
- **Archive Exclusion Rule (MANDATORY)**: Any automation that archives documentation MUST explicitly exclude `/docs/**` unless a direct docs-site archival/migration is explicitly requested by a human.

**Rationale**: Keeps AI-generated documentation organized and separate from official project documentation. Prevents repository clutter.

## Repository Outputs

This repository produces three primary, independently maintained deliverables. Each has distinct audiences, hosting, and update cadence.

### Output 1 — NuGet Packages (Library)

| Package | NuGet URL |
|---------|-----------|
| `WebSpark.HttpClientUtility` | <https://www.nuget.org/packages/WebSpark.HttpClientUtility> |
| `WebSpark.HttpClientUtility.Crawler` | <https://www.nuget.org/packages/WebSpark.HttpClientUtility.Crawler> |
| `WebSpark.HttpClientUtility.Testing` | <https://www.nuget.org/packages/WebSpark.HttpClientUtility.Testing> |

- Published via GitHub Actions CI/CD on `v*.*.*` tag push. Manual publishing is **strictly prohibited**.
- Multi-targets: `net8.0;net9.0;net10.0` (library and crawler packages). Lockstep versioning across all packages.
- Source: `WebSpark.HttpClientUtility/`, `WebSpark.HttpClientUtility.Crawler/`, `WebSpark.HttpClientUtility.Testing/`

### Output 2 — Static GitHub Pages Info Site

- **URL**: <https://httpclientutility.makeboldspark.com/>
- **Source**: `/docs/` directory (served by GitHub Pages from `main` branch)
- **Purpose**: NuGet package documentation — getting started, API reference, features, examples
- **Technology**: Static HTML/CSS (no build step; files are deployed as-is)
- **Governance**: `/docs/**` MUST NOT be targeted by any archive/harvest/cleanup automation
- **Cross-link**: MUST include a visible link to the HttpClientDecorator demo site

### Output 3 — HttpClientDecorator Demo Site

- **URL**: <https://httpclientdecorator.makeboldspark.com/>
- **Source**: `WebSpark.HttpClientUtility.Web/` (ASP.NET Core MVC)
- **Purpose**: Live interactive demonstration of the decorator pattern, caching, resilience, crawling, batch execution
- **Technology**: ASP.NET Core MVC + Vite (front-end assets)
- **Target Framework**: `net10.0` only (single TFM — not multi-targeted; always the latest stable .NET)
- **Cross-link**: MUST include a visible link to the GitHub Pages info site
- **Publish**: Folder publish to hosting provider. Run `dotnet publish` (without `--no-build` to ensure Vite runs)

### Cross-Site Linking Rule

Both sites MUST maintain mutual navigation links so users can easily move between:

- Package documentation ↔ Live demo
- Both sites ↔ GitHub repository and NuGet package page

## Governance

### Constitution Authority

This constitution supersedes all other development practices and guidelines. When conflicts arise between this document and other documentation, this constitution takes precedence.

### Amendment Process

Constitution amendments require:

1. **Proposal**: Document proposed changes with rationale
2. **Impact Analysis**: Identify affected templates, code patterns, and workflows
3. **Version Bump**: Increment constitution version per semantic versioning
4. **Sync Propagation**: Update dependent templates (plan-template.md, spec-template.md, tasks-template.md)
5. **Commit**: Record changes in Sync Impact Report (HTML comment at top of file)

### Complexity Justification

Any violation of constitutional principles (e.g., breaking decorator pattern, adding unjustified dependencies) MUST be explicitly justified in the implementation plan with:

- **Why Needed**: Specific technical requirement
- **Alternatives Rejected**: Simpler options considered and why they're insufficient
- **Mitigation**: How to minimize impact and maintain architectural integrity

### Compliance Review

All implementations MUST verify:

- [ ] Multi-targeting: Library compiles on .NET 8, .NET 9, AND .NET 10
- [ ] Demo app: Targets only `net10.0` (single TFM, not multi-targeted)
- [ ] Testing: MSTest tests included and passing; new features require corresponding test coverage
- [ ] Decorator Pattern: New features integrate via decorators, not breaking existing chain
- [ ] One-Line DI: Registration remains simple for consumers
- [ ] Documentation: XML docs on public APIs, README updated if user-facing
- [ ] Versioning: Breaking changes trigger MAJOR version, features trigger MINOR
- [ ] Warnings: All warnings addressed before commit
- [ ] Frontend: Vite manifest at `wwwroot/dist/vite-manifest.json` (not hidden directory)

### Runtime Development Guidance

For detailed development guidance during implementation, refer to `.github/copilot-instructions.md`, which provides:

- Architecture patterns (decorator chain specifics)
- Common pitfalls to avoid
- Integration point details (Polly, SignalR, OpenTelemetry, Memory Cache)
- File structure and key files for changes
- Testing patterns and standards
- Questions to ask before implementing

**Version**: 1.1.0 | **Ratified**: 2025-11-02 | **Last Amended**: 2026-05-16
