using Microsoft.Extensions.DependencyInjection;
using WebSpark.HttpClientUtility.BatchExecution;
using WebSpark.HttpClientUtility.RequestResult;

namespace WebSpark.HttpClientUtility.Test.BatchExecution;

[TestClass]
public class BatchExecutionPipelineIntegrationTests
{
    [TestMethod]
    public async Task BatchExecution_ResolvesFromDiAndExecutesThroughRequestPipeline()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHttpClientUtility(options =>
        {
            options.EnableBatchExecution = true;
            options.EnableCaching = true;
            options.EnableResilience = true;
            options.EnableTelemetry = true;
        });

        using var provider = services.BuildServiceProvider();
        var batchService = provider.GetRequiredService<IBatchExecutionService>();

        // We only validate composition here; requests are intentionally zero-work to avoid network dependency.
        var result = await batchService.ExecuteAsync(new BatchExecutionConfiguration
        {
            Environments = [new BatchEnvironment { Name = "A", BaseUrl = "https://example.test" }],
            Users = [],
            Requests = [new BatchRequestDefinition { Name = "Get", Method = "GET", PathTemplate = "/api" }],
            Iterations = 1,
            MaxConcurrency = 1
        });

        Assert.AreEqual(0, result.TotalPlannedCount);
        Assert.AreEqual(0, result.CompletedCount);
        Assert.IsNotNull(provider.GetRequiredService<IHttpRequestResultService>());
    }
}
