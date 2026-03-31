# Harvest Summary: HttpClientDecoratorPattern

This summary records the outcomes of harvesting ideas from the legacy `HttpClientDecoratorPattern` repository into `WebSpark.HttpClientUtility`.

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