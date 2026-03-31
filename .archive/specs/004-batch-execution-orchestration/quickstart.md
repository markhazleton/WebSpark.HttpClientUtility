# Quickstart: Batch Execution Orchestration

**Audience**: Maintainers and consumers validating the new batch execution feature

---

## Overview

Batch execution orchestration lets a consumer define environments, user contexts, and request templates once, then execute the full cartesian expansion through the existing `IHttpRequestResultService` pipeline.

This quickstart covers:
- enabling the feature in DI
- creating a minimal batch request
- wiring incremental progress and result streaming
- manually validating the demo page

---

## 1. Enable the feature

```csharp
using WebSpark.HttpClientUtility;

services.AddHttpClientUtility(options =>
{
    options.EnableResilience = true;
    options.EnableTelemetry = true;
    options.EnableBatchExecution = true;
});
```

---

## 2. Define a batch configuration

```csharp
using WebSpark.HttpClientUtility.BatchExecution;

var configuration = new BatchExecutionConfiguration
{
    Environments =
    [
        new BatchEnvironment
        {
            Name = "Local",
            BaseUrl = "https://localhost:5001"
        },
        new BatchEnvironment
        {
            Name = "Staging",
            BaseUrl = "https://staging.example.com"
        }
    ],
    Users =
    [
        new BatchUserContext
        {
            UserId = "john.doe",
            Properties = new Dictionary<string, string>
            {
                ["userId"] = "42",
                ["firstName"] = "John",
                ["lastName"] = "Doe"
            }
        }
    ],
    Requests =
    [
        new BatchRequestDefinition
        {
            Name = "GetProfile",
            Method = "GET",
            PathTemplate = "/api/users/{userId}"
        },
        new BatchRequestDefinition
        {
            Name = "UpdateProfile",
            Method = "PATCH",
            PathTemplate = "/api/users/{userId}",
            BodyTemplate = "{ \"firstName\": \"{firstName}\", \"lastName\": \"{lastName}\" }",
            ContentType = "application/json"
        }
    ],
    Iterations = 2,
    MaxConcurrency = 4
};
```

---

## 3. Execute with progress and a result sink

```csharp
public sealed class ConsoleSink : IBatchExecutionResultSink
{
    public Task OnResultAsync(BatchExecutionItemResult result, CancellationToken ct = default)
    {
        Console.WriteLine($"[{result.EnvironmentName}] {result.HttpMethod} {result.RequestPath} -> {result.StatusCode}");
        return Task.CompletedTask;
    }
}

var batchService = serviceProvider.GetRequiredService<IBatchExecutionService>();
var sink = new ConsoleSink();

var progress = new Progress<BatchProgress>(update =>
{
    Console.WriteLine($"{update.CompletedCount}/{update.TotalPlannedCount} complete");
});

var result = await batchService.ExecuteAsync(configuration, sink, progress, cancellationToken);

Console.WriteLine($"Success: {result.Statistics.SuccessCount}");
Console.WriteLine($"Failures: {result.Statistics.FailureCount}");
Console.WriteLine($"P95: {result.Statistics.P95Milliseconds}ms");
```

---

## 4. Expected behavior checklist

- `{key}` tokens in paths and bodies are replaced from `BatchUserContext.Properties`.
- `{{encoded_user_name}}` resolves from `BatchUserContext.UserId`.
- Missing placeholders remain unchanged.
- Each request executes through the existing resilience and telemetry pipeline.
- The result sink fires as each request completes.
- Progress updates reflect running statistics snapshots.

---

## 5. Validate the demo page

Run the web app:

```powershell
dotnet run --project WebSpark.HttpClientUtility.Web
```

Then verify:
- navigate to `/BatchExecution`
- the page shows pre-populated environments, users, requests, iterations, and concurrency controls
- submitting the form starts a capped run of about 50 planned requests or fewer
- statistics update before the run completes
- final results show counts, percentiles, and per-environment breakdowns

---

## 6. Validation commands

```powershell
dotnet build WebSpark.HttpClientUtility.sln
dotnet test WebSpark.HttpClientUtility.Test/WebSpark.HttpClientUtility.Test.csproj --configuration Release
```

Add focused tests for:
- template substitution edge cases
- cartesian expansion counts
- concurrency throttling
- cancellation handling
- percentile calculations
- response hashing determinism