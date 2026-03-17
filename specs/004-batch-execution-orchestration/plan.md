# Implementation Plan: Batch Execution Orchestration

**Branch**: `004-batch-execution-orchestration` | **Date**: 2026-03-17 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/004-batch-execution-orchestration/spec.md`

## Summary

Add an opt-in batch execution feature to the base WebSpark.HttpClientUtility package so consumers can define environments, user contexts, and request templates, then execute the full combination through the existing `IHttpRequestResultService` pipeline. The design keeps the existing decorator chain untouched, introduces a separate orchestration service plus template/statistics helpers, and adds an interactive MVC demo page that runs capped batches with live progress via polling-friendly endpoints.

## Technical Context

**Language/Version**: C# with the solution's current multi-targeting configuration; validation must pass on every configured target framework, with .NET 8 and .NET 9 treated as the constitutional minimum  
**Primary Dependencies**: Existing `Microsoft.Extensions.*` DI/logging/HTTP stack, `Polly`, `OpenTelemetry`, `IMemoryCache`, ASP.NET Core MVC in the demo app, and BCL hashing/collections (no new package required)  
**Storage**: In-memory only for orchestration state, statistics, and demo run tracking  
**Testing**: MSTest with Moq in `WebSpark.HttpClientUtility.Test`, including automated controller/demo-service coverage for the MVC demo; manual browser verification remains supplemental  
**Target Platform**: Cross-platform .NET library consumers plus the ASP.NET Core MVC demo app  
**Project Type**: Multi-project NuGet library solution with a demo web application  
**Performance Goals**: Support a single batch call covering up to 1,000 planned requests, keep in-flight work at or below configured concurrency, and calculate P50/P95/P99 within the spec tolerance  
**Constraints**: Preserve the existing decorator order, keep batch execution independent from `Concurrent/`, register only when `EnableBatchExecution` is enabled, leave unresolved placeholders intact, and cap the demo to a maximum of 50 requests per run  
**Scale/Scope**: New batch execution namespace in the base package, focused tests in the existing test project, and one new MVC demo flow with start/progress/result endpoints

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Design Review

- **Library-First Design**: PASS. The feature is added to the base package as interface-driven services and models, and remains independently testable.
- **Test Coverage and Quality**: PASS. The spec explicitly requires MSTest coverage for substitution, combinatorial expansion, throttling, cancellation, hashing, streaming, and automated demo controller/service behavior.
- **Multi-Targeting and Compatibility**: PASS. No framework changes or breaking API removals are required; the new capability is opt-in.
- **One-Line Developer Experience**: PASS. Registration stays within `services.AddHttpClientUtility(options => ...)` by adding `EnableBatchExecution`.
- **Observability and Diagnostics**: PASS. Individual requests continue flowing through correlation IDs, logging, resilience, and telemetry via `IHttpRequestResultService`.
- **Versioning and Release Discipline**: PASS. This is feature work in the existing package and does not require any publishing-path changes.
- **Decorator Pattern Architecture**: PASS. Batch orchestration is a separate service that uses the existing decorator chain for each request instead of altering decorator order.

### Post-Design Review

- **Library-First Design**: PASS. The data model separates orchestration input, per-request output, statistics, progress, and demo-run state.
- **Test Coverage and Quality**: PASS. The plan keeps all new tests in MSTest, adds automated demo coverage, and treats manual UI validation as supplemental rather than primary.
- **Multi-Targeting and Compatibility**: PASS. The contract uses BCL types and existing library abstractions only, and the release gate requires build and test success on every configured target framework.
- **One-Line Developer Experience**: PASS. Consumers still enable the feature in a single options delegate.
- **Observability and Diagnostics**: PASS. Parent run IDs plus per-request correlation IDs preserve traceability without changing existing telemetry decorators.
- **Versioning and Release Discipline**: PASS. No constitutional violations identified.
- **Decorator Pattern Architecture**: PASS. The orchestrator composes with `IHttpRequestResultService`; it does not replace or wrap the base decorators.

## Project Structure

### Documentation (this feature)

```text
specs/004-batch-execution-orchestration/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── batch-execution-library-contract.md
│   └── batch-execution-demo-api-contract.md
└── tasks.md
```

### Source Code (repository root)

```text
WebSpark.HttpClientUtility/
├── ServiceCollectionExtensions.cs                # Add EnableBatchExecution and DI wiring
├── RequestResult/                               # Existing per-request execution pipeline (reused)
└── BatchExecution/                              # New namespace for orchestration feature
    ├── IBatchExecutionService.cs
    ├── ITemplateSubstitutionService.cs
    ├── BatchExecutionService.cs
    ├── IBatchExecutionResultSink.cs
    ├── TemplateSubstitutionService.cs
    ├── BatchExecutionConfiguration.cs
    ├── BatchEnvironment.cs
    ├── BatchUserContext.cs
    ├── BatchRequestDefinition.cs
    ├── BatchExecutionItemResult.cs
    ├── BatchExecutionResult.cs
    ├── BatchExecutionStatistics.cs
    ├── BatchProgress.cs
    └── BatchStatisticsCollector.cs

WebSpark.HttpClientUtility.Test/
├── BatchExecution/
│   ├── TemplateSubstitutionServiceTests.cs
│   ├── BatchExecutionExpansionTests.cs
│   ├── BatchExecutionExecutionTests.cs
│   ├── BatchStatisticsCollectorTests.cs
│   ├── BatchProgressTests.cs
│   ├── BatchExecutionHashingTests.cs
│   ├── BatchExecutionStreamingTests.cs
│   ├── BatchExecutionPipelineIntegrationTests.cs
│   ├── BatchExecutionDemoControllerTests.cs
│   ├── BatchExecutionDemoServiceTests.cs
│   └── ServiceCollectionExtensionsBatchExecutionTests.cs
└── existing tests remain unchanged

WebSpark.HttpClientUtility.Web/
├── Controllers/
│   └── BatchExecutionController.cs
├── Models/
│   └── BatchExecutionViewModels.cs
├── Services/
│   └── BatchExecutionDemoService.cs
├── Views/
│   └── BatchExecution/
│       └── Index.cshtml
├── Views/Shared/_Layout.cshtml                  # Add navigation entry
└── Program.cs                                   # Ensure batch execution is enabled for demo usage
```

**Structure Decision**: The feature belongs in the base library package because RESTRunner needs to delegate batch orchestration without taking an additional package dependency. A dedicated `BatchExecution/` namespace is preferred over extending `Concurrent/` because the existing concurrent implementation is specialized around `SiteStatus` and simple parallel processing, while the new feature needs templating, cartesian expansion, incremental sinks, progress snapshots, and aggregate statistics.

## Validation Strategy

- Unit coverage validates template rendering, expansion rules, cancellation, progress, explicit body-capability handling for custom methods, hashing, and full output-result metadata.
- Integration coverage validates that batch execution continues to flow through the existing request pipeline so resilience, caching, telemetry, authentication, and correlation behavior remain compatible when batch execution is enabled.
- Demo coverage includes automated MSTest validation for the BatchExecution controller and demo run-tracking service, followed by manual browser verification of the capped interactive flow.
- Release validation requires `dotnet build` and `dotnet test` for the full solution in Release mode on every configured target framework, with no new warnings introduced by the feature work.

## Complexity Tracking

No constitutional violations require separate justification. The design adds a new orchestrator service rather than reworking the existing concurrent abstractions, which keeps complexity localized and preserves backward compatibility.

### Design Decisions

- **SHA-256 for response hashing**: The spec assumption states a cryptographically secure algorithm is not required — only fast, collision-resistant hashing. SHA-256 was chosen because it is available in all target frameworks via `System.Security.Cryptography.SHA256`, is deterministic, and provides excellent collision resistance. While it is technically a cryptographic hash, the performance overhead is negligible for the expected response body sizes, and using a well-known algorithm avoids introducing additional dependencies. Future maintainers may substitute a faster non-cryptographic hash (e.g., xxHash) if profiling indicates SHA-256 is a bottleneck.
- **.NET target framework scope**: The constitutional minimum is .NET 8 and .NET 9. The solution also multi-targets .NET 10 via `Directory.Build.props`. Task T039 validates all configured targets, which inherently covers .NET 10 without requiring explicit plan-level callout.
