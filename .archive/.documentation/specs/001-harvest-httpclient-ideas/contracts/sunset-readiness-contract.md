# Contract: Sunset Readiness Record

## Purpose

Defines the minimum evidence required before archiving the legacy repository.

## Required Fields

| Field | Required | Description |
|-------|----------|-------------|
| `readinessId` | Yes | Unique identifier for the readiness record |
| `legacyRepository` | Yes | Repository being sunset |
| `allAdoptNowComplete` | Yes | `yes` only when every adopt-now item is shipped |
| `migrationCommunicationPublished` | Yes | `yes` when migration guidance is publicly available |
| `archiveNoticePublished` | Yes | `yes` when the legacy repository clearly points to the primary repository |
| `finalHarvestSummaryPublished` | Yes | `yes` when the final harvest outcomes are documented |
| `approvedForArchive` | Yes | `yes` only when every prior field is `yes` |

## Rules

1. The legacy repository cannot be archived while `allAdoptNowComplete` is `no`.
2. The legacy repository cannot be archived while any required communication field is `no`.
3. `approvedForArchive` must remain `no` until every prerequisite has been satisfied.
4. The readiness record must be reviewable alongside the final harvest summary.