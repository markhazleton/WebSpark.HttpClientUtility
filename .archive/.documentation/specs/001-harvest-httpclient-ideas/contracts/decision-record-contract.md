# Contract: Decision Record

## Purpose

Defines the minimum required information for recording a harvest decision.

## Required Fields

| Field | Required | Description |
|-------|----------|-------------|
| `decisionId` | Yes | Unique identifier for the decision record |
| `candidateId` | Yes | Identifier of the related harvest candidate |
| `decisionState` | Yes | One of: `adopt-now`, `defer`, `docs-only`, `reject` |
| `impactLevel` | Yes | One of: `non-breaking`, `breaking`, `docs-only`, `sunset-only` |
| `rationale` | Yes | Brief explanation for the decision |
| `packageStrategy` | Yes | One of: `existing-package`, `docs-only`, `new-package-exception` |
| `migrationRequired` | Yes | `yes` or `no` |
| `migrationSummary` | Conditional | Required when `migrationRequired` is `yes` |

## Rules

1. Every harvest candidate must have exactly one decision record.
2. `new-package-exception` is valid only when the candidate serves a distinct consumer need and has clear standalone release value.
3. `migrationSummary` must be present for every breaking-impact adoption.
4. A record marked `adopt-now` must be traceable to an adoption work item.
