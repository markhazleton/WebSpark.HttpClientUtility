# Research: Batch Execution Orchestration

**Feature**: 004-batch-execution-orchestration  
**Date**: 2026-03-17  
**Status**: Complete

## Overview

This document captures the design decisions for adding batch execution orchestration to the base WebSpark.HttpClientUtility package while preserving the existing request decorator chain and the lightweight developer experience.

---

## Decision 1: Batch execution is a separate orchestrator service

**Decision**: Implement `IBatchExecutionService` and `BatchExecutionService` as a separate service that depends on `IHttpRequestResultService` for each individual HTTP call.

**Rationale**:
- The existing decorators in `RequestResult/` are per-request behaviors. Batch orchestration is coordination logic across many requests.
- Keeping orchestration separate preserves the existing order: base -> cache -> resilience -> telemetry.
- The orchestrator can automatically inherit retries, circuit breaker behavior, caching, correlation ID propagation, and telemetry by sending each item through `IHttpRequestResultService`.
- This avoids coupling the new feature to the older `Concurrent/` types, which are centered on `SiteStatus` and simple parallel execution.

**Alternatives considered**:
- Add a new decorator around `IHttpRequestResultService`: Rejected because it would mix per-request and batch-level responsibilities and make DI composition harder to reason about.
- Extend `Concurrent/` directly: Rejected because the current abstractions are too specialized and would require larger refactoring than a new focused namespace.

---

## Decision 2: Template substitution uses a single-pass string renderer

**Decision**: Use a single-pass template renderer that supports:
- `{key}` lookup against the user context property dictionary
- `{{encoded_user_name}}` special token lookup against the user identifier
- preservation of unresolved placeholders exactly as written

**Rationale**:
- A single-pass renderer matches the spec's assumption that substitution is not recursive.
- Leaving unresolved placeholders unchanged satisfies FR-003 and makes debugging missing data explicit.
- Supporting the special token directly keeps compatibility with the RESTRunner usage pattern called out in the spec.
- This approach works for both path templates and body templates with no extra dependency.

**Alternatives considered**:
- Recursive substitution: Rejected because it creates ambiguity when substituted values themselves contain brace-delimited text.
- Templating engine package: Rejected because the rules are intentionally narrow and the BCL solution is sufficient.

---

## Decision 3: Raw execution results remain request-oriented, batch results add orchestration metadata

**Decision**: The batch feature will produce item-level results containing orchestration metadata plus the per-request outcome needed for hashing, comparison, sinks, and aggregate statistics.

**Rationale**:
- Consumers need environment, user, iteration, request definition, duration, status, and response hash in one record.
- The existing `HttpRequestResult<T>` type is still the right execution primitive for individual sends, but it is not expressive enough by itself for a full batch run.
- A dedicated batch result model keeps the public API stable and avoids overloading `RequestContext` with orchestration concerns.

**Alternatives considered**:
- Return only `List<HttpRequestResult<string>>`: Rejected because it loses batch-specific fields such as environment name, user ID, iteration, and statistics snapshot.
- Store orchestration data only in `RequestContext`: Rejected because it makes the API implicit and harder to validate.

---

## Decision 4: Statistics are collected with concurrent counters plus snapshot-based percentile calculation

**Decision**: Use thread-safe counters and breakdown dictionaries during execution, capture response times in an in-memory concurrent collection, and compute percentiles from a copied sorted snapshot when building progress updates or the final result.

**Rationale**:
- Incremental counts by outcome, method, environment, user, and status code are naturally concurrent and low cost.
- Percentiles require ordered data, so computing them from a snapshot is simpler and less error-prone than trying to maintain a continuously ordered structure.
- The spec only requires accurate P50/P95/P99 within tolerance; snapshot-based calculation is sufficient for the planned scale.
- The same snapshot method can be reused for progress reporting and final completion.

**Alternatives considered**:
- Lock around a shared `List<long>` for all statistics: Rejected because it increases contention during high-concurrency runs.
- Approximate percentile algorithms: Rejected because the required scale does not justify the added complexity.

---

## Decision 5: Response hashing uses SHA-256 from the BCL

**Decision**: Use SHA-256 for deterministic response hashing.

**Rationale**:
- It is built into the platform, deterministic, cross-framework, and easy to document.
- It avoids adding a dependency for a feature that only needs stable comparison, not custom hashing performance tricks.
- The implementation is straightforward for string or byte content and can be validated easily in unit tests.

**Alternatives considered**:
- MD5: Rejected because there is no need to prefer a legacy algorithm when SHA-256 is equally easy to use.
- Non-cryptographic algorithms such as xxHash: Rejected because they would add dependencies or custom code without a clear benefit at the planned scale.

---

## Decision 6: Demo progress uses REST-friendly run tracking rather than tying the feature to SignalR

**Decision**: The MVC demo will expose a start-run endpoint and a status endpoint so the page can poll for live progress while the library reports progress through `IProgress<BatchProgress>`.

**Rationale**:
- The feature spec requires live updates in the demo but does not require a real-time library dependency.
- Polling keeps the demo implementation aligned with standard REST contracts and avoids making the batch execution feature depend on SignalR.
- The web app can maintain a lightweight in-memory run store for capped demo runs.
- This keeps the library pure while still demonstrating incremental progress.

**Alternatives considered**:
- SignalR push updates: Rejected for the first implementation because it introduces a more complex demo integration path than the feature requires.
- One-shot POST returning only final results: Rejected because it does not satisfy the live-update acceptance criteria for the demo.

---

## Decision 7: Feature registration follows the existing options pattern

**Decision**: Add `EnableBatchExecution` to `HttpClientUtilityOptions` and register batch services only when the flag is enabled.

**Rationale**:
- This preserves the established one-line developer experience and matches how caching, resilience, and telemetry are enabled.
- It keeps the batch feature opt-in so existing consumers do not get additional services unless requested.
- It aligns directly with FR-022.

**Alternatives considered**:
- Always register the orchestrator: Rejected because the spec explicitly requires opt-in behavior.
- Separate `AddHttpClientUtilityWithBatchExecution()` helper only: Rejected because the primary API should remain the existing options pattern.