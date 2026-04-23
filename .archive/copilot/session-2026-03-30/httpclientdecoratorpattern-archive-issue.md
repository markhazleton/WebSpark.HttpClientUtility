# Archive Announcement Draft: HttpClientDecoratorPattern

## Title

Archive legacy `HttpClientDecoratorPattern` repository after harvest completion

## Summary

All adopt-now harvest candidates have been implemented in `WebSpark.HttpClientUtility` and documented in the primary repository. This issue proposes archiving the legacy demo repository and directing users to the maintained packages and docs.

## Replacement Guidance

- Primary package: `WebSpark.HttpClientUtility`
- Crawler extension: `WebSpark.HttpClientUtility.Crawler`
- Main docs: `https://markhazleton.github.io/WebSpark.HttpClientUtility/`
- Harvested scenarios:
  - `/examples/httpclient-decorator-pattern-scenarios/`
  - `/examples/crawler-harvested-scenarios/`

## Migration Notes

- Existing TimeSpan resilience configuration keys remain supported.
- Legacy alias keys `RetryDelaySeconds` and `CircuitBreakerDurationSeconds` are now also supported in the primary package.
- No breaking API changes were introduced by the harvest adoption set.

## Archive Checklist

- [x] Adopt-now candidates implemented
- [x] Automated verification evidence linked
- [x] User-facing documentation evidence linked
- [x] Release/migration communication prepared
- [ ] Legacy repository README redirect merged
- [ ] Final archive approval recorded in sunset-readiness.md