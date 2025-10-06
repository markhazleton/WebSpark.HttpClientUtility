using System.Net;
using WebSpark.HttpClientUtility;

namespace WebSpark.HttpClientUtility.Test.HttpResponse;

[TestClass]
public class HttpResponseContentTests
{
    [TestMethod]
    public void Success_WithContent_CreatesSuccessfulResponse()
    {
        // Arrange
        var content = "Test content";
        var statusCode = HttpStatusCode.OK;

        // Act
        var response = HttpResponseContent<string>.Success(content, statusCode);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsTrue(response.IsSuccess);
        Assert.AreEqual(content, response.Content);
        Assert.AreEqual(statusCode, response.StatusCode);
        Assert.IsNull(response.ErrorMessage);
        Assert.IsNotNull(response.CorrelationId);
    }

    [TestMethod]
    public void Failure_WithErrorMessage_CreatesFailureResponse()
    {
        // Arrange
        var errorMessage = "Request failed";
        var statusCode = HttpStatusCode.BadRequest;

        // Act
        var response = HttpResponseContent<string>.Failure(errorMessage, statusCode);

        // Assert
        Assert.IsNotNull(response);
        Assert.IsFalse(response.IsSuccess);
        Assert.IsNull(response.Content);
        Assert.AreEqual(statusCode, response.StatusCode);
        Assert.AreEqual(errorMessage, response.ErrorMessage);
        Assert.IsNotNull(response.CorrelationId);
    }

    [TestMethod]
    public void Success_WithNullContent_AllowsNullContent()
    {
        // Act
        var response = HttpResponseContent<string?>.Success(null, HttpStatusCode.OK);

        // Assert
        Assert.IsTrue(response.IsSuccess);
        Assert.IsNull(response.Content);
    }

    [TestMethod]
    public void WithCorrelationId_SetsCorrelationId()
    {
        // Arrange
        var response = HttpResponseContent<string>.Success("test", HttpStatusCode.OK);
        var correlationId = "test-correlation-id";

        // Act
        var result = response.WithCorrelationId(correlationId);

        // Assert
        Assert.AreEqual(correlationId, result.CorrelationId);
        Assert.AreSame(response, result); // Should return same instance
    }

    [TestMethod]
    public void WithElapsedTime_SetsElapsedTimeAndCompletionDate()
    {
        // Arrange
        var response = HttpResponseContent<string>.Success("test", HttpStatusCode.OK);
        var elapsedMs = 1500L;
        var beforeCall = DateTime.UtcNow;

        // Act
        var result = response.WithElapsedTime(elapsedMs);
        var afterCall = DateTime.UtcNow;

        // Assert
        Assert.AreEqual(elapsedMs, result.ElapsedMilliseconds);
        Assert.IsNotNull(result.CompletionDate);
        Assert.IsTrue(result.CompletionDate >= beforeCall);
        Assert.IsTrue(result.CompletionDate <= afterCall);
        Assert.AreSame(response, result);
    }

    [TestMethod]
    public void WithContextProperty_AddsPropertyToContext()
    {
        // Arrange
        var response = HttpResponseContent<string>.Success("test", HttpStatusCode.OK);
        var key = "testKey";
        var value = "testValue";

        // Act
        var result = response.WithContextProperty(key, value);

        // Assert
        Assert.IsTrue(result.RequestContext.ContainsKey(key));
        Assert.AreEqual(value, result.RequestContext[key]);
        Assert.AreSame(response, result);
    }

    [TestMethod]
    public void WithContextProperty_OverwritesExistingProperty()
    {
        // Arrange
        var response = HttpResponseContent<string>.Success("test", HttpStatusCode.OK);
        var key = "testKey";
        var value1 = "firstValue";
        var value2 = "secondValue";

        // Act
        response.WithContextProperty(key, value1);
        var result = response.WithContextProperty(key, value2);

        // Assert
        Assert.AreEqual(value2, result.RequestContext[key]);
    }

    [TestMethod]
    public void RequestContext_DefaultsToEmptyDictionary()
    {
        // Arrange & Act
        var response = HttpResponseContent<string>.Success("test", HttpStatusCode.OK);

        // Assert
        Assert.IsNotNull(response.RequestContext);
        Assert.AreEqual(0, response.RequestContext.Count);
    }

    [TestMethod]
    public void CacheDurationMinutes_DefaultsToOne()
    {
        // Arrange & Act
        var response = HttpResponseContent<string>.Success("test", HttpStatusCode.OK);

        // Assert
        Assert.AreEqual(1, response.CacheDurationMinutes);
    }

    [TestMethod]
    public void CacheDurationMinutes_CanBeSet()
    {
        // Arrange
        var response = HttpResponseContent<string>.Success("test", HttpStatusCode.OK);
        var duration = 30;

        // Act
        response.CacheDurationMinutes = duration;

        // Assert
        Assert.AreEqual(duration, response.CacheDurationMinutes);
    }

    [TestMethod]
    public void Retries_DefaultsToZero()
    {
        // Arrange & Act
        var response = HttpResponseContent<string>.Success("test", HttpStatusCode.OK);

        // Assert
        Assert.AreEqual(0, response.Retries);
    }

    [TestMethod]
    public void Retries_CanBeSet()
    {
        // Arrange
        var response = HttpResponseContent<string>.Success("test", HttpStatusCode.OK);
        var retries = 3;

        // Act
        response.Retries = retries;

        // Assert
        Assert.AreEqual(retries, response.Retries);
    }

    [TestMethod]
    public void ResultAge_WithNullCompletionDate_ReturnsAppropriateMessage()
    {
        // Arrange
        var response = HttpResponseContent<string>.Success("test", HttpStatusCode.OK);
        // CompletionDate is null by default

        // Act
        var resultAge = response.ResultAge;

        // Assert
        Assert.AreEqual("Result Cache date is null.", resultAge);
    }

    [TestMethod]
    public void ResultAge_WithCompletionDate_ReturnsFormattedAge()
    {
        // Arrange
        var response = HttpResponseContent<string>.Success("test", HttpStatusCode.OK);
        response.CompletionDate = DateTime.UtcNow.AddMinutes(-5).AddSeconds(-30);

        // Act
        var resultAge = response.ResultAge;

        // Assert
        Assert.IsTrue(resultAge.Contains("Result Cache Age:"));
        Assert.IsTrue(resultAge.Contains("minutes"));
        Assert.IsTrue(resultAge.Contains("seconds"));
    }

    [TestMethod]
    public void ResultAge_WithOldDate_HandlesCarryOver()
    {
        // Arrange
        var response = HttpResponseContent<string>.Success("test", HttpStatusCode.OK);
        response.CompletionDate = DateTime.UtcNow.AddDays(-2).AddHours(-25).AddMinutes(-65);

        // Act
        var resultAge = response.ResultAge;

        // Assert
        Assert.IsTrue(resultAge.Contains("Result Cache Age:"));
        Assert.IsTrue(resultAge.Contains("days"));
        Assert.IsTrue(resultAge.Contains("hours"));
        Assert.IsTrue(resultAge.Contains("minutes"));
    }

    [TestMethod]
    public void ToString_SuccessResponse_ReturnsSuccessFormat()
    {
        // Arrange
        var response = HttpResponseContent<string>.Success("test", HttpStatusCode.OK)
            .WithCorrelationId("test-id")
            .WithElapsedTime(1000);

        // Act
        var result = response.ToString();

        // Assert
        Assert.IsTrue(result.Contains("Success:"));
        Assert.IsTrue(result.Contains("OK"));
        Assert.IsTrue(result.Contains("String"));
        Assert.IsTrue(result.Contains("test-id"));
        Assert.IsTrue(result.Contains("1000ms"));
    }

    [TestMethod]
    public void ToString_FailureResponse_ReturnsFailureFormat()
    {
        // Arrange
        var response = HttpResponseContent<string>.Failure("Error occurred", HttpStatusCode.InternalServerError)
            .WithCorrelationId("test-id")
            .WithElapsedTime(500);

        // Act
        var result = response.ToString();

        // Assert
        Assert.IsTrue(result.Contains("Failure:"));
        Assert.IsTrue(result.Contains("InternalServerError"));
        Assert.IsTrue(result.Contains("Error occurred"));
        Assert.IsTrue(result.Contains("test-id"));
        Assert.IsTrue(result.Contains("500ms"));
    }

    [TestMethod]
    public void ChainedCalls_AllMethodsReturnSameInstance()
    {
        // Arrange
        var response = HttpResponseContent<string>.Success("test", HttpStatusCode.OK);

        // Act
        var result = response
            .WithCorrelationId("test-id")
            .WithElapsedTime(1000)
            .WithContextProperty("key", "value");

        // Assert
        Assert.AreSame(response, result);
        Assert.AreEqual("test-id", result.CorrelationId);
        Assert.AreEqual(1000, result.ElapsedMilliseconds);
        Assert.AreEqual("value", result.RequestContext["key"]);
    }

    [TestMethod]
    public void Success_WithComplexObject_WorksCorrectly()
    {
        // Arrange
        var complexObject = new { Name = "Test", Values = new[] { 1, 2, 3 } };

        // Act
        var response = HttpResponseContent<object>.Success(complexObject, HttpStatusCode.Created);

        // Assert
        Assert.IsTrue(response.IsSuccess);
        Assert.AreSame(complexObject, response.Content);
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }
}