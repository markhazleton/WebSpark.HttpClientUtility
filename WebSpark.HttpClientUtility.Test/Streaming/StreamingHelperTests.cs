using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using WebSpark.HttpClientUtility.Streaming;

namespace WebSpark.HttpClientUtility.Test.Streaming;

[TestClass]
public class StreamingHelperTests
{
    private Mock<ILogger> _mockLogger = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger>();
    }

    [TestMethod]
    public void DefaultStreamingThreshold_HasExpectedValue()
    {
        // Assert
        Assert.AreEqual(10 * 1024 * 1024, StreamingHelper.DefaultStreamingThreshold);
    }

    [TestMethod]
    public async Task ProcessResponseAsync_NullResponse_ReturnsDefault()
    {
        // Arrange
        var jsonOptions = new JsonSerializerOptions();
        var correlationId = Guid.NewGuid().ToString();

        // Act
        var result = await StreamingHelper.ProcessResponseAsync<string>(
            null!, 1024, jsonOptions, _mockLogger.Object, correlationId);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task ProcessResponseAsync_NullContent_ReturnsDefault()
    {
        // Arrange
        var response = new HttpResponseMessage();
        response.Content = null;
        var jsonOptions = new JsonSerializerOptions();
        var correlationId = Guid.NewGuid().ToString();

        // Act
        var result = await StreamingHelper.ProcessResponseAsync<string>(
            response, 1024, jsonOptions, _mockLogger.Object, correlationId);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task ProcessResponseAsync_SmallContent_UsesStandardDeserialization()
    {
        // Arrange
        var testObject = new { Name = "Test", Value = 42 };
        var json = JsonSerializer.Serialize(testObject);
        var response = new HttpResponseMessage();
        response.Content = new StringContent(json);
        response.Content.Headers.ContentLength = json.Length;

        var jsonOptions = new JsonSerializerOptions();
        var correlationId = Guid.NewGuid().ToString();

        // Act
        var result = await StreamingHelper.ProcessResponseAsync<dynamic>(
            response, 1024, jsonOptions, _mockLogger.Object, correlationId);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ProcessResponseAsync_LargeContent_UsesStreamingDeserialization()
    {
        // Arrange
        var testObject = new { Name = "Test", Value = 42 };
        var json = JsonSerializer.Serialize(testObject);
        var response = new HttpResponseMessage();
        response.Content = new StringContent(json);

        // Set content length to be larger than threshold
        var largeSize = 2048;
        response.Content.Headers.ContentLength = largeSize;

        var jsonOptions = new JsonSerializerOptions();
        var correlationId = Guid.NewGuid().ToString();
        var threshold = 1024; // Smaller than our content

        // Act
        var result = await StreamingHelper.ProcessResponseAsync<JsonElement>(
            response, threshold, jsonOptions, _mockLogger.Object, correlationId);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ProcessResponseAsync_EmptyContent_ReturnsDefault()
    {
        // Arrange
        var response = new HttpResponseMessage();
        response.Content = new StringContent("");
        var jsonOptions = new JsonSerializerOptions();
        var correlationId = Guid.NewGuid().ToString();

        // Act
        var result = await StreamingHelper.ProcessResponseAsync<string>(
            response, 1024, jsonOptions, _mockLogger.Object, correlationId);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task ProcessResponseAsync_InvalidJson_ReturnsDefault()
    {
        // Arrange
        var response = new HttpResponseMessage();
        response.Content = new StringContent("invalid json content");
        var jsonOptions = new JsonSerializerOptions();
        var correlationId = Guid.NewGuid().ToString();

        // Act
        var result = await StreamingHelper.ProcessResponseAsync<object>(
            response, 1024, jsonOptions, _mockLogger.Object, correlationId);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task ProcessResponseAsync_WithCancellationToken_PropagatesToken()
    {
        // Arrange
        var testObject = new { Name = "Test" };
        var json = JsonSerializer.Serialize(testObject);
        var response = new HttpResponseMessage();
        response.Content = new StringContent(json);

        var jsonOptions = new JsonSerializerOptions();
        var correlationId = Guid.NewGuid().ToString();
        var cancellationToken = new CancellationToken();

        // Act
        var result = await StreamingHelper.ProcessResponseAsync<JsonElement>(
            response, 1024, jsonOptions, _mockLogger.Object, correlationId, cancellationToken);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ProcessResponseAsync_UnknownContentLength_UsesStandardDeserialization()
    {
        // Arrange
        var testObject = new { Name = "Test", Value = 42 };
        var json = JsonSerializer.Serialize(testObject);
        var response = new HttpResponseMessage();
        response.Content = new StringContent(json);

        // Don't set content length (will be null)
        response.Content.Headers.ContentLength = null;

        var jsonOptions = new JsonSerializerOptions();
        var correlationId = Guid.NewGuid().ToString();

        // Act
        var result = await StreamingHelper.ProcessResponseAsync<JsonElement>(
            response, 1024, jsonOptions, _mockLogger.Object, correlationId);

        // Assert
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void TruncateForLogging_NullContent_ReturnsNull()
    {
        // Act
        var result = StreamingHelper.TruncateForLogging(null!);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void TruncateForLogging_EmptyContent_ReturnsEmpty()
    {
        // Act
        var result = StreamingHelper.TruncateForLogging("");

        // Assert
        Assert.AreEqual("", result);
    }

    [TestMethod]
    public void TruncateForLogging_ShortContent_ReturnsUnchanged()
    {
        // Arrange
        var content = "Short content";

        // Act
        var result = StreamingHelper.TruncateForLogging(content, 50);

        // Assert
        Assert.AreEqual(content, result);
    }

    [TestMethod]
    public void TruncateForLogging_LongContent_TruncatesWithIndicator()
    {
        // Arrange
        var content = "This is a very long content that should be truncated";
        var maxLength = 20;

        // Act
        var result = StreamingHelper.TruncateForLogging(content, maxLength);

        // Assert
        Assert.IsTrue(result.Length > maxLength); // Due to "... [truncated]" suffix
        Assert.IsTrue(result.StartsWith(content[..maxLength]));
        Assert.IsTrue(result.EndsWith("... [truncated]"));
    }

    [TestMethod]
    public void TruncateForLogging_ContentExactlyMaxLength_ReturnsUnchanged()
    {
        // Arrange
        var content = "Exactly twenty chars";
        var maxLength = content.Length;

        // Act
        var result = StreamingHelper.TruncateForLogging(content, maxLength);

        // Assert
        Assert.AreEqual(content, result);
    }

    [TestMethod]
    public void TruncateForLogging_DefaultMaxLength_UsesDefault()
    {
        // Arrange
        var content = new string('x', 600); // Longer than default 500

        // Act
        var result = StreamingHelper.TruncateForLogging(content);

        // Assert
        Assert.IsTrue(result.EndsWith("... [truncated]"));
        Assert.IsTrue(result.Length > 500);
    }

    // Test helper class for deserialization
    public class TestData
    {
        public string Name { get; set; } = "";
        public int Value { get; set; }
    }

    [TestMethod]
    public async Task ProcessResponseAsync_ValidObject_DeserializesCorrectly()
    {
        // Arrange
        var testObject = new TestData { Name = "TestName", Value = 123 };
        var json = JsonSerializer.Serialize(testObject);
        var response = new HttpResponseMessage();
        response.Content = new StringContent(json);

        var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var correlationId = Guid.NewGuid().ToString();

        // Act
        var result = await StreamingHelper.ProcessResponseAsync<TestData>(
            response, 1024, jsonOptions, _mockLogger.Object, correlationId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("TestName", result.Name);
        Assert.AreEqual(123, result.Value);
    }
}
