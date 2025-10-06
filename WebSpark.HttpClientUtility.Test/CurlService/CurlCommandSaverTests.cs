using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Text;
using WebSpark.HttpClientUtility.CurlService;

namespace WebSpark.HttpClientUtility.Test.CurlService
{
    [TestClass]
    public class CurlCommandSaverTests
    {
        private MockRepository mockRepository = null!;
        private Mock<ILogger> mockLogger = null!;
        private Mock<IConfiguration> mockConfiguration = null!;
        private Mock<IConfigurationSection> mockConfigSection = null!;
        private string tempDirectory = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockLogger = this.mockRepository.Create<ILogger>();
            this.mockConfiguration = this.mockRepository.Create<IConfiguration>();
            this.mockConfigSection = this.mockRepository.Create<IConfigurationSection>();

            // Create a temporary directory for testing
            this.tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
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
                this.mockConfiguration.Setup(x => x["CsvOutputFolder"]).Returns(tempDirectory);

                // Setup the additional configuration options needed by the enhanced CurlCommandSaver
                this.mockConfiguration.Setup(x => x["CsvFileName"]).Returns("curl_commands");

                // Setup the configuration section approach for other settings
                var mockSection = this.mockRepository.Create<IConfigurationSection>();
                mockSection.Setup(x => x.Value).Returns("false"); // UseBatchProcessing = false
                this.mockConfiguration.Setup(x => x.GetSection("CurlCommandSaver:UseBatchProcessing")).Returns(mockSection.Object);

                // Set up logging expectation - this will be called when command is created
                this.mockLogger.Setup(x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()));
            }
            else
            {
                this.mockConfiguration.Setup(x => x["CsvOutputFolder"]).Returns((string?)null);

                // These still need to be set up for the constructor to complete before throwing
                this.mockConfiguration.Setup(x => x["CsvFileName"]).Returns("curl_commands");

                // Setup the configuration section approach for other settings
                var mockSection = this.mockRepository.Create<IConfigurationSection>();
                mockSection.Setup(x => x.Value).Returns("false"); // UseBatchProcessing = false
                this.mockConfiguration.Setup(x => x.GetSection("CurlCommandSaver:UseBatchProcessing")).Returns(mockSection.Object);
            }

            return new CurlCommandSaver(
                this.mockLogger.Object,
                this.mockConfiguration.Object);
        }

        [TestMethod]
        public void Constructor_WithMissingCsvOutputFolder_ThrowsArgumentException()
        {
            // Act & Assert
            Assert.ThrowsExactly<ArgumentException>(() => CreateCurlCommandSaver(configureValidFolder: false));
        }

        [TestMethod]
        public async Task SaveCurlCommandAsync_WithGetRequest_SavesToCsv()
        {
            // Arrange
            var curlCommandSaver = this.CreateCurlCommandSaver();
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/test");
            string memberName = "TestMethod";
            string filePath = "TestFile.cs";
            int lineNumber = 42;

            // Set up logging expectation with more flexible matching
            this.mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("curl")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

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

            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task SaveCurlCommandAsync_WithPostRequest_IncludesMethod()
        {
            // Arrange
            var curlCommandSaver = this.CreateCurlCommandSaver();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://example.com/api/test");

            // Set up logging expectation
            this.mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("curl -X POST")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            // Act
            await curlCommandSaver.SaveCurlCommandAsync(request);

            // Make sure any batched records are processed
            await curlCommandSaver.FlushAsync();

            // Assert
            string csvFilePath = Path.Combine(tempDirectory, "curl_commands.csv");
            Assert.IsTrue(File.Exists(csvFilePath), "CSV file should have been created");
            string fileContent = File.ReadAllText(csvFilePath);
            Assert.IsTrue(fileContent.Contains("-X POST"), "CSV should contain the POST method");
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task SaveCurlCommandAsync_WithRequestContent_IncludesData()
        {
            // Arrange
            var curlCommandSaver = this.CreateCurlCommandSaver();
            var request = new HttpRequestMessage(HttpMethod.Post, "https://example.com/api/test");

            string content = "{\"name\":\"test\",\"value\":\"data\"}";
            request.Content = new StringContent(content, Encoding.UTF8, "application/json");

            // Set up logging expectation - just check for general patterns
            this.mockLogger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("-d") &&
                    v.ToString()!.Contains("name")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

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
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        public async Task SaveCurlCommandAsync_MultipleCalls_AppendsToFile()
        {
            // Arrange
            var request1 = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/first");
            var request2 = new HttpRequestMessage(HttpMethod.Get, "https://example.com/api/second");

            // Use looser mock setup since we're just validating the file output
            this.mockRepository = new MockRepository(MockBehavior.Loose);
            this.mockLogger = this.mockRepository.Create<ILogger>();
            this.mockConfiguration = this.mockRepository.Create<IConfiguration>();

            // Setup all the configuration options needed
            this.mockConfiguration.Setup(x => x["CsvOutputFolder"]).Returns(tempDirectory);
            this.mockConfiguration.Setup(x => x["CsvFileName"]).Returns("curl_commands");

            // Setup the configuration section approach for other settings
            var mockSection = this.mockRepository.Create<IConfigurationSection>();
            mockSection.Setup(x => x.Value).Returns("false"); // UseBatchProcessing = false
            this.mockConfiguration.Setup(x => x.GetSection("CurlCommandSaver:UseBatchProcessing")).Returns(mockSection.Object);

            var curlCommandSaver = new CurlCommandSaver(
                this.mockLogger.Object,
                this.mockConfiguration.Object);

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
            // Use looser mock setup since we're just validating null handling
            this.mockRepository = new MockRepository(MockBehavior.Loose);
            this.mockLogger = this.mockRepository.Create<ILogger>();
            this.mockConfiguration = this.mockRepository.Create<IConfiguration>();

            // Setup all the configuration options needed
            this.mockConfiguration.Setup(x => x["CsvOutputFolder"]).Returns(tempDirectory);
            this.mockConfiguration.Setup(x => x["CsvFileName"]).Returns("curl_commands");

            // Setup the configuration section approach for other settings
            var mockSection = this.mockRepository.Create<IConfigurationSection>();
            mockSection.Setup(x => x.Value).Returns("false"); // UseBatchProcessing = false
            this.mockConfiguration.Setup(x => x.GetSection("CurlCommandSaver:UseBatchProcessing")).Returns(mockSection.Object);

            var curlCommandSaver = new CurlCommandSaver(
                this.mockLogger.Object,
                this.mockConfiguration.Object);

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

        /// <summary>
        /// Makes sure the content of the CSV file matches the expected curl command.
        /// </summary>
        /// <param name="filePath">Path to the CSV file</param>
        /// <param name="expectedContent">Content that should be in the CSV file</param>
        /// <returns>True if the file contains the expected content, false otherwise</returns>
        private bool ContainsCurlCommand(string filePath, string expectedContent)
        {
            if (!File.Exists(filePath))
                return false;

            string content = File.ReadAllText(filePath);
            return content.Contains(expectedContent);
        }
    }
}
