# Adoption Work Items

Date: 2026-03-30

| workItemId | candidateId | implementationTargets | verificationEvidence | documentationEvidence | releaseStatus |
|---|---|---|---|---|---|
| AW-001 | HC-001 | WebSpark.HttpClientUtility/ServiceCollectionExtensions.cs; WebSpark.HttpClientUtility.Test/ServiceCollectionExtensionsTests.cs; WebSpark.HttpClientUtility.Test/RequestResult/HttpRequestResultTests.cs; WebSpark.HttpClientUtility/RequestResult/HttpRequestResultPollyOptions.cs | dotnet test WebSpark.HttpClientUtility.Test/WebSpark.HttpClientUtility.Test.csproj --configuration Release --no-restore (720/720 passed) | README.md resilience config section; documentation/GettingStarted.md resilience config section | release-ready |
| AW-002 | HC-002 | src/pages/examples/httpclient-decorator-pattern-scenarios.md; src/pages/examples.md | Docs source validation via src/pages/examples.md + harvested scenario page review | src/pages/examples/httpclient-decorator-pattern-scenarios.md and index link in src/pages/examples.md | release-ready |
| AW-003 | HC-003 | src/pages/examples/crawler-harvested-scenarios.md; src/pages/examples.md | Docs source validation via src/pages/examples.md + harvested scenario page review | src/pages/examples/crawler-harvested-scenarios.md and index link in src/pages/examples.md | release-ready |
| AW-004 | HC-005 | README.md; documentation/GettingStarted.md; CHANGELOG.md; copilot/session-2026-03-30/final-harvest-summary.md | dotnet test WebSpark.HttpClientUtility.Test/WebSpark.HttpClientUtility.Test.csproj --configuration Release --no-restore (720/720 passed) | README.md, documentation/GettingStarted.md, CHANGELOG.md, final-harvest-summary.md | release-ready |

## Release-Ready Rule

A work item can transition to release-ready only when:
- Automated verification evidence is linked.
- User-facing documentation evidence is linked.
- Migration guidance is linked if impact is breaking.