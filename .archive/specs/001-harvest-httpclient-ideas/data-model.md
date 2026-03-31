# Data Model: Harvest HttpClientDecoratorPattern Ideas

## Overview

This feature manages planning and release-governance artifacts rather than introducing a runtime domain model. The entities below describe the information required to move an idea from discovery to implementation and, eventually, to legacy repository archival.

## Entity: HarvestCandidate

**Purpose**: Represents one feature, requirement, pattern, example, or repository practice discovered during the legacy repository audit.

**Fields**:
- `candidateId`: Stable unique identifier for traceability.
- `title`: Short name for the candidate.
- `sourceRepository`: Legacy repository name.
- `sourceReference`: File, page, workflow, or documentation location where the candidate was found.
- `candidateType`: Runtime feature, docs/example, packaging idea, release/process requirement, or sunset communication item.
- `noveltyAssessment`: Already present, partially present, or not present in the primary repository.
- `targetArea`: Core package, crawler package, documentation/examples, release process, or legacy repository sunset assets.
- `decisionState`: Adopt now, defer, docs-only, or reject.

**Validation Rules**:
- `candidateId`, `title`, `sourceRepository`, `sourceReference`, `candidateType`, and `decisionState` are required.
- `decisionState` must be one of the allowed states.
- `targetArea` must reflect an allowed destination for the chosen decision state.

## Entity: DecisionRecord

**Purpose**: Captures the decision for a HarvestCandidate and the rationale behind it.

**Fields**:
- `decisionId`: Stable unique identifier.
- `candidateId`: Reference to the associated HarvestCandidate.
- `decisionState`: Adopt now, defer, docs-only, or reject.
- `impactLevel`: Non-breaking, breaking, docs-only, or sunset-only.
- `rationale`: Brief explanation for the decision.
- `packageStrategy`: Existing package, documentation/examples only, or new package exception.
- `migrationRequired`: Yes or no.
- `migrationSummary`: Required when `migrationRequired` is yes.

**Validation Rules**:
- Every DecisionRecord must reference one existing HarvestCandidate.
- `migrationSummary` is required for breaking-impact decisions.
- `packageStrategy` may specify a new package only when compelling separation is proven.

## Entity: AdoptionWorkItem

**Purpose**: Tracks the implementation and release evidence for an adopt-now candidate.

**Fields**:
- `workItemId`: Stable unique identifier.
- `candidateId`: Associated HarvestCandidate.
- `implementationTargets`: One or more repository locations impacted by the work.
- `verificationEvidence`: Links or references to automated verification results.
- `documentationEvidence`: Links or references to user-facing documentation updates.
- `releaseStatus`: Planned, in progress, release-ready, or shipped.

**Validation Rules**:
- At least one implementation target is required.
- `verificationEvidence` and `documentationEvidence` are required before `releaseStatus` may become `release-ready`.

## Entity: SunsetReadinessRecord

**Purpose**: Proves whether the legacy repository can be archived.

**Fields**:
- `readinessId`: Stable unique identifier.
- `legacyRepository`: Repository scheduled for archival.
- `allAdoptNowComplete`: Yes or no.
- `migrationCommunicationPublished`: Yes or no.
- `archiveNoticePublished`: Yes or no.
- `finalHarvestSummaryPublished`: Yes or no.
- `approvedForArchive`: Yes or no.

**Validation Rules**:
- `approvedForArchive` may only be yes when all other readiness fields are yes.
- `allAdoptNowComplete` must stay no until every AdoptionWorkItem is shipped.

## Relationships

- One `HarvestCandidate` has exactly one `DecisionRecord`.
- One `HarvestCandidate` may have zero or one `AdoptionWorkItem`.
- One `SunsetReadinessRecord` depends on the complete set of adopt-now `AdoptionWorkItem` entities for the feature.

## State Transitions

### HarvestCandidate / DecisionRecord

`discovered` → `assessed` → `decided`

- `decided` branches to `adopt-now`, `defer`, `docs-only`, or `reject`.

### AdoptionWorkItem

`planned` → `in-progress` → `release-ready` → `shipped`

- Transition to `release-ready` requires automated verification evidence and documentation evidence.
- Breaking-impact items also require migration guidance before `release-ready`.

### SunsetReadinessRecord

`not-ready` → `communication-ready` → `archive-approved`

- `communication-ready` requires migration and archive messaging.
- `archive-approved` requires all adopt-now work items to be shipped.
