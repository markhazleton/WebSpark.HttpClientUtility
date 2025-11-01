using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using WebSpark.HttpClientUtility.CurlService;

namespace WebSpark.HttpClientUtility.Test.CurlService
{
    [TestClass]
    public class CurlCommandSaverTests
    {
     private Mock<ILogger> mockLogger = null!;
        private Mock<IConfiguration> mockConfiguration = null!;
    private string tempDirectory = null!;

        [TestInitialize]
        public void TestInitialize()
        {
  // Use Loose mocking to avoid strict verification issues
            mockLogger = new Mock<ILogger>(MockBehavior.Loose);
       mockConfiguration = new Mock<IConfiguration>(MockBehavior.Loose);

            // Create a temporary directory for testing
    tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
         Directory.CreateDirectory(tempDirectory);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Clean up the temporary directory after tests
     if (Directory.Exists(tempDirectory))
            {
   Directory.Delete(tempDirectory, true);
            }
        }

        private CurlCommandSaver CreateCurlCommandSaver(bool configureValidFolder = true)
        {
   if (configureValidFolder)
            {
       // Setup the base CsvOutputFolder configuration
    mockConfiguration.Setup(x => x["CsvOutputFolder"]).Returns(tempDirectory);
          mockConfiguration.Setup(x => x["CsvFileName"]).Returns("curl_commands");

    // Setup the configuration section approach for other settings
       var mockSection = new Mock<IConfigurationSection>(MockBehavior.Loose);
        mockSection.Setup(x => x.Value).Returns("false"); // UseBatchProcessing = false
           mockConfiguration.Setup(x => x.GetSection("CurlCommandSaver:UseBatchProcessing")).Returns(mockSection.Object);
       }
       else
     {
   mockConfiguration.Setup(x => x["CsvOutputFolder"]).Returns((string?)null);
          mockConfiguration.Setup(x => x["CsvFileName"]).Returns("curl_commands");

   // Setup the configuration section approach for other settings
       var mockSection = new Mock<IConfigurationSection>(MockBehavior.Loose);
     mockSection.Setup(x => x.Value).Returns("false"); // UseBatchProcessing = false
             mockConfiguration.Setup(x => x.GetSection("CurlCommandSaver:UseBatchProcessing")).Returns(mockSection.Object);
  }

   return new CurlCommandSaver(
  mockLogger.Object,
     mockConfiguration.Object);
        }

        [TestMethod]
        public void Constructor_WithMissingCsvOutputFolder_LogsWarningAndDisablesFileLogging()
        {
       // Arrange & Act
 var curlCommandSaver = CreateCurlCommandSaver(configureValidFolder: false);

   // Assert
    Assert.IsNotNull(curlCommandSaver, "CurlCommandSaver should be created even without output folder configured");

        // Verify that warning was logged
     mockLogger.Verify(x => x.Log(
     LogLevel.Warning,
                It.IsAny<EventId>(),
       It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CsvOutputFolder is not configured")),
 It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

        // Verify initialization log
  mockLogger.Verify(x => x.Log(
        LogLevel.Information,
     It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("CurlCommandSaver initialized")),
       It.IsAny<Exception?>(),
       It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [TestMethod]
        public async Task SaveCurlCommandAsync_WithoutConfiguration_DoesNotCreateFile()
        {
            // Arrange
 var curlCommandSaver = CreateCurlCommandSaver(configureValidFolder: false);
       var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/test");

            // Act
 await curlCommandSaver.SaveCurlCommandAsync(request);
        await curlCommandSaver.FlushAsync();

       // Assert
        string csvFilePath = Path.Combine(tempDirectory, "curl_commands.csv");
 Assert.IsFalse(File.Exists(csvFilePath), "CSV file should NOT be created when configuration is missing");

            // Verify that curl command was still created (logged) but not saved to file
            mockLogger.Verify(x => x.Log(
       LogLevel.Information,
     It.IsAny<EventId>(),
          It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Created curl command")),
     It.IsAny<Exception?>(),
   It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.AtLeastOnce);
        }

     [TestMethod]
        public async Task SaveCurlCommandAsync_WithGetRequest_SavesToCsv()
        {
            // Arrange
    var curlCommandSaver = CreateCurlCommandSaver();
     var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/test");
            string memberName = "TestMethod";
            string filePath = "TestFile.cs";
   int lineNumber = 42;

      // Act
            await curlCommandSaver.SaveCurlCommandAsync(
             request,
  memberName,
              filePath,
     lineNumber);

     // Make sure any batched records are processed
   await curlCommandSaver.FlushAsync();

            // Assert
  string csvFilePath = Path.Combine(tempDirectory, "curl_commands.csv");
         Assert.IsTrue(File.Exists(csvFilePath), "CSV file should have been created");
     string fileContent = File.ReadAllText(csvFilePath);

    // Check for key components instead of exact string match
         Assert.IsTrue(fileContent.Contains("curl"), "CSV should contain curl command");
            Assert.IsTrue(fileContent.Contains("example.com"), "CSV should contain the domain");
            Assert.IsTrue(fileContent.Contains("/api/test"), "CSV should contain the path");
    Assert.IsTrue(fileContent.Contains("TestMethod"), "CSV should contain the calling method name");
     }

        [TestMethod]
   public async Task SaveCurlCommandAsync_WithPostRequest_IncludesMethod()
        {
 // Arrange
      var curlCommandSaver = CreateCurlCommandSaver();
   var request = new HttpRequestMessage(HttpMethod.Post, "https://example.com/api/test");

      // Act
      await curlCommandSaver.SaveCurlCommandAsync(request);

// Make sure any batched records are processed
    await curlCommandSaver.FlushAsync();

     // Assert
 string csvFilePath = Path.Combine(tempDirectory, "curl_commands.csv");
            Assert.IsTrue(File.Exists(csvFilePath), "CSV file should have been created");
            string fileContent = File.ReadAllText(csvFilePath);
         Assert.IsTrue(fileContent.Contains("-X POST"), "CSV should contain the POST method");
 }

        [TestMethod]
     public async Task SaveCurlCommandAsync_WithRequestContent_IncludesData()
        {
       // Arrange
     var curlCommandSaver = CreateCurlCommandSaver();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://example.com/api/test");

          string content = "{\"name\":\"test\",\"value\":\"data\"}";
   request.Content = new StringContent(content, Encoding.UTF8, "application/json");

         // Act
await curlCommandSaver.SaveCurlCommandAsync(request);

    // Make sure any batched records are processed
            await curlCommandSaver.FlushAsync();

            // Assert
    string csvFilePath = Path.Combine(tempDirectory, "curl_commands.csv");
      Assert.IsTrue(File.Exists(csvFilePath), "CSV file should have been created");
            string fileContent = File.ReadAllText(csvFilePath);
     Assert.IsTrue(fileContent.Contains("-d"), "CSV should contain the data parameter");
         Assert.IsTrue(fileContent.Contains("name"), "CSV should contain the JSON content");
        }

[TestMethod]
        public async Task SaveCurlCommandAsync_MultipleCalls_AppendsToFile()
        {
            // Arrange
            var request1 = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/first");
 var request2 = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/second");

         var curlCommandSaver = CreateCurlCommandSaver();

        // Act
    await curlCommandSaver.SaveCurlCommandAsync(request1);
    await curlCommandSaver.SaveCurlCommandAsync(request2);

            // Make sure any batched records are processed
            await curlCommandSaver.FlushAsync();

       // Assert
            string csvFilePath = Path.Combine(tempDirectory, "curl_commands.csv");
   Assert.IsTrue(File.Exists(csvFilePath), "CSV file should have been created");
     string fileContent = File.ReadAllText(csvFilePath);
      Assert.IsTrue(fileContent.Contains("api/first"), "CSV should contain the first request");
  Assert.IsTrue(fileContent.Contains("api/second"), "CSV should contain the second request");

  // Check that we have more than one line (header + at least 2 records)
            string[] lines = fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
      Assert.IsTrue(lines.Length >= 3, "CSV should contain header and at least 2 records");
        }

  [TestMethod]
        public async Task SaveCurlCommandAsync_WithNullRequest_HandlesGracefully()
        {
            // Arrange
          var curlCommandSaver = CreateCurlCommandSaver();
            HttpRequestMessage? request = null;

            // Act
    await curlCommandSaver.SaveCurlCommandAsync(request);

     // Make sure any batched records are processed
            await curlCommandSaver.FlushAsync();

   // Assert
         string csvFilePath = Path.Combine(tempDirectory, "curl_commands.csv");
   Assert.IsTrue(File.Exists(csvFilePath), "CSV file should have been created even with null request");

       // Additional validation for null request handling
       string fileContent = File.ReadAllText(csvFilePath);
  Assert.IsTrue(fileContent.Contains("curl"), "CSV should contain the basic curl command");
        }
    }
}
