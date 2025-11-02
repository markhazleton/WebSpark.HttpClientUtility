<!--
Sync Impact Report:
Version: 0.0.0 → 1.0.0
Modified Principles: Initial constitution creation
Added Sections: All sections (initial creation)
Removed Sections: None
Templates Status:
  ✅ plan-template.md - Aligned with constitution principles
  ✅ spec-template.md - Aligned with constitution principles
  ✅ tasks-template.md - Aligned with constitution principles
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
- **Current Targets**: .NET 8 (LTS) and .NET 9
- **Breaking Changes**: Follow semantic versioning strictly (MAJOR.MINOR.PATCH)
- **Nullable Reference Types**: Enabled globally (`<Nullable>enable</Nullable>`)
- **API Stability**: Backward compatibility is critical; breaking changes require MAJOR version bump

**Rationale**: .NET 8 is LTS (Long Term Support) for enterprise users; .NET 9 provides latest features. Multi-targeting ensures broad adoption while nullable reference types prevent null reference exceptions.

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

**Rationale**: As a published NuGet package, version chaos breaks consumer builds. Automated publishing reduces human error; changelog provides upgrade guidance.

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

1. **Specification**: Create feature spec in `.specify/specs/[###-feature-name]/spec.md`
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

**Never combine version bumps with feature work** - separate concerns for clean history.

### AI Agent Output Organization

AI-generated documentation MUST follow these rules:
- **Session Folders**: Save all AI-generated `.md` files to `/copilot/session-{YYYY-MM-DD}/`
- **No Root Clutter**: Never create `.md` files in repository root (except updating existing files)
- **Date Format**: Use ISO format `YYYY-MM-DD` for session folders (e.g., `/copilot/session-2025-11-02/`)
- **Exceptions**: Only update existing root-level docs (`README.md`, `CHANGELOG.md`) when explicitly requested

**Rationale**: Keeps AI-generated documentation organized and separate from official project documentation. Prevents repository clutter.

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
- [ ] Multi-targeting: Compiles on .NET 8 AND .NET 9
- [ ] Testing: MSTest tests included and passing (252+ tests maintained)
- [ ] Decorator Pattern: New features integrate via decorators, not breaking existing chain
- [ ] One-Line DI: Registration remains simple for consumers
- [ ] Documentation: XML docs on public APIs, README updated if user-facing
- [ ] Versioning: Breaking changes trigger MAJOR version, features trigger MINOR
- [ ] Warnings: All warnings addressed before commit

### Runtime Development Guidance

For detailed development guidance during implementation, refer to `.github/copilot-instructions.md`, which provides:
- Architecture patterns (decorator chain specifics)
- Common pitfalls to avoid
- Integration point details (Polly, SignalR, OpenTelemetry, Memory Cache)
- File structure and key files for changes
- Testing patterns and standards
- Questions to ask before implementing

**Version**: 1.0.0 | **Ratified**: 2025-11-02 | **Last Amended**: 2025-11-02
