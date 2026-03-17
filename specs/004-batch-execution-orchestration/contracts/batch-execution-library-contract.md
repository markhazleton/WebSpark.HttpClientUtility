# Batch Execution Library Contract

**Package**: WebSpark.HttpClientUtility  
**Feature Area**: BatchExecution  
**Registration Model**: Opt-in via `HttpClientUtilityOptions.EnableBatchExecution`

---

## DI Contract

```csharp
public sealed class HttpClientUtilityOptions
{
    public bool EnableCaching { get; set; }
    public bool EnableResilience { get; set; }
    public bool EnableTelemetry { get; set; }
    public bool EnableBatchExecution { get; set; }
    public HttpRequestResultPollyOptions ResilienceOptions { get; set; }
    public bool UseNewtonsoftJson { get; set; }
}
```

When `EnableBatchExecution` is `true`, the library registers:
- `IBatchExecutionService`
- `ITemplateSubstitutionService`
- internal statistics helpers required by the orchestrator

When `EnableBatchExecution` is `false`, none of the above services are registered.

---

## Public Service Contract

```csharp
namespace WebSpark.HttpClientUtility.BatchExecution;

public interface IBatchExecutionService
{
    Task<BatchExecutionResult> ExecuteAsync(
        BatchExecutionConfiguration configuration,
        IBatchExecutionResultSink? resultSink = null,
        IProgress<BatchProgress>? progress = null,
        CancellationToken ct = default);
}
```

### Behavioral Rules

- The orchestrator must expand all combinations of environments, users, requests, and iterations.
- Each expanded request must be executed through `IHttpRequestResultService`.
- Concurrency must be capped with semaphore-style throttling.
- Cancellation must stop dispatching new work while allowing in-flight requests to finish.
- The result sink and progress callback must be invoked incrementally as each work item completes.

---

## Supporting Contracts

```csharp
public interface IBatchExecutionResultSink
{
    Task OnResultAsync(BatchExecutionItemResult result, CancellationToken ct = default);
}

public interface ITemplateSubstitutionService
{
    string Render(string template, BatchUserContext? userContext);
}
```

### Template Rendering Rules

- Replace `{key}` tokens from `BatchUserContext.Properties`.
- Replace `{{encoded_user_name}}` from `BatchUserContext.UserId`.
- Preserve unresolved placeholders exactly as written.
- Apply rendering independently to path and body templates.

---

## Data Contracts

```csharp
public sealed class BatchExecutionConfiguration
{
    public string? RunId { get; set; }
    public IReadOnlyList<BatchEnvironment> Environments { get; set; } = [];
    public IReadOnlyList<BatchUserContext> Users { get; set; } = [];
    public IReadOnlyList<BatchRequestDefinition> Requests { get; set; } = [];
    public int Iterations { get; set; } = 1;
    public int MaxConcurrency { get; set; } = 10;
}

public sealed class BatchEnvironment
{
    public string Name { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, string> DefaultHeaders { get; set; } =
        new Dictionary<string, string>();
}

public sealed class BatchUserContext
{
    public string UserId { get; set; } = string.Empty;
    public IReadOnlyDictionary<string, string> Properties { get; set; } =
        new Dictionary<string, string>();
}

public sealed class BatchRequestDefinition
{
    public string Name { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
    public string PathTemplate { get; set; } = string.Empty;
    public string? BodyTemplate { get; set; }
    public IReadOnlyDictionary<string, string> Headers { get; set; } =
        new Dictionary<string, string>();
    public string? ContentType { get; set; }
}
```

```csharp
public sealed class BatchExecutionItemResult
{
    public string RunId { get; set; } = string.Empty;
    public string EnvironmentName { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string RequestName { get; set; } = string.Empty;
    public int Iteration { get; set; }
    public string HttpMethod { get; set; } = string.Empty;
    public string RequestPath { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public int? StatusCode { get; set; }
    public string? ResponseBodyHash { get; set; }
    public long DurationMilliseconds { get; set; }
    public DateTimeOffset TimestampUtc { get; set; }
    public IReadOnlyList<string> ErrorMessages { get; set; } = [];
}

public sealed class BatchExecutionStatistics
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public double P50Milliseconds { get; set; }
    public double P95Milliseconds { get; set; }
    public double P99Milliseconds { get; set; }
    public IReadOnlyDictionary<string, int> ByMethod { get; set; } =
        new Dictionary<string, int>();
    public IReadOnlyDictionary<string, int> ByEnvironment { get; set; } =
        new Dictionary<string, int>();
    public IReadOnlyDictionary<string, int> ByUser { get; set; } =
        new Dictionary<string, int>();
    public IReadOnlyDictionary<int, int> ByStatusCode { get; set; } =
        new Dictionary<int, int>();
}

public sealed class BatchProgress
{
    public string RunId { get; set; } = string.Empty;
    public int CompletedCount { get; set; }
    public int TotalPlannedCount { get; set; }
    public BatchExecutionStatistics StatisticsSnapshot { get; set; } = new();
}

public sealed class BatchExecutionResult
{
    public string RunId { get; set; } = string.Empty;
    public int TotalPlannedCount { get; set; }
    public int CompletedCount { get; set; }
    public bool WasCancelled { get; set; }
    public BatchExecutionStatistics Statistics { get; set; } = new();
    public IReadOnlyList<BatchExecutionItemResult> Results { get; set; } = [];
}
```

---

## Integration Contract with Existing Request Pipeline

- The orchestrator must create a per-item `HttpRequestResult<T>` request model and send it through `IHttpRequestResultService`.
- Existing resilience, caching, and telemetry behavior must remain unchanged and apply per item when enabled.
- Correlation IDs remain per request; the orchestrator groups items by `RunId` in its own result model.
- Methods with bodies attach templated content only for body-capable methods and explicit custom-method scenarios.