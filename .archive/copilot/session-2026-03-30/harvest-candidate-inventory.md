# Harvest Candidate Inventory

Date: 2026-03-30
Source: HttpClientDecoratorPattern

| candidateId | title | sourceReference | candidateType | noveltyAssessment | targetArea | decisionState |
|---|---|---|---|---|---|---|
| HC-001 | Seconds-based resilience configuration aliases | HttpClientDecorator.Web/appsettings.json | runtime feature | partially present | core package | adopt-now |
| HC-002 | Circuit-breaker and concurrent-calls scenario documentation | HttpClientDecorator.Web/Pages/CircuitBreaker.cshtml + README.md | docs/example | not present | documentation/examples | adopt-now |
| HC-003 | Crawler progress and site-analysis scenario documentation | HttpClientDecorator.Web/Pages/CrawlDomain.cshtml | docs/example | not present | documentation/examples | adopt-now |
| HC-004 | Legacy manual decorator composition explanation | README.md + Program.cs | architecture guidance | already present | documentation/examples | docs-only |
| HC-005 | Legacy package setup and install narrative cleanup | README.md | docs/example | partially present | documentation/examples | adopt-now |
| HC-006 | Bootswatch-heavy presentation patterns from legacy demo | README.md + Razor pages | UI/demo style | already present | demo app/docs | defer |
| HC-007 | Duplicate web-only startup patterns in legacy repository | Program.cs | process/architecture | already present | release process | reject |

## Notes

- HC-001 is pre-vetted in feature research and drives T012-T015.
- HC-002/HC-003 support T016-T018 and user-facing examples.
- HC-005 captures cross-doc consistency in README and GettingStarted updates.
- HC-006 and HC-007 remain traceable but intentionally not adopted as runtime/package changes.