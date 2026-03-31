# HttpClientDecoratorPattern Audit

Date: 2026-03-30
Source repository: C:\GitHub\MarkHazleton\HttpClientDecoratorPattern
Audited by: Copilot

## Scope

- README and architecture narrative
- Runtime configuration model in Program.cs and appsettings.json
- Demo Razor pages for resilience and crawler scenarios

## Key Findings

1. Legacy configuration uses seconds-based resilience keys:
- HttpRequestResultPollyOptions:RetryDelaySeconds
- HttpRequestResultPollyOptions:CircuitBreakerDurationSeconds

2. Legacy demo highlights two high-value scenario groups worth preserving in docs:
- Circuit breaker and concurrent calls operational behavior
- Crawler progress and site-analysis workflow with SignalR updates

3. Legacy repo uses manual decorator wiring in the web app startup, while primary package favors one-line DI registration.

4. Legacy repo mixes demonstration concerns and package usage guidance in README and Razor pages, which should be normalized into the primary docs site and package docs.

## Source References

- README.md
- HttpClientDecorator.Web/Program.cs
- HttpClientDecorator.Web/appsettings.json
- HttpClientDecorator.Web/Pages/CircuitBreaker.cshtml
- HttpClientDecorator.Web/Pages/CrawlDomain.cshtml

## Candidate Extraction Notes

- Candidate list and decision records are tracked in sibling files:
- harvest-candidate-inventory.md
- harvest-decision-records.md
- adoption-work-items.md

## Conclusion

The most valuable adopt-now items are configuration compatibility improvements and documentation/example migration. The legacy repository should remain active until adopt-now items are shipped and release evidence is complete.