# Harvest Decision Records

Date: 2026-03-30

## DR-001
- decisionId: DR-001
- candidateId: HC-001
- decisionState: adopt-now
- impactLevel: non-breaking
- rationale: Add compatibility binding for seconds-based keys already used in legacy examples without changing existing TimeSpan options.
- packageStrategy: existing-package
- migrationRequired: no
- migrationSummary: n/a

## DR-002
- decisionId: DR-002
- candidateId: HC-002
- decisionState: adopt-now
- impactLevel: docs-only
- rationale: Preserve practical resilience scenarios in the primary docs site for discoverability.
- packageStrategy: docs-only
- migrationRequired: no
- migrationSummary: n/a

## DR-003
- decisionId: DR-003
- candidateId: HC-003
- decisionState: adopt-now
- impactLevel: docs-only
- rationale: Preserve crawler workflow and progress-reporting examples for crawler package users.
- packageStrategy: docs-only
- migrationRequired: no
- migrationSummary: n/a

## DR-004
- decisionId: DR-004
- candidateId: HC-004
- decisionState: docs-only
- impactLevel: docs-only
- rationale: Primary repository already documents decorator behavior; legacy explanation can be referenced but does not require runtime change.
- packageStrategy: docs-only
- migrationRequired: no
- migrationSummary: n/a

## DR-005
- decisionId: DR-005
- candidateId: HC-005
- decisionState: adopt-now
- impactLevel: docs-only
- rationale: Align README and GettingStarted configuration guidance with newly supported aliases and examples.
- packageStrategy: docs-only
- migrationRequired: no
- migrationSummary: n/a

## DR-006
- decisionId: DR-006
- candidateId: HC-006
- decisionState: defer
- impactLevel: docs-only
- rationale: Legacy presentation patterns are not required for package capability and may conflict with current documentation style.
- packageStrategy: docs-only
- migrationRequired: no
- migrationSummary: n/a

## DR-007
- decisionId: DR-007
- candidateId: HC-007
- decisionState: reject
- impactLevel: sunset-only
- rationale: Manual startup composition duplicates functionality already available via AddHttpClientUtility one-line registration.
- packageStrategy: existing-package
- migrationRequired: no
- migrationSummary: n/a