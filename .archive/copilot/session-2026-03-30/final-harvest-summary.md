# Final Harvest Summary (Internal)

Date: 2026-03-30
Feature: specs/001-harvest-httpclient-ideas

## Outcome by Candidate

| Candidate | Decision | Status | Verification Evidence | Documentation Evidence |
|---|---|---|---|---|
| HC-001 | adopt-now | release-ready | dotnet test WebSpark.HttpClientUtility.Test/WebSpark.HttpClientUtility.Test.csproj --configuration Release --no-restore (720/720 passed) | WebSpark.HttpClientUtility/ServiceCollectionExtensions.cs, WebSpark.HttpClientUtility/RequestResult/HttpRequestResultPollyOptions.cs, README.md, documentation/GettingStarted.md |
| HC-002 | adopt-now | release-ready | docs source validation (examples pages + index) | src/pages/examples/httpclient-decorator-pattern-scenarios.md, src/pages/examples.md |
| HC-003 | adopt-now | release-ready | docs source validation (examples pages + index) | src/pages/examples/crawler-harvested-scenarios.md, src/pages/examples.md |
| HC-004 | docs-only | complete | n/a | captured in harvest-decision-records.md |
| HC-005 | adopt-now | release-ready | dotnet test WebSpark.HttpClientUtility.Test/WebSpark.HttpClientUtility.Test.csproj --configuration Release --no-restore (720/720 passed) | README.md, documentation/GettingStarted.md, CHANGELOG.md |
| HC-006 | defer | tracked | n/a | captured in harvest-decision-records.md |
| HC-007 | reject | tracked | n/a | captured in harvest-decision-records.md |

## Release Notes Alignment

- CHANGELOG updated with 2.6.0 harvest adoption entry.
- Version bumped lockstep to 2.6.0 in Directory.Build.props.
- Crawler metadata updated to require base package [2.6.0].

## Sunset Gate State

- allAdoptNowComplete: no (release-ready but not yet shipped)
- migrationCommunicationPublished: yes (CHANGELOG + docs updates)
- archiveNoticePublished: pending final merge to legacy README
- finalHarvestSummaryPublished: yes (internal + consumer-facing summary generated)
- approvedForArchive: no

## Reconciliation Against quickstart.md (T028)

- Candidate inventory, decision records, adoption work items, and sunset readiness are cross-linked.
- Release-ready transitions include both verification and documentation evidence.
- Breaking-impact migration guidance is not required for this batch because no adopt-now item is marked breaking.

## Success Criteria Verification

- SC-001: pass (every inventory candidate has a decision record).
- SC-002: pass (all adopt-now entries have automated verification evidence).
- SC-003: pass (all adopt-now entries have documentation evidence; no breaking migration guidance required).
- SC-004: pending (sunset readiness remains not-approved until adopt-now items are shipped).

## Package Governance Check (T033)

- No new `.csproj` or package manifest was introduced during harvest adoption.
- No `new-package-exception` decision record was required.