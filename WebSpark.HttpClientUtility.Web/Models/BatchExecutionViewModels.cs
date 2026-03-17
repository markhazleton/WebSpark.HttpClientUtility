using WebSpark.HttpClientUtility.BatchExecution;

namespace WebSpark.HttpClientUtility.Web.Models;

public enum DemoRunState
{
    Queued,
    Running,
    Completed,
    Cancelled,
    Failed
}

public sealed class BatchExecutionPageViewModel
{
    public DemoStartRunRequest SampleRequest { get; set; } = DemoStartRunRequest.CreateDefault();
    public int DemoMaxPlannedRequests { get; set; } = 50;
}

public sealed class DemoStartRunRequest
{
    public List<BatchEnvironment> Environments { get; set; } = [];
    public List<BatchUserContext> Users { get; set; } = [];
    public List<BatchRequestDefinition> Requests { get; set; } = [];
    public int Iterations { get; set; } = 1;
    public int MaxConcurrency { get; set; } = 4;

    public static DemoStartRunRequest CreateDefault()
    {
        return new DemoStartRunRequest
        {
            Environments =
            [
                new BatchEnvironment { Name = "Local", BaseUrl = "https://localhost:5001" },
                new BatchEnvironment { Name = "Staging", BaseUrl = "https://staging.example.com" }
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
                new BatchRequestDefinition { Name = "GetProfile", Method = "GET", PathTemplate = "/api/users/{userId}" }
            ],
            Iterations = 2,
            MaxConcurrency = 4
        };
    }
}

public sealed class DemoRunAcceptedResponse
{
    public string RunId { get; set; } = string.Empty;
    public int TotalPlannedCount { get; set; }
    public string Status { get; set; } = DemoRunState.Queued.ToString();
}

public sealed class DemoRunStatusResponse
{
    public string RunId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int CompletedCount { get; set; }
    public int TotalPlannedCount { get; set; }
    public BatchExecutionStatistics Statistics { get; set; } = new();
    public IReadOnlyList<BatchExecutionItemResult>? Results { get; set; }
}
