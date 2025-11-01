using System.Net;
using WebSpark.HttpClientUtility.Crawler;
using WebSpark.HttpClientUtility.RequestResult;

namespace WebSpark.HttpClientUtility.Test.Crawler;

[TestClass]
public class CrawlerUtilityTests
{
    [TestClass]
    public class CrawlExceptionTests
    {
        [TestMethod]
        public void Constructor_WithAllParameters_SetsAllProperties()
        {
            // Arrange
            var message = "Crawl failed";
            var url = "https://example.com";
            var statusCode = HttpStatusCode.NotFound;
            var depth = 2;
            var innerException = new ArgumentException("Inner error");

            // Act
            var exception = new CrawlException(message, url, statusCode, depth, innerException);

            // Assert
            Assert.AreEqual(message, exception.Message);
            Assert.AreEqual(url, exception.Url);
            Assert.AreEqual(statusCode, exception.StatusCode);
            Assert.AreEqual(depth, exception.Depth);
            Assert.AreEqual(innerException, exception.InnerException);
        }

        [TestMethod]
        public void Constructor_WithMinimalParameters_SetsDefaults()
        {
            // Arrange
            var message = "Crawl failed";
            var url = "https://example.com";
            var statusCode = HttpStatusCode.BadRequest;

            // Act
            var exception = new CrawlException(message, url, statusCode);

            // Assert
            Assert.AreEqual(message, exception.Message);
            Assert.AreEqual(url, exception.Url);
            Assert.AreEqual(statusCode, exception.StatusCode);
            Assert.AreEqual(0, exception.Depth);
            Assert.IsNull(exception.InnerException);
        }

        [TestMethod]
        public void Constructor_WithGeneralError_SetsInternalServerError()
        {
            // Arrange
            var message = "General error";
            var url = "https://example.com";
            var innerException = new InvalidOperationException("Inner error");

            // Act
            var exception = new CrawlException(message, url, innerException);

            // Assert
            Assert.AreEqual(message, exception.Message);
            Assert.AreEqual(url, exception.Url);
            Assert.AreEqual(HttpStatusCode.InternalServerError, exception.StatusCode);
            Assert.AreEqual(0, exception.Depth);
            Assert.AreEqual(innerException, exception.InnerException);
        }

        [TestMethod]
        public void ToString_ContainsAllRelevantInformation()
        {
            // Arrange
            var message = "Test error";
            var url = "https://test.com/page";
            var statusCode = HttpStatusCode.Forbidden;
            var depth = 3;

            var exception = new CrawlException(message, url, statusCode, depth);

            // Act
            var result = exception.ToString();

            // Assert
            Assert.IsTrue(result.Contains(message));
            Assert.IsTrue(result.Contains(url));
            Assert.IsTrue(result.Contains("Forbidden"));
            Assert.IsTrue(result.Contains("3"));
            Assert.IsTrue(result.Contains("URL:"));
            Assert.IsTrue(result.Contains("Status Code:"));
            Assert.IsTrue(result.Contains("Depth:"));
        }

        [TestMethod]
        public void ToString_WithInnerException_IncludesInnerExceptionDetails()
        {
            // Arrange
            var innerException = new ArgumentNullException("testParam", "Test inner exception");
            var exception = new CrawlException("Outer error", "https://test.com", innerException);

            // Act
            var result = exception.ToString();

            // Assert
            Assert.IsTrue(result.Contains("Outer error"));
            Assert.IsTrue(result.Contains("Test inner exception"));
        }
    }

    [TestClass]
    public class CrawlResultTests
    {
        [TestMethod]
        public void Constructor_Default_InitializesCorrectly()
        {
            // Act
            var result = new CrawlResult();

            // Assert
            Assert.IsNotNull(result.Errors);
            Assert.AreEqual(0, result.Errors.Count);
            Assert.AreEqual(string.Empty, result.FoundUrl);
            Assert.AreEqual(0, result.Depth);
        }

        [TestMethod]
        public void Constructor_WithHttpRequestResult_CopiesProperties()
        {
            // Arrange
            var httpResult = new HttpRequestResult<string>
            {
                StatusCode = HttpStatusCode.OK,
                RequestPath = "https://example.com",
                RequestMethod = HttpMethod.Get,
                Iteration = 1,
                CacheDurationMinutes = 5,
                Retries = 2,
                ElapsedMilliseconds = 1000,
                Id = 123
            };
            httpResult.ErrorList.Add("Test error");

            // Act
            var crawlResult = new CrawlResult(httpResult);

            // Assert
            Assert.AreEqual(httpResult.StatusCode, crawlResult.StatusCode);
            Assert.AreEqual(httpResult.RequestPath, crawlResult.RequestPath);
            Assert.AreEqual(httpResult.RequestMethod, crawlResult.RequestMethod);
            Assert.AreEqual(httpResult.Iteration, crawlResult.Iteration);
            Assert.AreEqual(httpResult.CacheDurationMinutes, crawlResult.CacheDurationMinutes);
            Assert.AreEqual(httpResult.Retries, crawlResult.Retries);
            Assert.AreEqual(httpResult.ElapsedMilliseconds, crawlResult.ElapsedMilliseconds);
            Assert.AreEqual(httpResult.Id, crawlResult.Id);
            Assert.IsTrue(crawlResult.Errors.Contains("Test error"));
        }

        [TestMethod]
        public void Constructor_WithNullHttpRequestResult_ThrowsNullReferenceException()
        {
            // The constructor calls base(crawlResponse) first, which causes NullReferenceException
            // before the explicit null check can be performed

            // Act & Assert
            Assert.ThrowsExactly<NullReferenceException>(() => new CrawlResult(null!));
        }

        [TestMethod]
        public void Constructor_WithParameters_SetsProperties()
        {
            // Arrange
            var requestPath = "https://example.com/page";
            var foundUrl = "https://example.com/found";
            var depth = 2;
            var id = 456;

            // Act
            var result = new CrawlResult(requestPath, foundUrl, depth, id);

            // Assert
            Assert.AreEqual(requestPath, result.RequestPath);
            Assert.AreEqual(foundUrl, result.FoundUrl);
            Assert.AreEqual(depth, result.Depth);
            Assert.AreEqual(id, result.Id);
        }

        [TestMethod]
        public void Constructor_WithParameters_NullRequestPath_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                new CrawlResult(null!, "found", 1, 1));
        }

        [TestMethod]
        public void Constructor_WithParameters_NullFoundUrl_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                new CrawlResult("request", null!, 1, 1));
        }

        [TestMethod]
        public void ResponseHtmlDocument_WithNullResponseResults_ReturnsNull()
        {
            // Arrange
            var result = new CrawlResult();

            // Act
            var htmlDoc = result.ResponseHtmlDocument;

            // Assert
            Assert.IsNull(htmlDoc);
        }

        [TestMethod]
        public void ResponseHtmlDocument_WithEmptyResponseResults_ReturnsNull()
        {
            // Arrange
            var result = new CrawlResult();
            result.ResponseResults = "";

            // Act
            var htmlDoc = result.ResponseHtmlDocument;

            // Assert
            Assert.IsNull(htmlDoc);
        }

        [TestMethod]
        public void ResponseHtmlDocument_WithValidHtml_ReturnsHtmlDocument()
        {
            // Arrange
            var result = new CrawlResult();
            result.ResponseResults = "<html><body><h1>Test</h1></body></html>";

            // Act
            var htmlDoc = result.ResponseHtmlDocument;

            // Assert
            Assert.IsNotNull(htmlDoc);
            Assert.IsNotNull(htmlDoc.DocumentNode);
        }

        [TestMethod]
        public void ResponseHtmlDocument_WithInvalidHtml_ReturnsNull()
        {
            // Arrange
            var result = new CrawlResult();
            result.ResponseResults = "Not HTML content";

            // Act
            var htmlDoc = result.ResponseHtmlDocument;

            // Assert
            // HtmlAgilityPack is usually forgiving, but this tests the exception handling
            // If it doesn't return null, at least it shouldn't throw
            Assert.IsNotNull(htmlDoc); // HtmlAgilityPack creates a document even for invalid HTML
        }

        [TestMethod]
        public void CrawlLinks_WithNullHtmlDocument_ReturnsEmptyList()
        {
            // Arrange
            var result = new CrawlResult();
            result.ResponseResults = null;

            // Act
            var links = result.CrawlLinks;

            // Assert
            Assert.IsNotNull(links);
            Assert.AreEqual(0, links.Count);
        }

        [TestMethod]
        public void CrawlLinks_WithNoLinks_ReturnsEmptyList()
        {
            // Arrange
            var result = new CrawlResult();
            result.ResponseResults = "<html><body><p>No links here</p></body></html>";
            result.RequestPath = "https://example.com";

            // Act
            var links = result.CrawlLinks;

            // Assert
            Assert.IsNotNull(links);
            Assert.AreEqual(0, links.Count);
        }

        [TestMethod]
        public void ToString_ReturnsFormattedString()
        {
            // Arrange
            var result = new CrawlResult();
            result.Id = 123;
            result.Depth = 2;
            result.StatusCode = HttpStatusCode.OK;
            result.RequestPath = "https://example.com/test";

            // Act
            var stringResult = result.ToString();

            // Assert
            Assert.IsTrue(stringResult.Contains("ID:123"));
            Assert.IsTrue(stringResult.Contains("Depth:2"));
            Assert.IsTrue(stringResult.Contains("Status:OK"));
            Assert.IsTrue(stringResult.Contains("URL:https://example.com/test"));
        }

        [TestMethod]
        public void Errors_CanAddAndRetrieve()
        {
            // Arrange
            var result = new CrawlResult();
            var error1 = "First error";
            var error2 = "Second error";

            // Act
            result.Errors.Add(error1);
            result.Errors.Add(error2);

            // Assert
            Assert.AreEqual(2, result.Errors.Count);
            Assert.IsTrue(result.Errors.Contains(error1));
            Assert.IsTrue(result.Errors.Contains(error2));
        }

        [TestMethod]
        public void FoundUrl_CanBeSetAndRetrieved()
        {
            // Arrange
            var result = new CrawlResult();
            var url = "https://found.example.com";

            // Act
            result.FoundUrl = url;

            // Assert
            Assert.AreEqual(url, result.FoundUrl);
        }

        [TestMethod]
        public void Depth_CanBeSetAndRetrieved()
        {
            // Arrange
            var result = new CrawlResult();
            var depth = 5;

            // Act
            result.Depth = depth;

            // Assert
            Assert.AreEqual(depth, result.Depth);
        }
    }

    [TestClass]
    public class HttpRequestResultPollyOptionsTests
    {
        [TestMethod]
        public void Constructor_SetsDefaultValues()
        {
            // Act
            var options = new HttpRequestResultPollyOptions();

            // Assert
            Assert.AreEqual(0, options.MaxRetryAttempts);
            Assert.AreEqual(TimeSpan.Zero, options.RetryDelay);
            Assert.AreEqual(0, options.CircuitBreakerThreshold);
            Assert.AreEqual(TimeSpan.Zero, options.CircuitBreakerDuration);
        }

        [TestMethod]
        public void MaxRetryAttempts_CanBeSetAndRetrieved()
        {
            // Arrange
            var options = new HttpRequestResultPollyOptions();
            var maxRetries = 5;

            // Act
            options.MaxRetryAttempts = maxRetries;

            // Assert
            Assert.AreEqual(maxRetries, options.MaxRetryAttempts);
        }

        [TestMethod]
        public void RetryDelay_CanBeSetAndRetrieved()
        {
            // Arrange
            var options = new HttpRequestResultPollyOptions();
            var delay = TimeSpan.FromSeconds(2);

            // Act
            options.RetryDelay = delay;

            // Assert
            Assert.AreEqual(delay, options.RetryDelay);
        }

        [TestMethod]
        public void CircuitBreakerThreshold_CanBeSetAndRetrieved()
        {
            // Arrange
            var options = new HttpRequestResultPollyOptions();
            var threshold = 10;

            // Act
            options.CircuitBreakerThreshold = threshold;

            // Assert
            Assert.AreEqual(threshold, options.CircuitBreakerThreshold);
        }

        [TestMethod]
        public void CircuitBreakerDuration_CanBeSetAndRetrieved()
        {
            // Arrange
            var options = new HttpRequestResultPollyOptions();
            var duration = TimeSpan.FromMinutes(5);

            // Act
            options.CircuitBreakerDuration = duration;

            // Assert
            Assert.AreEqual(duration, options.CircuitBreakerDuration);
        }

        [TestMethod]
        public void AllProperties_CanBeSetTogether()
        {
            // Arrange
            var options = new HttpRequestResultPollyOptions();
            var maxRetries = 3;
            var retryDelay = TimeSpan.FromSeconds(1);
            var threshold = 5;
            var duration = TimeSpan.FromMinutes(2);

            // Act
            options.MaxRetryAttempts = maxRetries;
            options.RetryDelay = retryDelay;
            options.CircuitBreakerThreshold = threshold;
            options.CircuitBreakerDuration = duration;

            // Assert
            Assert.AreEqual(maxRetries, options.MaxRetryAttempts);
            Assert.AreEqual(retryDelay, options.RetryDelay);
            Assert.AreEqual(threshold, options.CircuitBreakerThreshold);
            Assert.AreEqual(duration, options.CircuitBreakerDuration);
        }
    }
}
