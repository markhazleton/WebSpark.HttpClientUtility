using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using WebSpark.HttpClientUtility.BatchExecution;

namespace WebSpark.HttpClientUtility.Test.BatchExecution;

[TestClass]
public class ServiceCollectionExtensionsBatchExecutionTests
{
    [TestMethod]
    public void AddHttpClientUtility_WhenBatchExecutionDisabled_DoesNotRegisterBatchServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddHttpClientUtility();
        using var provider = services.BuildServiceProvider();

        Assert.IsNull(provider.GetService<IBatchExecutionService>());
        Assert.IsNull(provider.GetService<ITemplateSubstitutionService>());
    }

    [TestMethod]
    public void AddHttpClientUtility_WhenBatchExecutionEnabled_RegistersBatchServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddHttpClientUtility(options =>
        {
            options.EnableBatchExecution = true;
        });

        using var provider = services.BuildServiceProvider();

        Assert.IsNotNull(provider.GetService<IBatchExecutionService>());
        Assert.IsNotNull(provider.GetService<ITemplateSubstitutionService>());
    }

    [TestMethod]
    public async Task BatchExecutionService_WithNegativeIterations_ThrowsArgumentOutOfRangeException()
    {
        var service = CreateService();

        var config = new BatchExecutionConfiguration
        {
            Environments = [new BatchEnvironment { Name = "Local", BaseUrl = "https://example.test" }],
            Users = [new BatchUserContext { UserId = "u1", Properties = new Dictionary<string, string>() }],
            Requests = [new BatchRequestDefinition { Name = "Get", Method = "GET", PathTemplate = "/api" }],
            Iterations = -1,
            MaxConcurrency = 1
        };

        await AssertThrowsAsync<ArgumentOutOfRangeException>(() => service.ExecuteAsync(config));
    }

    [TestMethod]
    public async Task BatchExecutionService_WithInvalidMaxConcurrency_ThrowsArgumentOutOfRangeException()
    {
        var service = CreateService();

        var config = new BatchExecutionConfiguration
        {
            Environments = [new BatchEnvironment { Name = "Local", BaseUrl = "https://example.test" }],
            Users = [new BatchUserContext { UserId = "u1", Properties = new Dictionary<string, string>() }],
            Requests = [new BatchRequestDefinition { Name = "Get", Method = "GET", PathTemplate = "/api" }],
            Iterations = 1,
            MaxConcurrency = 0
        };

        await AssertThrowsAsync<ArgumentOutOfRangeException>(() => service.ExecuteAsync(config));
    }

    [TestMethod]
    public async Task BatchExecutionService_WithMalformedBaseUrl_ThrowsArgumentException()
    {
        var service = CreateService();

        var config = new BatchExecutionConfiguration
        {
            Environments = [new BatchEnvironment { Name = "Broken", BaseUrl = "not-a-url" }],
            Users = [new BatchUserContext { UserId = "u1", Properties = new Dictionary<string, string>() }],
            Requests = [new BatchRequestDefinition { Name = "Get", Method = "GET", PathTemplate = "/api" }],
            Iterations = 1,
            MaxConcurrency = 1
        };

        await AssertThrowsAsync<ArgumentException>(() => service.ExecuteAsync(config));
    }

    [TestMethod]
    public async Task BatchExecutionService_WithEmptyCollections_ReturnsZeroPlannedWork()
    {
        var service = CreateService();

        var config = new BatchExecutionConfiguration
        {
            Environments = [],
            Users = [],
            Requests = [],
            Iterations = 1,
            MaxConcurrency = 1
        };

        var result = await service.ExecuteAsync(config);

        Assert.AreEqual(0, result.TotalPlannedCount);
        Assert.AreEqual(0, result.CompletedCount);
    }

    private static IBatchExecutionService CreateService()
    {
        var requestService = new Mock<WebSpark.HttpClientUtility.RequestResult.IHttpRequestResultService>(MockBehavior.Strict);
        var logger = Mock.Of<ILogger<BatchExecutionService>>();
        return new BatchExecutionService(requestService.Object, new TemplateSubstitutionService(), logger);
    }

    private static async Task AssertThrowsAsync<TException>(Func<Task> action)
        where TException : Exception
    {
        try
        {
            await action();
            Assert.Fail($"Expected exception of type {typeof(TException).Name} was not thrown.");
        }
        catch (TException)
        {
            // Expected.
        }
    }
}
