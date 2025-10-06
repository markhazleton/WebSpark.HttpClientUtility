using Microsoft.Extensions.Logging;
using System.Net;
using WebSpark.HttpClientUtility.MockService;

namespace WebSpark.HttpClientUtility.Test.MockService;

[TestClass]
public class MockServiceUtilityTests
{
    [TestClass]
    public class ConsoleLoggerTests
    {
        private ConsoleLogger _logger = null!;
        private StringWriter _consoleOutput = null!;
        private TextWriter _originalOutput = null!;

        [TestInitialize]
        public void Setup()
        {
            _logger = new ConsoleLogger();
            _consoleOutput = new StringWriter();
            _originalOutput = Console.Out;
            Console.SetOut(_consoleOutput);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Console.SetOut(_originalOutput);
            _consoleOutput.Dispose();
        }

        [TestMethod]
        public void IsEnabled_AnyLogLevel_ReturnsTrue()
        {
            // Test all log levels
            Assert.IsTrue(_logger.IsEnabled(LogLevel.Trace));
            Assert.IsTrue(_logger.IsEnabled(LogLevel.Debug));
            Assert.IsTrue(_logger.IsEnabled(LogLevel.Information));
            Assert.IsTrue(_logger.IsEnabled(LogLevel.Warning));
            Assert.IsTrue(_logger.IsEnabled(LogLevel.Error));
            Assert.IsTrue(_logger.IsEnabled(LogLevel.Critical));
            Assert.IsTrue(_logger.IsEnabled(LogLevel.None));
        }

        [TestMethod]
        public void Log_WithMessage_OutputsToConsole()
        {
            // Arrange
            var message = "Test log message";

            // Act
            _logger.Log(LogLevel.Information, new EventId(1), message, null, (state, ex) => state);

            // Assert
            var output = _consoleOutput.ToString();
            Assert.IsTrue(output.Contains("[Information]"));
            Assert.IsTrue(output.Contains(message));
        }

        [TestMethod]
        public void Log_WithException_OutputsExceptionToConsole()
        {
            // Arrange
            var message = "Test error message";
            var exception = new InvalidOperationException("Test exception");

            // Act
            _logger.Log(LogLevel.Error, new EventId(1), message, exception, (state, ex) => state);

            // Assert
            var output = _consoleOutput.ToString();
            Assert.IsTrue(output.Contains("[Error]"));
            Assert.IsTrue(output.Contains(message));
            Assert.IsTrue(output.Contains("Test exception"));
        }

        [TestMethod]
        public void Log_WithNullFormatter_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                _logger.Log(LogLevel.Information, new EventId(), "state", null, null!));
        }

        [TestMethod]
        public void Log_WithEmptyMessage_DoesNotOutput()
        {
            // Act
            _logger.Log(LogLevel.Information, new EventId(), "", null, (state, ex) => state);

            // Assert
            var output = _consoleOutput.ToString();
            Assert.IsTrue(string.IsNullOrWhiteSpace(output));
        }

        [TestMethod]
        public void Log_FormatterReturnsNull_DoesNotOutput()
        {
            // Act
            _logger.Log(LogLevel.Information, new EventId(), "state", null, (state, ex) => null!);

            // Assert
            var output = _consoleOutput.ToString();
            Assert.IsTrue(string.IsNullOrWhiteSpace(output));
        }

        [TestMethod]
        public void TrackEvent_WithMessage_OutputsToConsole()
        {
            // Arrange
            var message = "Test event tracking";

            // Act
            _logger.TrackEvent(message);

            // Assert
            var output = _consoleOutput.ToString();
            Assert.IsTrue(output.Contains("[TrackEvent]"));
            Assert.IsTrue(output.Contains(message));
        }

        [TestMethod]
        public void TrackException_WithExceptionAndMessage_OutputsToConsole()
        {
            // Arrange
            var message = "Exception occurred";
            var exception = new ArgumentException("Test argument exception");

            // Act
            _logger.TrackException(exception, message);

            // Assert
            var output = _consoleOutput.ToString();
            Assert.IsTrue(output.Contains("[TrackException]"));
            Assert.IsTrue(output.Contains(message));
            Assert.IsTrue(output.Contains("Test argument exception"));
        }

        [TestMethod]
        public void BeginScope_ReturnsNoOpDisposable()
        {
            // Act
            var scope = _logger.BeginScope("test scope");

            // Assert
            Assert.IsNotNull(scope);
            
            // Should not throw when disposed
            scope.Dispose();
        }

        [TestMethod]
        public void BeginScope_GenericState_ReturnsNoOpDisposable()
        {
            // Arrange
            var state = new { Property = "value" };

            // Act
            using var scope = ((ILogger)_logger).BeginScope(state);

            // Assert
            Assert.IsNotNull(scope);
            // Should not throw when disposed - using statement handles disposal
        }

        [TestMethod]
        public void LogLevel_OutputFormat_ContainsTimestamp()
        {
            // Act
            _logger.Log(LogLevel.Warning, new EventId(), "test", null, (state, ex) => state);

            // Assert
            var output = _consoleOutput.ToString();
            // Should contain a date/time format
            Assert.IsTrue(output.Contains("[Warning]"));
            // Basic check for timestamp format (year should be present)
            Assert.IsTrue(output.Contains(DateTime.Now.Year.ToString()));
        }
    }

    [TestClass]
    public class MockResultsTests
    {
        [TestMethod]
        public void Constructor_WithParameters_SetsProperties()
        {
            // Arrange
            var loopCount = 5;
            var maxTimeMS = 1000;

            // Act
            var result = new MockResults(loopCount, maxTimeMS);

            // Assert
            Assert.AreEqual(loopCount, result.LoopCount);
            Assert.AreEqual(maxTimeMS, result.MaxTimeMS);
            Assert.AreEqual(0, result.RunTimeMS);
            Assert.AreEqual("initialization", result.Message);
            Assert.AreEqual("initialization", result.ResultValue);
        }

        [TestMethod]
        public void Constructor_Default_SetsDefaultValues()
        {
            // Act
            var result = new MockResults();

            // Assert
            Assert.AreEqual(0, result.LoopCount);
            Assert.AreEqual(0, result.MaxTimeMS);
            Assert.AreEqual(0, result.RunTimeMS);
            Assert.AreEqual("initialization", result.Message);
            Assert.AreEqual("initialization", result.ResultValue);
        }

        [TestMethod]
        public void Properties_CanBeModified()
        {
            // Arrange
            var result = new MockResults();

            // Act
            result.LoopCount = 10;
            result.MaxTimeMS = 2000;
            result.RunTimeMS = 1500;
            result.Message = "Test completed";
            result.ResultValue = "Success";

            // Assert
            Assert.AreEqual(10, result.LoopCount);
            Assert.AreEqual(2000, result.MaxTimeMS);
            Assert.AreEqual(1500, result.RunTimeMS);
            Assert.AreEqual("Test completed", result.Message);
            Assert.AreEqual("Success", result.ResultValue);
        }

        [TestMethod]
        public void RunTimeMS_CanBeNull()
        {
            // Arrange
            var result = new MockResults();

            // Act
            result.RunTimeMS = null;

            // Assert
            Assert.IsNull(result.RunTimeMS);
        }

        [TestMethod]
        public void Message_CanBeNull()
        {
            // Arrange
            var result = new MockResults();

            // Act
            result.Message = null;

            // Assert
            Assert.IsNull(result.Message);
        }

        [TestMethod]
        public void ResultValue_CanBeNull()
        {
            // Arrange
            var result = new MockResults();

            // Act
            result.ResultValue = null;

            // Assert
            Assert.IsNull(result.ResultValue);
        }
    }
}