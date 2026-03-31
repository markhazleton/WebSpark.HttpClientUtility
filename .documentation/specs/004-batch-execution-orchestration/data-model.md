# Data Model: Batch Execution Orchestration

**Feature**: 004-batch-execution-orchestration  
**Date**: 2026-03-17

## Overview

This document defines the orchestration entities, validation rules, and relationships for batch execution in the base WebSpark.HttpClientUtility package, plus the lightweight demo-run state used by the MVC sample page.

---

## Entity Relationship Summary

```text
BatchExecutionConfiguration
├── Environments: 1..*
│   └── BatchEnvironment
├── Users: 0..*
│   └── BatchUserContext
├── Requests: 0..*
│   └── BatchRequestDefinition
└── Produces: 0..*
    ├── BatchExecutionItemResult
    ├── BatchExecutionStatistics
    └── BatchProgress

BatchExecutionDemoRun
├── references BatchExecutionConfiguration
├── exposes BatchProgress snapshots
└── completes with BatchExecutionResult
```

---

## Core Entities

### BatchExecutionConfiguration

Represents one orchestration request submitted by a library consumer.

**Fields**:
- `RunId: string`
- `Environments: IReadOnlyList<BatchEnvironment>`
- `Users: IReadOnlyList<BatchUserContext>`
- `Requests: IReadOnlyList<BatchRequestDefinition>`
- `Iterations: int`
- `MaxConcurrency: int`

**Validation rules**:
- `RunId` may be auto-generated when omitted.
- `Iterations` must be `>= 0`.
- `MaxConcurrency` must be `>= 1`.
- Empty `Users` or `Requests` are valid and result in zero planned work.
- Each environment must have a valid absolute base URL.

**Internal state** (set by the orchestrator, not supplied by consumers):
- `StartedAtUtc: DateTimeOffset?` — populated when execution begins.
- `RequestedBy: string?` — optional operational metadata.

**Relationships**:
- Owns all environments, users, and request definitions for a single batch.
- Produces exactly one `BatchExecutionResult`.

---

### BatchEnvironment

Represents one target API environment.

**Fields**:
- `Name: string`
- `BaseUrl: string`
- `DefaultHeaders: IReadOnlyDictionary<string, string>`

**Validation rules**:
- `Name` is required and should be unique within a batch.
- `BaseUrl` must be an absolute HTTP or HTTPS URL.

**Relationships**:
- Combined with every user, request definition, and iteration during expansion.

---

### BatchUserContext

Represents a named execution persona plus substitution values.

**Fields**:
- `UserId: string`
- `Properties: IReadOnlyDictionary<string, string>`

**Validation rules**:
- `UserId` is required when user contexts are provided.
- `Properties` may be empty.
- Property keys are matched case-sensitively during template substitution.

**Relationships**:
- Supplies values to template rendering.
- Contributes to breakdowns by user.

---

### BatchRequestDefinition

Represents one parameterized request template.

**Fields**:
- `Name: string`
- `Method: string`
- `PathTemplate: string`
- `BodyTemplate: string?`
- `IsBodyCapable: bool`
- `Headers: IReadOnlyDictionary<string, string>`
- `ContentType: string?`

**Validation rules**:
- `Name`, `Method`, and `PathTemplate` are required.
- `Method` must accept standard methods and arbitrary custom values.
- `IsBodyCapable` defaults to `true` for standard body methods (POST, PUT, PATCH) and `false` for all others (GET, DELETE, HEAD, OPTIONS, custom methods). Consumers may override the default explicitly for custom methods.
- `BodyTemplate` is optional and only attached to the outbound request when `IsBodyCapable` is `true`.

**Relationships**:
- Combined with every environment, user, and iteration.
- Produces one or more `BatchExecutionItemResult` records.

---

### BatchExecutionItemResult

Represents the outcome of one expanded execution work item.

**Fields**:
- `RunId: string`
- `EnvironmentName: string`
- `UserId: string?`
- `RequestName: string`
- `Iteration: int`
- `HttpMethod: string`
- `RequestPath: string`
- `CorrelationId: string`
- `TimestampUtc: DateTimeOffset`
- `DurationMilliseconds: long`
- `IsSuccess: bool`
- `StatusCode: int?`
- `ResponseBodyHash: string?`
- `ErrorMessages: IReadOnlyList<string>`

**Validation rules**:
- `DurationMilliseconds` must be `>= 0`.
- `StatusCode` may be null when no response was received.
- `ResponseBodyHash` may be null for failures with no response body.

**Relationships**:
- Feeds `BatchExecutionStatistics`.
- Streamed to `IBatchExecutionResultSink` as work completes.

---

### BatchExecutionStatistics

Represents aggregated metrics for a batch run.

**Fields**:
- `TotalCount: int`
- `SuccessCount: int`
- `FailureCount: int`
- `P50Milliseconds: double`
- `P95Milliseconds: double`
- `P99Milliseconds: double`
- `ByMethod: IReadOnlyDictionary<string, int>`
- `ByEnvironment: IReadOnlyDictionary<string, int>`
- `ByUser: IReadOnlyDictionary<string, int>`
- `ByStatusCode: IReadOnlyDictionary<int, int>`

**Validation rules**:
- Counts must always sum consistently.
- Percentiles default to `0` for empty result sets.

**Relationships**:
- Included in both progress snapshots and final batch results.

---

### BatchExecutionResult

Represents the final outcome of a complete batch run.

**Fields**:
- `RunId: string`
- `TotalPlannedCount: int`
- `CompletedCount: int`
- `WasCancelled: bool`
- `Statistics: BatchExecutionStatistics`
- `Results: IReadOnlyList<BatchExecutionItemResult>`

**Validation rules**:
- `CompletedCount` must be between `0` and `TotalPlannedCount`.
- `WasCancelled` is `true` when cancellation stopped dispatch before all work items completed.
- `Statistics` reflects only completed work items.

**Relationships**:
- Returned by `IBatchExecutionService.ExecuteAsync` upon completion.
- Referenced by `BatchExecutionDemoRun` for demo polling responses.

---

### BatchProgress

Represents an incremental progress update emitted after each completed request.

**Fields**:
- `RunId: string`
- `CompletedCount: int`
- `TotalPlannedCount: int`
- `StatisticsSnapshot: BatchExecutionStatistics`
- `LastUpdatedUtc: DateTimeOffset`

**Validation rules**:
- `CompletedCount` must be between `0` and `TotalPlannedCount`.
- Snapshot values must reflect all completed work at the time of emission.

**Relationships**:
- Delivered through `IProgress<BatchProgress>`.
- Stored by the demo run tracker for polling.

## Demo-Only Entity

### BatchExecutionDemoRun

Represents one capped run managed by the MVC demo controller.

**Fields**:
- `RunId: string`
- `Configuration: BatchExecutionConfiguration`
- `State: DemoRunState`
- `LatestProgress: BatchProgress?`
- `FinalResult: BatchExecutionResult?`
- `CreatedAtUtc: DateTimeOffset`
- `ExpiresAtUtc: DateTimeOffset`

**States**:
- `Queued`
- `Running`
- `Completed`
- `Cancelled`
- `Failed`

**State transitions**:

```text
Queued -> Running -> Completed
Queued -> Running -> Cancelled
Queued -> Running -> Failed
```

**Validation rules**:
- Demo runs must enforce the UI cap of about 50 planned requests.
- Expired runs may be evicted from the in-memory store.

---

## Derived Counts

The planned request count is defined as:

```text
plannedCount = environmentCount * userCount * requestCount * iterations
```

Special cases:
- If `iterations == 0`, planned count is `0`.
- If `users` is empty, planned count is `0` under the current spec assumptions.
- If `requests` is empty, planned count is `0`.

---

## Notes

- Batch execution intentionally does not reuse the legacy `Concurrent/` model types because those types are specialized around `SiteStatus` and do not model templating, hashing, or aggregate statistics.
- Correlation IDs remain per-request; `RunId` provides the batch-level grouping identifier.