# Tasks: Batch Execution Orchestration

**Feature**: 004-batch-execution-orchestration  
**Input**: Design documents from `/specs/004-batch-execution-orchestration/`  
**Prerequisites**: plan.md ✓, spec.md ✓, research.md ✓, data-model.md ✓, contracts/ ✓, quickstart.md ✓

**Tests**: MSTest coverage is explicitly required for User Stories 1-6. Write the listed tests before implementation, keep the existing automated suites green, and treat manual demo validation as supplemental acceptance coverage rather than the only check.

**Organization**: Tasks are grouped by user story so each story can be implemented and validated independently.

---

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel when dependencies are satisfied and files do not overlap
- **[Story]**: Maps the task to a specific user story
- Every task includes the exact implementation file path

---

## Phase 1: Setup (Shared Structure)

**Purpose**: Create the source, test, and demo scaffolding for the feature before shared implementation begins.

- [X] T001 [P] Create the batch execution source and test folders under WebSpark.HttpClientUtility/BatchExecution/ and WebSpark.HttpClientUtility.Test/BatchExecution/
- [X] T002 [P] Create the demo MVC scaffolding files WebSpark.HttpClientUtility.Web/Controllers/BatchExecutionController.cs, WebSpark.HttpClientUtility.Web/Models/BatchExecutionViewModels.cs, WebSpark.HttpClientUtility.Web/Services/BatchExecutionDemoService.cs, and WebSpark.HttpClientUtility.Web/Views/BatchExecution/Index.cshtml

**Checkpoint**: Source, test, and demo locations exist and are ready for shared contracts.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish opt-in registration, shared batch contracts, and orchestration primitives that every user story depends on.

**⚠️ CRITICAL**: No user story work should begin until this phase is complete.

- [X] T003 Add `EnableBatchExecution` to `HttpClientUtilityOptions` in WebSpark.HttpClientUtility/ServiceCollectionExtensions.cs
- [X] T004 Create the public batch execution contracts in WebSpark.HttpClientUtility/BatchExecution/IBatchExecutionService.cs, WebSpark.HttpClientUtility/BatchExecution/IBatchExecutionResultSink.cs, and WebSpark.HttpClientUtility/BatchExecution/ITemplateSubstitutionService.cs
- [X] T005 [P] Create the shared batch models in WebSpark.HttpClientUtility/BatchExecution/BatchExecutionConfiguration.cs, WebSpark.HttpClientUtility/BatchExecution/BatchEnvironment.cs, WebSpark.HttpClientUtility/BatchExecution/BatchUserContext.cs, WebSpark.HttpClientUtility/BatchExecution/BatchRequestDefinition.cs, WebSpark.HttpClientUtility/BatchExecution/BatchExecutionItemResult.cs, WebSpark.HttpClientUtility/BatchExecution/BatchExecutionResult.cs, WebSpark.HttpClientUtility/BatchExecution/BatchExecutionStatistics.cs, and WebSpark.HttpClientUtility/BatchExecution/BatchProgress.cs
- [X] T006 [P] Create the thread-safe statistics helper shell in WebSpark.HttpClientUtility/BatchExecution/BatchStatisticsCollector.cs
- [X] T007 Create the orchestrator shell and conditional DI wiring in WebSpark.HttpClientUtility/BatchExecution/BatchExecutionService.cs and WebSpark.HttpClientUtility/ServiceCollectionExtensions.cs
- [X] T008 Create opt-in registration coverage in WebSpark.HttpClientUtility.Test/BatchExecution/ServiceCollectionExtensionsBatchExecutionTests.cs
- [X] T041 [P] Write input validation tests covering invalid Iterations (<0), invalid MaxConcurrency (<1), malformed base URLs, and empty collections in WebSpark.HttpClientUtility.Test/BatchExecution/ServiceCollectionExtensionsBatchExecutionTests.cs and implement validation logic in WebSpark.HttpClientUtility/BatchExecution/BatchExecutionService.cs

**Checkpoint**: Feature flag, contracts, models, service shell, DI guardrails, and input validation are in place.

---

## Phase 3: User Story 1 - Parameterized Request Execution (Priority: P1) 🎯 MVP

**Goal**: Deliver single-pass template substitution for request paths and bodies with unresolved placeholders preserved.

**Independent Test**: Render path and body templates from `BatchUserContext.Properties` and verify resolved values, `{{encoded_user_name}}`, raw non-encoded substitution, and missing-key preservation without needing the rest of the orchestration pipeline.

### Tests for User Story 1

- [X] T009 [US1] Write template rendering tests for path substitution, body substitution, missing keys, raw `{{encoded_user_name}}` substitution, and no extra URL encoding in WebSpark.HttpClientUtility.Test/BatchExecution/TemplateSubstitutionServiceTests.cs

### Implementation for User Story 1

- [X] T010 [US1] Implement `ITemplateSubstitutionService` with single-pass rendering rules in WebSpark.HttpClientUtility/BatchExecution/TemplateSubstitutionService.cs
- [X] T011 [US1] Apply rendered path and body composition from request definitions and user properties in WebSpark.HttpClientUtility/BatchExecution/BatchExecutionService.cs
- [X] T012 [US1] Add nested-brace, empty-value, and legacy-token regression coverage in WebSpark.HttpClientUtility.Test/BatchExecution/TemplateSubstitutionServiceTests.cs

**Checkpoint**: Template substitution works independently for outbound path and body rendering.

---

## Phase 4: User Story 2 - Combinatorial Batch Execution (Priority: P2)

**Goal**: Execute the full environment × user × request × iteration matrix through `IHttpRequestResultService` with throttling, cancellation, and progress reporting.

**Independent Test**: Configure a small matrix, execute the batch, and verify planned count, completed items, concurrency limits, cancellation behavior, and incremental progress updates.

### Tests for User Story 2

- [X] T013 [P] [US2] Write combinatorial expansion, planned-count, zero-work, and explicit custom-method body-capability tests in WebSpark.HttpClientUtility.Test/BatchExecution/BatchExecutionExpansionTests.cs
- [X] T014 [P] [US2] Write concurrency throttling, cancellation, progress callback, and correlation-ID propagation tests in WebSpark.HttpClientUtility.Test/BatchExecution/BatchExecutionExecutionTests.cs
- [X] T042 [P] [US2] Write unreachable-environment edge case test verifying that when one environment is completely unreachable, execution continues for other environments and statistics reflect the failures in WebSpark.HttpClientUtility.Test/BatchExecution/BatchExecutionExecutionTests.cs

### Implementation for User Story 2

- [X] T015 [US2] Implement environment, user, request, and iteration expansion plus planned-count calculation in WebSpark.HttpClientUtility/BatchExecution/BatchExecutionService.cs
- [X] T016 [US2] Implement semaphore-based dispatch, cancellation boundaries, and per-item execution through `IHttpRequestResultService` in WebSpark.HttpClientUtility/BatchExecution/BatchExecutionService.cs
- [X] T017 [US2] Map HTTP methods, explicit body-capability metadata for custom methods, conditional request bodies, headers, and correlation IDs into outbound requests in WebSpark.HttpClientUtility/BatchExecution/BatchExecutionService.cs

**Checkpoint**: The orchestrator executes the full matrix correctly and respects configured concurrency and cancellation limits.

---

## Phase 5: User Story 3 - Execution Statistics Collection (Priority: P3)

**Goal**: Produce thread-safe aggregate metrics and percentile snapshots for completed work items.

**Independent Test**: Run a batch with known timings and outcomes, then verify total, success, failure, P50, P95, P99, and per-dimension breakdowns.

### Tests for User Story 3

- [X] T018 [P] [US3] Write percentile and per-dimension aggregation tests in WebSpark.HttpClientUtility.Test/BatchExecution/BatchStatisticsCollectorTests.cs
- [X] T019 [P] [US3] Write concurrent snapshot and empty-result regression tests in WebSpark.HttpClientUtility.Test/BatchExecution/BatchProgressTests.cs

### Implementation for User Story 3

- [X] T020 [US3] Implement thread-safe counts, breakdown dictionaries, and percentile snapshot calculations in WebSpark.HttpClientUtility/BatchExecution/BatchStatisticsCollector.cs
- [X] T021 [US3] Feed running statistics snapshots and final aggregates into batch results and progress updates in WebSpark.HttpClientUtility/BatchExecution/BatchExecutionService.cs

**Checkpoint**: Final results and progress updates expose accurate statistics under concurrent execution.

---

## Phase 6: User Story 4 - Response Comparison and Hashing (Priority: P4)

**Goal**: Compute deterministic response hashes so matching and divergent responses can be compared across environments and users.

**Independent Test**: Execute requests with identical and differing bodies, then verify matching content yields identical hashes and differing content yields different hashes.

### Tests for User Story 4

- [X] T022 [US4] Write deterministic response hash tests for identical and different response bodies in WebSpark.HttpClientUtility.Test/BatchExecution/BatchExecutionHashingTests.cs

### Implementation for User Story 4

- [X] T023 [US4] Implement SHA-256 response body hashing for completed requests in WebSpark.HttpClientUtility/BatchExecution/BatchExecutionService.cs
- [X] T024 [US4] Populate response hash and comparison-ready metadata on `BatchExecutionItemResult` in WebSpark.HttpClientUtility/BatchExecution/BatchExecutionService.cs

**Checkpoint**: Each completed item includes a stable response hash suitable for comparison workflows.

---

## Phase 7: User Story 5 - Result Output Streaming (Priority: P5)

**Goal**: Stream item results to an optional sink as each request completes instead of buffering them until the end.

**Independent Test**: Execute a small batch with a mock sink and verify the sink receives successful and failed items incrementally during the run.

### Tests for User Story 5

- [X] T025 [US5] Write incremental sink delivery tests for success and failure paths, asserting the full FR-016 item metadata contract in WebSpark.HttpClientUtility.Test/BatchExecution/BatchExecutionStreamingTests.cs

### Implementation for User Story 5

- [X] T026 [US5] Invoke `IBatchExecutionResultSink` immediately after each completed item and populate full FR-016 metadata (environment, path, method, status, hash, duration, user, timestamp) in WebSpark.HttpClientUtility/BatchExecution/BatchExecutionService.cs
- [X] T027 [US5] Preserve sink delivery, progress reporting, and graceful partial results during cancellation in WebSpark.HttpClientUtility/BatchExecution/BatchExecutionService.cs

**Checkpoint**: Consumers can observe results as they complete without waiting for full batch completion.

---

## Phase 8: User Story 6 - Interactive Web Demo (Priority: P6)

**Goal**: Add an MVC demo page that starts capped batch runs, polls for status, and renders live statistics and final results.

**Independent Test**: Launch the web app, open `/BatchExecution`, submit the pre-populated sample configuration, and confirm the page shows live progress plus final statistics and results.

### Tests for User Story 6

- [X] T028 [US6] Write automated controller and demo run-tracking MSTest coverage in WebSpark.HttpClientUtility.Test/BatchExecution/BatchExecutionDemoControllerTests.cs and WebSpark.HttpClientUtility.Test/BatchExecution/BatchExecutionDemoServiceTests.cs

### Implementation for User Story 6

- [X] T029 [US6] Create request, response, and run-state view models in WebSpark.HttpClientUtility.Web/Models/BatchExecutionViewModels.cs
- [X] T030 [US6] Implement capped in-memory demo run tracking and orchestration integration in WebSpark.HttpClientUtility.Web/Services/BatchExecutionDemoService.cs
- [X] T031 [US6] Implement `GET /BatchExecution`, `POST /BatchExecution/runs`, and `GET /BatchExecution/runs/{runId}` in WebSpark.HttpClientUtility.Web/Controllers/BatchExecutionController.cs
- [X] T032 [US6] Enable batch execution and register demo services in WebSpark.HttpClientUtility.Web/Program.cs
- [X] T033 [US6] Build the interactive BatchExecution Razor page with pre-populated samples, polling, and statistics rendering in WebSpark.HttpClientUtility.Web/Views/BatchExecution/Index.cshtml
- [X] T034 [US6] Add a Batch Execution navigation entry in WebSpark.HttpClientUtility.Web/Views/Shared/_Layout.cshtml
- [ ] T035 [US6] Manually validate the capped run flow and live update behavior against specs/004-batch-execution-orchestration/quickstart.md and specs/004-batch-execution-orchestration/contracts/batch-execution-demo-api-contract.md

**Checkpoint**: The web demo demonstrates capped batch execution with live statistics and final results.

---

## Phase 9: Polish & Cross-Cutting Concerns

**Purpose**: Finish documentation, API polish, and end-to-end validation across the library and demo app.

- [X] T036 [P] Update batch execution usage documentation in README.md and documentation/GettingStarted.md
- [X] T037 [P] Add XML documentation for public batch execution APIs in WebSpark.HttpClientUtility/BatchExecution/*.cs and WebSpark.HttpClientUtility/ServiceCollectionExtensions.cs
- [X] T038 Write batch pipeline integration tests covering resilience, caching, telemetry, and authentication compatibility when `EnableBatchExecution` is enabled in WebSpark.HttpClientUtility.Test/BatchExecution/BatchExecutionPipelineIntegrationTests.cs
- [X] T039 Run full solution `dotnet build` and `dotnet test` validation in Release mode across all configured target frameworks, including the new batch execution tests and the pre-existing regression suites
- [X] T040 Resolve any new analyzer or build warnings introduced by the batch execution feature before completion

---

## Dependencies & Execution Order

### Phase Dependencies

- **Phase 1: Setup**: No dependencies.
- **Phase 2: Foundational**: Depends on Phase 1 and blocks all user stories.
- **Phase 3: US1**: Depends on Phase 2.
- **Phase 4: US2**: Depends on US1 because request rendering is required for expanded executions.
- **Phase 5: US3**: Depends on US2 because statistics consume completed batch executions.
- **Phase 6: US4**: Depends on US2 because hashing is applied to completed execution results.
- **Phase 7: US5**: Depends on US2 because the sink streams completed execution results.
- **Phase 8: US6**: Depends on US1, US2, and US3 because the demo exercises rendering, execution, and statistics.
- **Phase 9: Polish**: Depends on all targeted user stories being complete.

### User Story Dependency Graph

```text
Setup -> Foundational -> US1 -> US2 -> US3 -> US6
                                 |      \
                                 |       -> US4
                                 |
                                 -> US5
```

### Story Completion Order

1. US1 - Parameterized Request Execution
2. US2 - Combinatorial Batch Execution
3. US3 - Execution Statistics Collection
4. US4 - Response Comparison and Hashing
5. US5 - Result Output Streaming
6. US6 - Interactive Web Demo

---

## Parallel Execution Examples

### User Story 1

Limited parallelism after the first failing test because the core work centers on WebSpark.HttpClientUtility/BatchExecution/TemplateSubstitutionService.cs and WebSpark.HttpClientUtility.Test/BatchExecution/TemplateSubstitutionServiceTests.cs.

### User Story 2

```text
T013 WebSpark.HttpClientUtility.Test/BatchExecution/BatchExecutionExpansionTests.cs
T014 WebSpark.HttpClientUtility.Test/BatchExecution/BatchExecutionExecutionTests.cs
```

### User Story 3

```text
T018 WebSpark.HttpClientUtility.Test/BatchExecution/BatchStatisticsCollectorTests.cs
T019 WebSpark.HttpClientUtility.Test/BatchExecution/BatchProgressTests.cs
```

### User Story 4

Limited parallelism because hashing and result metadata both land in WebSpark.HttpClientUtility/BatchExecution/BatchExecutionService.cs.

### User Story 5

Limited parallelism because streaming behavior, cancellation handling, and progress ordering all converge in WebSpark.HttpClientUtility/BatchExecution/BatchExecutionService.cs.

### User Story 6

```text
T028 WebSpark.HttpClientUtility.Test/BatchExecution/BatchExecutionDemoControllerTests.cs
T029 WebSpark.HttpClientUtility.Web/Models/BatchExecutionViewModels.cs
T030 WebSpark.HttpClientUtility.Web/Services/BatchExecutionDemoService.cs
```

After those files stabilize, continue with:

```text
T031 WebSpark.HttpClientUtility.Web/Controllers/BatchExecutionController.cs
T033 WebSpark.HttpClientUtility.Web/Views/BatchExecution/Index.cshtml
```

---

## Implementation Strategy

### MVP First

1. Complete Phase 1 and Phase 2.
2. Complete Phase 3 (US1) to establish template rendering.
3. Complete Phase 4 (US2) to deliver the first end-to-end batch execution path.
4. Validate the library with the US1 and US2 independent tests before moving on.

### Incremental Delivery

1. Add US3 next so executions return actionable statistics.
2. Add US4 and US5 as follow-on library capabilities once the execution core is stable.
3. Finish with US6 once both automated demo tests and the interactive page behavior are ready.
4. Use Phase 9 for documentation, XML comments, pipeline compatibility coverage, full-solution validation, and warning cleanup.

### Task Count Summary

- Total tasks: 42
- Setup + Foundational: 10
- US1: 4
- US2: 6
- US3: 4
- US4: 3
- US5: 3
- US6: 8
- Polish: 5

---

## Notes

- All tasks follow the required checklist format: checkbox, task ID, optional `[P]`, required story label for story phases, and exact file path references.
- User stories remain independently testable at each checkpoint even though later stories build on earlier capabilities.
- The task plan keeps the existing request decorator chain unchanged and routes all orchestration work through `IHttpRequestResultService`.
