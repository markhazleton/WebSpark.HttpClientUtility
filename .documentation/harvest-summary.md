# Harvest Summary: HttpClientDecoratorPattern

This summary records the outcomes of harvesting ideas from the legacy `HttpClientDecoratorPattern` repository into `WebSpark.HttpClientUtility`.

## Traceability (2026-03-31)

- Completed specification knowledge has been consolidated into living documentation and changelog traceability entries.
- Completed spec sets identified for archival:
  - `001-harvest-httpclient-ideas`
  - `003-split-nuget-packages`
  - `004-batch-execution-orchestration`
- Active draft spec sets intentionally retained in place:
  - `001-static-docs-site`
  - `002-clean-compiler-warnings`

## Adopted

- Added compatibility binding for legacy resilience aliases:
  - `RetryDelaySeconds`
  - `CircuitBreakerDurationSeconds`
- Added harvested docs scenarios:
  - HttpClient decorator resilience/concurrency scenarios
  - Crawler progress and site-analysis scenarios
- Updated user-facing documentation for resilience configuration options.

## Deferred or Rejected

- Deferred: legacy presentation-specific UI patterns that are not required for package capability.
- Rejected: manual startup composition patterns already superseded by one-line DI registration in the package.

## Consumer Guidance

No breaking API changes were introduced by the harvested adopt-now set. Existing TimeSpan-based resilience keys continue to work, and legacy seconds-based aliases are now also supported.

## Evidence

- Automated verification: `dotnet test WebSpark.HttpClientUtility.Test/WebSpark.HttpClientUtility.Test.csproj --configuration Release --no-restore`
- Docs updates: `README.md`, `documentation/GettingStarted.md`, `src/pages/examples.md`, harvested scenario pages