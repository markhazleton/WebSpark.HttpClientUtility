# Tasks: Harvest HttpClientDecoratorPattern Ideas

**Input**: Design documents from `/specs/001-harvest-httpclient-ideas/`
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`

**Tests**: Include automated verification tasks for adopted code changes because the feature specification requires automated verification evidence for every adopt-now candidate.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Prepare the harvest workspace and target documentation/example locations.

- [ ] T001 Create the harvest working folder at `copilot/session-2026-03-30/`
- [ ] T002 Create the initial audit notes file at `copilot/session-2026-03-30/httpclientdecoratorpattern-audit.md`
- [ ] T003 [P] Create the harvested examples source folder structure under `src/pages/examples/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Establish the traceability artifacts and release-governance records that every story depends on.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

- [ ] T004 Create the candidate inventory document in `copilot/session-2026-03-30/harvest-candidate-inventory.md`
- [ ] T005 Create the decision record register in `copilot/session-2026-03-30/harvest-decision-records.md`
- [ ] T006 Create the adoption work item tracker in `copilot/session-2026-03-30/adoption-work-items.md`
- [ ] T007 Create the sunset readiness tracker in `copilot/session-2026-03-30/sunset-readiness.md`
- [ ] T007a Document release-readiness criteria in `specs/001-harvest-httpclient-ideas/quickstart.md`: a candidate transitions to `release-ready` only when automated verification evidence, user-facing documentation evidence, and migration guidance (if breaking-impact) are all linked in `copilot/session-2026-03-30/adoption-work-items.md` (satisfies FR-005)

**Checkpoint**: Foundation ready - user story implementation can now begin.

---

## Phase 3: User Story 1 - Create Harvest Backlog (Priority: P1) 🎯 MVP

**Goal**: Produce a complete, traceable harvest backlog and decision matrix for the legacy repository.

**Independent Test**: Review `copilot/session-2026-03-30/harvest-candidate-inventory.md`, `copilot/session-2026-03-30/harvest-decision-records.md`, and `copilot/session-2026-03-30/adoption-work-items.md` and confirm every identified idea has a decision state, rationale, impact classification, and target area.

- [ ] T008 [US1] Populate `copilot/session-2026-03-30/httpclientdecoratorpattern-audit.md` with the complete repository audit from `C:\GitHub\MarkHazleton\HttpClientDecoratorPattern`
- [ ] T009 [US1] Populate `copilot/session-2026-03-30/harvest-candidate-inventory.md` with all unique harvest candidates and novelty assessments
- [ ] T010 [P] [US1] Populate `copilot/session-2026-03-30/harvest-decision-records.md` using `specs/001-harvest-httpclient-ideas/contracts/decision-record-contract.md`
- [ ] T011 [P] [US1] Populate `copilot/session-2026-03-30/adoption-work-items.md` with implementation targets, impact levels, and evidence requirements for every `adopt-now` candidate

**Checkpoint**: User Story 1 is complete when the harvest backlog and decision matrix are fully traceable and independently reviewable.

---

## Phase 4: User Story 2 - Execute Approved Adoptions (Priority: P2)

**Goal**: Implement the approved adopt-now candidates in the primary repository with automated verification and user-facing documentation.

> **Pre-Vetting Note**: T012–T018 target the `RetryDelaySeconds`/`CircuitBreakerDurationSeconds` configuration binding and related example pages based on pre-approved candidates identified in `specs/001-harvest-httpclient-ideas/research.md` (Decision 5). These are not speculative; they reflect research completed before Phase 3. If Phase 3 uncovers additional or different adopt-now candidates, create corresponding test and implementation tasks following the same pattern before proceeding with T012–T018.

**Independent Test**: Verify the resilience configuration enhancement works through automated tests, then confirm the harvested example pages and documentation updates describe the adopted ideas and any migration guidance without relying on the legacy repository.

### Tests for User Story 2 ⚠️

> **NOTE: Write these tests first and ensure they fail before implementation.**

- [ ] T012 [P] [US2] Add configuration binding tests for alternate seconds-based resilience keys in `WebSpark.HttpClientUtility.Test/ServiceCollectionExtensionsTests.cs`
- [ ] T013 [P] [US2] Add regression tests for existing resilience option behavior in `WebSpark.HttpClientUtility.Test/RequestResult/HttpRequestResultTests.cs`

### Implementation for User Story 2

- [ ] T014 [US2] Implement support for `RetryDelaySeconds` and `CircuitBreakerDurationSeconds` binding in `WebSpark.HttpClientUtility/ServiceCollectionExtensions.cs`
- [ ] T015 [US2] Update XML documentation and option guidance in `WebSpark.HttpClientUtility/RequestResult/HttpRequestResultPollyOptions.cs`
- [ ] T016 [P] [US2] Create a harvested circuit-breaker and concurrent-calls examples page in `src/pages/examples/httpclient-decorator-pattern-scenarios.md`
- [ ] T017 [P] [US2] Create a harvested crawler-progress and site-analysis examples page in `src/pages/examples/crawler-harvested-scenarios.md`
- [ ] T018 [US2] Update the examples index in `src/pages/examples.md`
- [ ] T019 [US2] Update user-facing documentation in `README.md` and `documentation/GettingStarted.md` for the adopted configuration and examples
- [ ] T020 [US2] Add release and migration guidance for adopted items in `CHANGELOG.md`; update the internal working summary in `copilot/session-2026-03-30/final-harvest-summary.md` with release-ready status and evidence links for each adopted candidate

**Checkpoint**: User Story 2 is complete when adopted code changes are verified automatically and the primary repository documents the harvested examples and any migration implications.

---

## Phase 5: User Story 3 - Sunset Redundant Repository (Priority: P3)

**Goal**: Prepare the legacy repository for archival without user confusion and only after all adopt-now work is complete.

**Independent Test**: Review the legacy repository notice, the final harvest summary, and the sunset readiness tracker to confirm the archive gate cannot be satisfied until every adopt-now work item is shipped.

- [ ] T021 [US3] Update the legacy repository notice in `../HttpClientDecoratorPattern/README.md` (assumes the legacy repository is checked out as a sibling of this repository; this task is performed manually and cannot run in GitHub Actions CI)
- [ ] T022 [P] [US3] Draft the archive announcement and replacement guidance in `copilot/session-2026-03-30/httpclientdecoratorpattern-archive-issue.md`
- [ ] T023 [P] [US3] Update `copilot/session-2026-03-30/sunset-readiness.md` using `specs/001-harvest-httpclient-ideas/contracts/sunset-readiness-contract.md`
- [ ] T024 [US3] Publish the final harvest outcome summary: update internal working copy at `copilot/session-2026-03-30/final-harvest-summary.md` and create the consumer-facing version at `documentation/harvest-summary.md` (satisfies FR-010)
- [ ] T025 [US3] Perform the archive-readiness review against `copilot/session-2026-03-30/adoption-work-items.md` and `copilot/session-2026-03-30/sunset-readiness.md`

**Checkpoint**: User Story 3 is complete when the legacy repository can be clearly redirected and the archive gate is documented as satisfied only after all adopt-now work is shipped.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Final validation across stories and repository-wide release readiness.

- [ ] T026 [P] Validate the harvested docs source files in `src/pages/examples/` and `src/pages/examples.md`
- [ ] T027 Run solution-level build and test validation for `WebSpark.HttpClientUtility.sln`
- [ ] T028 [P] Reconcile `copilot/session-2026-03-30/harvest-decision-records.md`, `copilot/session-2026-03-30/adoption-work-items.md`, and `copilot/session-2026-03-30/sunset-readiness.md` against `specs/001-harvest-httpclient-ideas/quickstart.md`
- [ ] T029 Bump version in `WebSpark.HttpClientUtility/WebSpark.HttpClientUtility.csproj` (and `WebSpark.HttpClientUtility.Crawler/WebSpark.HttpClientUtility.Crawler.csproj` if crawler candidates were adopted): use MAJOR for any breaking-impact adoption, MINOR for backward-compatible additions; commit separately from all feature work per constitution release discipline
- [ ] T030 [P] Verify SC-001: cross-reference every entry in `copilot/session-2026-03-30/harvest-candidate-inventory.md` against `copilot/session-2026-03-30/harvest-decision-records.md`; confirm zero uncategorized candidates remain
- [ ] T031 [P] Verify SC-002 and SC-003: confirm every adopt-now entry in `copilot/session-2026-03-30/adoption-work-items.md` has linked automated verification evidence and user-facing documentation evidence; confirm every breaking-impact adoption has migration guidance in `CHANGELOG.md`
- [ ] T032 [P] Verify SC-004: confirm `copilot/session-2026-03-30/sunset-readiness.md` shows all adopt-now work items as shipped and all communication prerequisites satisfied; confirm `documentation/harvest-summary.md` is published before approving archive readiness
- [ ] T033 [P] Package governance check: confirm no new `.csproj` or package manifest was introduced during adoption work unless a compelling-separation Decision Record exists in `copilot/session-2026-03-30/harvest-decision-records.md` (satisfies FR-005b and FR-005c)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - blocks all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational completion
- **User Story 2 (Phase 4)**: Depends on User Story 1 because adopt-now implementation targets come from the decision matrix
- **User Story 3 (Phase 5)**: Depends on User Story 2 because sunset cannot begin until all adopt-now items are fully implemented
- **Polish (Phase 6)**: Depends on all desired user stories being complete; includes T029 (version bump), T030–T032 (success criteria verification), T033 (package governance gate)

### User Story Dependencies

- **User Story 1 (P1)**: Starts after Foundational - no dependency on other stories
- **User Story 2 (P2)**: Starts after User Story 1 - depends on the approved candidate list and evidence model
- **User Story 3 (P3)**: Starts after User Story 2 - depends on shipped adopt-now outcomes and migration messaging

### Within Each User Story

- Tests for adopted code changes must be written and fail before implementation
- Audit and inventory before decision records
- Decision records before adoption work items
- Automated verification before release-ready documentation sign-off
- Sunset communication before archive-readiness review

### Parallel Opportunities

- T003 can run in parallel with T001-T002
- T010 and T011 can run in parallel after T009 completes
- T016 and T017 can run in parallel once T014 implementation is complete; they may overlap with T015 XML documentation updates since they target different files
- T022 and T023 can run in parallel once User Story 3 starts
- T026 and T028 can run in parallel during Polish

---

## Parallel Example: User Story 1

```text
- [ ] T010 [P] [US1] Populate copilot/session-2026-03-30/harvest-decision-records.md using specs/001-harvest-httpclient-ideas/contracts/decision-record-contract.md
- [ ] T011 [P] [US1] Populate copilot/session-2026-03-30/adoption-work-items.md with implementation targets, impact levels, and evidence requirements for every adopt-now candidate
```

## Parallel Example: User Story 2

```text
- [ ] T012 [P] [US2] Add configuration binding tests for alternate seconds-based resilience keys in WebSpark.HttpClientUtility.Test/ServiceCollectionExtensionsTests.cs
- [ ] T013 [P] [US2] Add regression tests for existing resilience option behavior in WebSpark.HttpClientUtility.Test/RequestResult/HttpRequestResultTests.cs
- [ ] T016 [P] [US2] Create a harvested circuit-breaker and concurrent-calls examples page in src/pages/examples/httpclient-decorator-pattern-scenarios.md
- [ ] T017 [P] [US2] Create a harvested crawler-progress and site-analysis examples page in src/pages/examples/crawler-harvested-scenarios.md
```

## Parallel Example: User Story 3

```text
- [ ] T022 [P] [US3] Draft the archive announcement and replacement guidance in copilot/session-2026-03-30/httpclientdecoratorpattern-archive-issue.md
- [ ] T023 [P] [US3] Update copilot/session-2026-03-30/sunset-readiness.md using specs/001-harvest-httpclient-ideas/contracts/sunset-readiness-contract.md
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Review the harvest backlog and decision matrix independently
5. Use the approved candidate list to scope implementation work

### Incremental Delivery

1. Complete Setup + Foundational
2. Add User Story 1 and validate the harvest backlog
3. Add User Story 2 and validate automated verification plus documentation evidence
4. Add User Story 3 and validate archive readiness only after adopt-now items ship
5. Finish with solution-level validation and artifact reconciliation

### Parallel Team Strategy

1. One maintainer completes Setup + Foundational artifacts
2. After User Story 1 decisions are locked:
   - Maintainer A handles resilience configuration and automated tests
   - Maintainer B handles harvested example pages and docs updates
3. After User Story 2 ships:
   - Maintainer A prepares legacy repository messaging
   - Maintainer B validates sunset readiness artifacts

---

## Notes

- All tasks follow the required checklist format.
- User Story 1 is the recommended MVP scope.
- User Story 2 contains the only required automated test tasks because it includes adopted runtime changes.
- User Story 3 must not be completed before User Story 2 is fully shipped.
- T029 (version bump) MUST be a separate commit from all feature work per constitution release discipline.
- T030–T033 (verification and governance tasks) are prerequisite gates for release sign-off and archive approval.
- T007a remains in Phase 2 to ensure release-readiness criteria are defined before any adoption work begins.