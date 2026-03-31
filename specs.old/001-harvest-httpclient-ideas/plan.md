# Implementation Plan: Harvest HttpClientDecoratorPattern Ideas

**Branch**: `001-harvest-httpclient-ideas` | **Date**: 2026-03-30 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-harvest-httpclient-ideas/spec.md`

## Summary

Harvest all worthwhile ideas from HttpClientDecoratorPattern into the primary WebSpark.HttpClientUtility repository by creating a traceable candidate inventory, implementing approved items in existing packages by default, permitting a new package only when a distinct consumer need and standalone release value are proven, and preparing the legacy repository for archive only after every adopt-now candidate is fully implemented with automated verification, user-facing documentation, and migration guidance where breaking changes occur.

## Technical Context

**Language/Version**: C# 12 on .NET 8, .NET 9, and .NET 10 LTS; Markdown for planning and documentation artifacts  
**Primary Dependencies**: Existing repository stack only unless a harvested candidate is approved: Microsoft.Extensions.*, Polly, OpenTelemetry, HtmlAgilityPack/CsvHelper/SignalR in crawler scope, GitHub Actions CI/CD  
**Storage**: Repository files and artifacts in `specs/`, library projects, test projects, docs, and release documentation  
**Testing**: MSTest + Moq, `dotnet build`, `dotnet test` across all target frameworks, documentation verification through artifact review  
**Target Platform**: Multi-project .NET library ecosystem for local Windows development and GitHub Actions-based build/release execution  
**Project Type**: Multi-project .NET library repository with separate crawler extension, test projects, docs site, and demo web app  
**Performance Goals**: No unapproved runtime regressions; adopted code changes must preserve current release quality gates, pass all target-framework tests, and introduce no new warnings  
**Constraints**: Preserve decorator order, one-line DI experience, XML documentation on public APIs, CI-only publishing, semantic versioning discipline, lockstep multi-package release behavior where required, dual-evidence adoption gates  
**Scale/Scope**: One legacy repository audit, one consolidated harvest backlog, possible updates to core package, crawler package, docs/examples, release communications, and legacy repository sunset assets

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Research Gate Review

| Principle | Status | Notes |
|-----------|--------|-------|
| Library-First Design | Pass | Harvest defaults to existing packages and docs; no new package without compelling separation. |
| Test Coverage and Quality | Pass | Plan requires automated verification for every adopt-now candidate. |
| Multi-Targeting and Compatibility | Pass | All implementation work remains subject to net8.0/net9.0/net10.0 compatibility checks. |
| One-Line Developer Experience | Pass | Any adopted runtime feature must preserve existing DI ergonomics. |
| Observability and Diagnostics | Pass | Documentation and migration artifacts must retain traceability and operational clarity. |
| Versioning and Release Discipline | Pass | Breaking-impact changes require migration guidance and CI-driven release workflow. |
| Decorator Pattern Architecture | Pass | Any runtime adoption must fit the existing decorator chain or remain outside runtime pipeline. |

**Gate Result**: PASS. No constitutional violation is required to proceed.

## Project Structure

### Documentation (this feature)

```text
specs/001-harvest-httpclient-ideas/
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── contracts/
│   ├── decision-record-contract.md
│   └── sunset-readiness-contract.md
└── tasks.md
```

### Source Code (repository root)

```text
WebSpark.HttpClientUtility/
├── ServiceCollectionExtensions.cs
├── RequestResult/
├── Authentication/
├── BatchExecution/
├── Concurrent/
├── CurlService/
├── OpenTelemetry/
└── StringConverter/

WebSpark.HttpClientUtility.Crawler/
├── ServiceCollectionExtensions.cs
├── SiteCrawler.cs
├── SimpleSiteCrawler.cs
└── README-Crawler.md

WebSpark.HttpClientUtility.Test/
├── ServiceCollectionExtensionsTests.cs
├── RequestResult/
├── BatchExecution/
└── OpenTelemetry/

WebSpark.HttpClientUtility.Crawler.Test/
├── CrawlerUtilityTests.cs
└── ...

docs/
documentation/
README.md
CHANGELOG.md
.github/workflows/
```

**Structure Decision**: Use the existing multi-project library structure. Planning artifacts live under the feature spec directory; adopted implementation work will land in the core library, crawler library, tests, and user-facing documentation depending on each candidate decision.

## Phase 0: Research Summary

- Sunset strategy: Use a harvest-first, communicate, then archive flow; archive only after all adopt-now work is complete and communicated.
- Package boundary strategy: Keep work in existing packages and docs by default; allow a new package only for a distinct consumer need with clear standalone release value.
- Adoption evidence strategy: Require automated verification plus user-facing documentation for every adopted candidate, with explicit migration guidance for breaking-impact changes.

## Phase 1: Design Summary

- `research.md` records the planning decisions and alternatives.
- `data-model.md` defines harvest candidates, decision records, adoption work items, and sunset readiness tracking.
- `contracts/` defines the required structure for decision records and sunset readiness artifacts.
- `quickstart.md` documents the maintainer workflow from inventory through legacy repo archival.

## Post-Design Constitution Check

| Principle | Status | Notes |
|-----------|--------|-------|
| Library-First Design | Pass | Data model and contracts keep package expansion exceptional rather than default. |
| Test Coverage and Quality | Pass | Dual-evidence gates are now explicit in research, contracts, and quickstart. |
| Multi-Targeting and Compatibility | Pass | Design assumes all adopted runtime work will validate on all target frameworks. |
| One-Line Developer Experience | Pass | No design artifact introduces consumer-facing setup complexity by default. |
| Observability and Diagnostics | Pass | Traceability and migration communication are explicit deliverables. |
| Versioning and Release Discipline | Pass | Breaking changes and multi-package decisions remain bound to migration guidance and CI release rules. |
| Decorator Pattern Architecture | Pass | Design preserves the existing runtime architecture as a review gate for any adopted item. |

**Post-Design Gate Result**: PASS. No constitution issues introduced by Phase 1 design.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| None | N/A | N/A |

