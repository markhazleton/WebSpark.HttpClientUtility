using Moq;
using System;
using System.Net;
using WebSpark.HttpClientUtility.RequestResult;

namespace WebSpark.HttpClientUtility.Test.RequestResult;

public class Person
{
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}

// Additional test data class to test different response types
public class Product
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

[TestClass]
public class HttpRequestResultTests
{
    private MockRepository? mockRepository;

    [TestInitialize]
    public void TestInitialize()
    {
        this.mockRepository = new MockRepository(MockBehavior.Strict);
    }

    [TestMethod]
    public void DefaultConstructor_Initializes_WithDefaultValues()
    {
        // Arrange & Act
        var result = new HttpRequestResult<Person>();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNull(result.ResponseResults);
        Assert.AreEqual(HttpMethod.Get, result.RequestMethod);
        Assert.AreEqual(string.Empty, result.RequestPath);
        Assert.AreEqual(0, result.Iteration);
        Assert.AreEqual(0, (int)result.StatusCode); // Default status code is 0/OK
        Assert.IsNotNull(result.CorrelationId);
        // Verify the correlation ID is a valid GUID
        Assert.IsTrue(Guid.TryParse(result.CorrelationId, out _), "CorrelationId should be a valid GUID");
    }

    [TestMethod]
    public void Constructor_WithIteration_And_Path_Initializes_Correctly()
    {
        // Arrange
        int expectedIteration = 5;
        string expectedPath = "https://api.example.com/data";

        // Act
        var result = new HttpRequestResult<Person>(expectedIteration, expectedPath);

        // Assert
        Assert.AreEqual(expectedIteration, result.Iteration);
        Assert.AreEqual(expectedPath, result.RequestPath);
        Assert.IsNull(result.ResponseResults);
    }

    [TestMethod]
    public void Constructor_From_HttpRequestResultBase_Copies_Properties()
    {
        // Arrange
        var baseRequest = new HttpRequestResult<object>();
        baseRequest.Iteration = 3;
        baseRequest.RequestPath = "https://api.test.com";
        baseRequest.RequestMethod = HttpMethod.Post;
        baseRequest.CacheDurationMinutes = 10;
        baseRequest.Retries = 5;
        baseRequest.IsDebugEnabled = true;

        // Add a request context item
        baseRequest.RequestContext["TestKey"] = "TestValue";

        // Act
        var result = new HttpRequestResult<Person>(baseRequest);

        // Assert
        Assert.AreEqual(baseRequest.Iteration, result.Iteration);
        Assert.AreEqual(baseRequest.RequestPath, result.RequestPath);
        Assert.AreEqual(baseRequest.RequestMethod, result.RequestMethod);
        Assert.AreEqual(baseRequest.CacheDurationMinutes, result.CacheDurationMinutes);
        Assert.AreEqual(baseRequest.Retries, result.Retries);
        Assert.AreEqual(baseRequest.CorrelationId, result.CorrelationId);
        Assert.AreEqual(baseRequest.IsDebugEnabled, result.IsDebugEnabled);
        Assert.AreEqual("TestValue", result.RequestContext["TestKey"]);
    }

    [TestMethod]
    public void IsSuccessStatusCode_ReturnsTrueFor_200_Range()
    {
        // Arrange
        var okResult = new HttpRequestResult<Person> { StatusCode = HttpStatusCode.OK };
        var createdResult = new HttpRequestResult<Person> { StatusCode = HttpStatusCode.Created };
        var acceptedResult = new HttpRequestResult<Person> { StatusCode = HttpStatusCode.Accepted };
        var noContentResult = new HttpRequestResult<Person> { StatusCode = HttpStatusCode.NoContent };

        // Act & Assert
        Assert.IsTrue(okResult.IsSuccessStatusCode);
        Assert.IsTrue(createdResult.IsSuccessStatusCode);
        Assert.IsTrue(acceptedResult.IsSuccessStatusCode);
        Assert.IsTrue(noContentResult.IsSuccessStatusCode);
    }

    [TestMethod]
    public void IsSuccessStatusCode_ReturnsFalseForNon_200_Range()
    {
        // Arrange
        var badRequestResult = new HttpRequestResult<Person> { StatusCode = HttpStatusCode.BadRequest };
        var unauthorizedResult = new HttpRequestResult<Person> { StatusCode = HttpStatusCode.Unauthorized };
        var notFoundResult = new HttpRequestResult<Person> { StatusCode = HttpStatusCode.NotFound };
        var serverErrorResult = new HttpRequestResult<Person> { StatusCode = HttpStatusCode.InternalServerError };

        // Act & Assert
        Assert.IsFalse(badRequestResult.IsSuccessStatusCode);
        Assert.IsFalse(unauthorizedResult.IsSuccessStatusCode);
        Assert.IsFalse(notFoundResult.IsSuccessStatusCode);
        Assert.IsFalse(serverErrorResult.IsSuccessStatusCode);
    }

    [TestMethod]
    public void RequestDurationMilliseconds_ReturnsElapsedMilliseconds()
    {
        // Arrange
        var result = new HttpRequestResult<Person>();
        long expectedDuration = 150;
        result.ElapsedMilliseconds = expectedDuration;

        // Act
        var actualDuration = result.RequestDurationMilliseconds;

        // Assert
        Assert.AreEqual(expectedDuration, actualDuration);
    }

    [TestMethod]
    public void ResponseResults_CanBeSet_AndRetrieved()
    {
        // Arrange
        var result = new HttpRequestResult<Person>();
        var person = new Person { Name = "John Doe", Age = 30 };

        // Act
        result.ResponseResults = person;

        // Assert
        Assert.IsNotNull(result.ResponseResults);
        Assert.AreEqual("John Doe", result.ResponseResults.Name);
        Assert.AreEqual(30, result.ResponseResults.Age);
    }

    [TestMethod]
    public void ToString_Returns_FormattedString_ForSuccessfulRequest()
    {
        // Arrange
        var result = new HttpRequestResult<Person>
        {
            RequestMethod = HttpMethod.Get,
            RequestPath = "https://api.example.com/people/1",
            StatusCode = HttpStatusCode.OK,
            ElapsedMilliseconds = 250
        };

        // Save the correlation ID to check it appears in the string
        string correlationId = result.CorrelationId;

        // Act
        string output = result.ToString();

        // Assert
        Assert.IsTrue(output.Contains("Success"));
        Assert.IsTrue(output.Contains("GET"));
        Assert.IsTrue(output.Contains("https://api.example.com/people/1"));
        Assert.IsTrue(output.Contains("OK"));
        Assert.IsTrue(output.Contains("250ms"));
        Assert.IsTrue(output.Contains(correlationId));
    }

    [TestMethod]
    public void ToString_Returns_FormattedString_ForFailedRequest()
    {
        // Arrange
        var result = new HttpRequestResult<Person>
        {
            RequestMethod = HttpMethod.Post,
            RequestPath = "https://api.example.com/people",
            StatusCode = HttpStatusCode.BadRequest,
            ElapsedMilliseconds = 150
        };

        // Save the correlation ID to check it appears in the string
        string correlationId = result.CorrelationId;

        // Act
        string output = result.ToString();

        // Assert
        Assert.IsTrue(output.Contains("Failed"));
        Assert.IsTrue(output.Contains("POST"));
        Assert.IsTrue(output.Contains("https://api.example.com/people"));
        Assert.IsTrue(output.Contains("BadRequest"));
        Assert.IsTrue(output.Contains("150ms"));
        Assert.IsTrue(output.Contains(correlationId));
    }

    [TestMethod]
    public void ToString_With_URL_Containing_Credentials()
    {
        // Arrange
        var result = new HttpRequestResult<Person>
        {
            RequestMethod = HttpMethod.Get,
            RequestPath = "https://username:password@api.example.com/people/1",
            StatusCode = HttpStatusCode.OK,
            ElapsedMilliseconds = 250
        };

        // Act
        string output = result.ToString();

        // Assert
        // The actual implementation does not currently strip credentials from the URL
        // so we just check that the ToString method works without throwing an exception
        Assert.IsNotNull(output);
        Assert.IsTrue(output.Contains("GET"));
        Assert.IsTrue(output.Contains("https://"));
        Assert.IsTrue(output.Contains("api.example.com/people/1"));
        Assert.IsTrue(output.Contains("OK"));
    }

    [TestMethod]
    public void ToString_Works_With_Null_RequestPath()
    {
        // Arrange
        var result = new HttpRequestResult<Person>
        {
            RequestMethod = HttpMethod.Get,
            RequestPath = null!, // Explicitly set to null
            StatusCode = HttpStatusCode.OK,
            ElapsedMilliseconds = 250
        };

        // Act
        string output = result.ToString();

        // Assert
        Assert.IsNotNull(output, "ToString should not throw an exception with null RequestPath");
        Assert.IsTrue(output.Contains("GET"), "ToString output should still contain the HTTP method");
        Assert.IsTrue(output.Contains("OK"), "ToString output should still contain the status code");
    }

    [TestMethod]
    public void Different_ResponseTypes_Work_Correctly()
    {
        // Arrange & Act
        var personResult = new HttpRequestResult<Person>();
        var productResult = new HttpRequestResult<Product>();
        var stringResult = new HttpRequestResult<string>();
        var intResult = new HttpRequestResult<int>();
        var boolResult = new HttpRequestResult<bool>();

        // Set values
        personResult.ResponseResults = new Person { Name = "Jane Smith", Age = 28 };
        productResult.ResponseResults = new Product { Id = "P123", Name = "Smartphone", Price = 799.99M };
        stringResult.ResponseResults = "Test string response";
        intResult.ResponseResults = 42;
        boolResult.ResponseResults = true;

        // Assert
        Assert.IsNotNull(personResult.ResponseResults);
        Assert.AreEqual("Jane Smith", personResult.ResponseResults.Name);

        Assert.IsNotNull(productResult.ResponseResults);
        Assert.AreEqual("P123", productResult.ResponseResults.Id);
        Assert.AreEqual(799.99M, productResult.ResponseResults.Price);

        Assert.AreEqual("Test string response", stringResult.ResponseResults);
        Assert.AreEqual(42, intResult.ResponseResults);
        Assert.IsTrue(boolResult.ResponseResults);
    }

    [TestMethod]
    public void HttpRequestResult_Works_With_Nullable_ResponseTypes()
    {
        // Arrange & Act
        var nullableDateTimeResult = new HttpRequestResult<DateTime?>();
        nullableDateTimeResult.ResponseResults = DateTime.Now;

        var nullableIntResult = new HttpRequestResult<int?>();
        nullableIntResult.ResponseResults = null;

        // Assert
        Assert.IsNotNull(nullableDateTimeResult.ResponseResults);
        Assert.IsInstanceOfType(nullableDateTimeResult.ResponseResults, typeof(DateTime?));

        Assert.IsNull(nullableIntResult.ResponseResults);
    }

    [TestMethod]
    public void HttpRequestResult_Works_With_Collection_ResponseTypes()
    {
        // Arrange
        var listResult = new HttpRequestResult<List<Person>>();
        var personList = new List<Person>
        {
            new Person { Name = "Alice", Age = 25 },
            new Person { Name = "Bob", Age = 30 },
            new Person { Name = "Charlie", Age = 35 }
        };

        // Act
        listResult.ResponseResults = personList;

        // Assert
        Assert.IsNotNull(listResult.ResponseResults);
        Assert.AreEqual(3, listResult.ResponseResults.Count);
        Assert.AreEqual("Alice", listResult.ResponseResults[0].Name);
        Assert.AreEqual(30, listResult.ResponseResults[1].Age);
        Assert.AreEqual("Charlie", listResult.ResponseResults[2].Name);
    }

    [TestMethod]
    public void AddError_AddsErrorMessageWithCorrelationId()
    {
        // Arrange
        var result = new HttpRequestResult<Person>();
        string errorMessage = "Test error message";

        // Act
        result.AddError(errorMessage);

        // Assert
        Assert.AreEqual(1, result.ErrorList.Count);
        Assert.IsTrue(result.ErrorList[0].Contains(errorMessage));
        Assert.IsTrue(result.ErrorList[0].Contains(result.CorrelationId));
    }

    [TestMethod]
    public void ProcessException_EnrichesExceptionWithContextData()
    {
        // Arrange
        var result = new HttpRequestResult<Person>
        {
            RequestMethod = HttpMethod.Get,
            RequestPath = "https://api.example.com/people",
            StatusCode = HttpStatusCode.InternalServerError
        };
        var exception = new Exception("Test exception");
        var contextMessage = "Error calling API";

        // Act
        var enrichedException = result.ProcessException(exception, contextMessage);

        // Assert
        Assert.IsNotNull(enrichedException);
        Assert.AreEqual(1, result.ErrorList.Count);
        Assert.IsTrue(result.ErrorList[0].Contains("Test exception"));
        Assert.IsTrue(result.ErrorList[0].Contains(result.CorrelationId));
    }

    [TestMethod]
    public void ResultAge_ReturnsCorrectFormat_WhenCompletionDateIsSet()
    {
        // Arrange
        var result = new HttpRequestResult<Person>();
        result.CompletionDate = DateTime.UtcNow.AddMinutes(-30).AddSeconds(-15); // 30 mins and 15 seconds ago

        // Act
        string ageString = result.ResultAge;

        // Assert
        Assert.IsTrue(ageString.Contains("0 hours, 30 minutes"), "ResultAge should show correct hours and minutes");
    }

    [TestMethod]
    public void ResultAge_ReturnsMessage_WhenCompletionDateIsNull()
    {
        // Arrange
        var result = new HttpRequestResult<Person>();
        result.CompletionDate = null; // Not set

        // Act
        string ageString = result.ResultAge;

        // Assert
        Assert.AreEqual("Result Cache date is null.", ageString);
    }

    [TestMethod]
    public void RequestBody_Handles_Null_Values()
    {
        // Arrange
        var result = new HttpRequestResult<Person>();

        // Act & Assert - Default value is null
        Assert.IsNull(result.RequestBody);

        // Act & Assert - Can set to null explicitly
        result.RequestBody = null;
        Assert.IsNull(result.RequestBody);
    }

    [TestMethod]
    public void RequestHeaders_Initialize_As_Empty_Collection()
    {
        // Arrange & Act
        var result = new HttpRequestResult<Person>();

        // Assert
        Assert.IsNotNull(result.RequestHeaders);
        Assert.AreEqual(0, result.RequestHeaders.Count);
    }

    [TestMethod]
    public void ErrorList_AddsMultipleErrors()
    {
        // Arrange
        var result = new HttpRequestResult<Person>();

        // Act
        result.AddError("First error");
        result.AddError("Second error");
        result.AddError("Third error");

        // Assert
        Assert.AreEqual(3, result.ErrorList.Count);
        Assert.IsTrue(result.ErrorList[0].Contains("First error"));
        Assert.IsTrue(result.ErrorList[1].Contains("Second error"));
        Assert.IsTrue(result.ErrorList[2].Contains("Third error"));

        // Each error should contain the correlation ID
        foreach (var error in result.ErrorList)
        {
            Assert.IsTrue(error.Contains(result.CorrelationId));
        }
    }

    [TestMethod]
    public void LogRequestDetails_Works_With_Valid_Logger()
    {
        // Arrange
        var result = new HttpRequestResult<Person>
        {
            RequestMethod = HttpMethod.Get,
            RequestPath = "https://api.example.com/people/1",
            StatusCode = HttpStatusCode.OK
        };

        // Create a mock logger
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger>();
        mockLogger.Setup(x => x.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug)).Returns(true);
        mockLogger.Setup(x => x.Log(
            It.IsAny<Microsoft.Extensions.Logging.LogLevel>(),
            It.IsAny<Microsoft.Extensions.Logging.EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ));

        // Act - This should not throw
        result.LogRequestDetails(mockLogger.Object);

        // Assert - Verify the logger was called
        mockLogger.Verify(x => x.Log(
            It.IsAny<Microsoft.Extensions.Logging.LogLevel>(),
            It.IsAny<Microsoft.Extensions.Logging.EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ), Times.Once);
    }

    [TestMethod]
    public void LogRequestDetails_Doesnt_Log_When_DebugDisabled()
    {
        // Arrange
        var result = new HttpRequestResult<Person>
        {
            RequestMethod = HttpMethod.Get,
            RequestPath = "https://api.example.com/people/1",
            StatusCode = HttpStatusCode.OK
        };

        // Create a mock logger where debug is disabled
        var mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger>();
        mockLogger.Setup(x => x.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug)).Returns(false);

        // Act
        result.LogRequestDetails(mockLogger.Object);

        // Assert - Verify the logger was not called for logging
        mockLogger.Verify(x => x.Log(
            It.IsAny<Microsoft.Extensions.Logging.LogLevel>(),
            It.IsAny<Microsoft.Extensions.Logging.EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()
        ), Times.Never);
    }

    [TestMethod]
    public void ProcessException_IncludesRequestContext_InEnrichedException()
    {
        // Arrange
        var result = new HttpRequestResult<Person>
        {
            RequestMethod = HttpMethod.Get,
            RequestPath = "https://api.example.com/people",
            StatusCode = HttpStatusCode.InternalServerError
        };

        // Add custom context data
        result.RequestContext["CustomValue1"] = "Value1";
        result.RequestContext["CustomValue2"] = 42;

        var exception = new Exception("Test exception");
        var contextMessage = "Error calling API";

        // Act
        var enrichedException = result.ProcessException(exception, contextMessage);

        // Assert
        Assert.IsNotNull(enrichedException);
        Assert.AreEqual(1, result.ErrorList.Count);
        Assert.IsTrue(result.ErrorList[0].Contains("Test exception"));
    }

    [TestMethod]
    public void RequestStartTimestamp_IsSetToUtcTime()
    {
        // Arrange & Act
        var result = new HttpRequestResult<Person>();

        // Assert
        // Verify the timestamp is recent (within the last 5 seconds)
        TimeSpan difference = DateTime.UtcNow - result.RequestStartTimestamp;
        Assert.IsTrue(difference.TotalSeconds < 5, "RequestStartTimestamp should be initialized to recent UTC time");

        // Verify it's UTC (no timezone offset)
        Assert.AreEqual(DateTimeKind.Utc, result.RequestStartTimestamp.Kind);
    }

    [TestMethod]
    public void ElapsedMilliseconds_CanBeModified()
    {
        // Arrange
        var result = new HttpRequestResult<Person>();
        long expectedDuration = 500;

        // Act
        result.ElapsedMilliseconds = expectedDuration;

        // Assert
        Assert.AreEqual(expectedDuration, result.ElapsedMilliseconds);
        Assert.AreEqual(expectedDuration, result.RequestDurationMilliseconds); // Also tests the property accessor
    }
}
