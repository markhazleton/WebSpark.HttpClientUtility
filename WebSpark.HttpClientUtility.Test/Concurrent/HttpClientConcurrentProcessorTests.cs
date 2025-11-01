using Moq;
using WebSpark.HttpClientUtility.Concurrent;
using WebSpark.HttpClientUtility.RequestResult;

namespace WebSpark.HttpClientUtility.Test.Concurrent;

[TestClass]
public class HttpClientConcurrentProcessorTests
{
    private Mock<IHttpRequestResultService> _mockHttpService;
    private HttpClientConcurrentProcessor _processor;

    [TestInitialize]
    public void Setup()
    {
        _mockHttpService = new Mock<IHttpRequestResultService>();

        // Setup a factory function for testing
        Func<int, HttpClientConcurrentModel> taskDataFactory = (taskId) =>
        new HttpClientConcurrentModel(taskId, "https://example.com/test");

        _processor = new HttpClientConcurrentProcessor(taskDataFactory, _mockHttpService.Object);
    }

    [TestMethod]
    public void Constructor_WithValidParameters_SetsProperties()
    {
        // Arrange
        Func<int, HttpClientConcurrentModel> factory = (id) => new HttpClientConcurrentModel(id, "test");
        var service = new Mock<IHttpRequestResultService>().Object;

        // Act
        var processor = new HttpClientConcurrentProcessor(factory, service);

        // Assert
        Assert.IsNotNull(processor);
    }

    [TestMethod]
    public void Constructor_WithNullService_ThrowsArgumentNullException()
    {
        // Arrange
        Func<int, HttpClientConcurrentModel> factory = (id) => new HttpClientConcurrentModel(id, "test");

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
      new HttpClientConcurrentProcessor(factory, null!));
    }

    [TestMethod]
    public async Task RunAsync_WithValidParameters_ProcessesTasksSuccessfully()
    {
        // Arrange
        var mockResult = new HttpRequestResult<SiteStatus>
        {
            RequestPath = "https://example.com/test",
            StatusCode = System.Net.HttpStatusCode.OK,
            ResponseResults = new SiteStatus(
 DateTime.Now,
    new BuildVersion(),
  new Features(),
    Array.Empty<object>(),
   "Test Region",
    200,
           new Tests()
    )
        };

        _mockHttpService
       .Setup(s => s.HttpSendRequestResultAsync(
      It.IsAny<HttpRequestResult<SiteStatus>>(),
  It.IsAny<string>(),
   It.IsAny<string>(),
    It.IsAny<int>(),
    It.IsAny<CancellationToken>()))
  .ReturnsAsync(mockResult);

        // Act
        var results = await _processor.RunAsync(maxTaskCount: 3, maxConcurrency: 2);

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(3, results.Count);

        foreach (var result in results)
        {
            Assert.IsNotNull(result);
            Assert.IsTrue(result.TaskId >= 1 && result.TaskId <= 3);
            Assert.AreEqual("https://example.com/test", result.StatusCall.RequestPath);
            Assert.AreEqual(System.Net.HttpStatusCode.OK, result.StatusCall.StatusCode);
            Assert.IsTrue(result.DurationMS >= 0);
        }

        // Verify the service was called for each task
        _mockHttpService.Verify(
 s => s.HttpSendRequestResultAsync(
       It.IsAny<HttpRequestResult<SiteStatus>>(),
      It.IsAny<string>(),
    It.IsAny<string>(),
    It.IsAny<int>(),
    It.IsAny<CancellationToken>()),
   Times.Exactly(3));
    }

    [TestMethod]
    public async Task RunAsync_WithCancellation_StopsProcessing()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        _mockHttpService
    .Setup(s => s.HttpSendRequestResultAsync(
     It.IsAny<HttpRequestResult<SiteStatus>>(),
      It.IsAny<string>(),
         It.IsAny<string>(),
      It.IsAny<int>(),
       It.IsAny<CancellationToken>()))
      .ThrowsAsync(new OperationCanceledException());

        // Act & Assert
        try
        {
            await _processor.RunAsync(maxTaskCount: 5, maxConcurrency: 1, cancellationTokenSource.Token);
            Assert.Fail("Expected OperationCanceledException was not thrown");
        }
        catch (OperationCanceledException)
        {
            // Expected exception - test passes
        }
    }

    [TestMethod]
    public async Task RunAsync_WithHttpServiceException_PropagatesException()
    {
        // Arrange
        var expectedException = new HttpRequestException("Network error");

        _mockHttpService
               .Setup(s => s.HttpSendRequestResultAsync(
              It.IsAny<HttpRequestResult<SiteStatus>>(),
         It.IsAny<string>(),
         It.IsAny<string>(),
                  It.IsAny<int>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act & Assert
        try
        {
            await _processor.RunAsync(maxTaskCount: 1, maxConcurrency: 1);
            Assert.Fail("Expected HttpRequestException was not thrown");
        }
        catch (HttpRequestException ex)
        {
            Assert.AreEqual("Network error", ex.Message);
        }
    }

    [TestMethod]
    public async Task RunAsync_WithZeroTaskCount_ReturnsEmptyResults()
    {
        // Act
        var results = await _processor.RunAsync(maxTaskCount: 0, maxConcurrency: 1);

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(0, results.Count);

        // Verify no service calls were made
        _mockHttpService.Verify(
     s => s.HttpSendRequestResultAsync(
It.IsAny<HttpRequestResult<SiteStatus>>(),
    It.IsAny<string>(),
   It.IsAny<string>(),
It.IsAny<int>(),
 It.IsAny<CancellationToken>()),
    Times.Never);
    }

    [TestMethod]
    public async Task RunAsync_WithSingleTask_ProcessesSequentially()
    {
        // Arrange
        var mockResult = new HttpRequestResult<SiteStatus>
        {
            RequestPath = "https://example.com/test",
            StatusCode = System.Net.HttpStatusCode.OK,
            ResponseResults = new SiteStatus(
        DateTime.Now,
          new BuildVersion(),
          new Features(),
             Array.Empty<object>(),
               "Test Region",
        200,
        new Tests()
           )
        };

        _mockHttpService
             .Setup(s => s.HttpSendRequestResultAsync(
        It.IsAny<HttpRequestResult<SiteStatus>>(),
              It.IsAny<string>(),
       It.IsAny<string>(),
              It.IsAny<int>(),
          It.IsAny<CancellationToken>()))
     .ReturnsAsync(mockResult);

        // Act
        var results = await _processor.RunAsync(maxTaskCount: 1, maxConcurrency: 1);

        // Assert
        Assert.IsNotNull(results);
        Assert.AreEqual(1, results.Count);
        Assert.AreEqual(1, results[0].TaskId);

        _mockHttpService.Verify(
    s => s.HttpSendRequestResultAsync(
       It.IsAny<HttpRequestResult<SiteStatus>>(),
      It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<int>(),
     It.IsAny<CancellationToken>()),
        Times.Once);
    }
}
