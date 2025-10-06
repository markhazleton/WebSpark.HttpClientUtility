using Moq;
using System.Diagnostics;
using WebSpark.HttpClientUtility.MockService;

namespace WebSpark.HttpClientUtility.Test.MockService
{
    [TestClass]
    public class AsyncMockServiceTests
    {
        private AsyncMockService _asyncMockService = null!;
        private Mock<ICommonLogger> _mockLogger = null!;

        [TestInitialize]
        public void Setup()
        {
            _asyncMockService = new AsyncMockService();
            _mockLogger = new Mock<ICommonLogger>();
        }

        #region ExampleMethodAsync Tests

        [TestMethod]
        public async Task ExampleMethodAsync_WithCancellationToken_ShouldCancelGracefully()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            var stopwatch = Stopwatch.StartNew();

            // Act & Assert
            var task = AsyncMockService.ExampleMethodAsync(cts.Token);

            // Cancel after a short delay
            await Task.Delay(500);
            cts.Cancel();

            await Assert.ThrowsExactlyAsync<TaskCanceledException>(() => task);
            stopwatch.Stop();

            // Verify it was cancelled relatively quickly (should be less than 2 seconds)
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 2000);
        }

        [TestMethod]
        public async Task ExampleMethodAsync_WithAlreadyCancelledToken_ShouldThrowImmediately()
        {
            // Arrange
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsExactlyAsync<OperationCanceledException>(
                () => AsyncMockService.ExampleMethodAsync(cts.Token));
        }

        #endregion

        #region LongRunningCancellableOperation Tests

        [TestMethod]
        public async Task LongRunningCancellableOperation_WithValidInput_ShouldReturnExpectedResult()
        {
            // Arrange
            const int loop = 5;
            const decimal expectedResult = 0 + 1 + 2 + 3 + 4; // Sum of 0 to 4
            using var cts = new CancellationTokenSource();

            // Act
            var result = await AsyncMockService.LongRunningCancellableOperation(loop, cts.Token);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public async Task LongRunningCancellableOperation_WithZeroLoop_ShouldReturnZero()
        {
            // Arrange
            const int loop = 0;
            using var cts = new CancellationTokenSource();

            // Act
            var result = await AsyncMockService.LongRunningCancellableOperation(loop, cts.Token);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public async Task LongRunningCancellableOperation_WithCancellation_ShouldThrowTaskCancelledException()
        {
            // Arrange
            const int loop = 1000; // Large number to ensure cancellation happens
            using var cts = new CancellationTokenSource();

            // Act
            var task = AsyncMockService.LongRunningCancellableOperation(loop, cts.Token);

            // Cancel after a short delay
            await Task.Delay(100);
            cts.Cancel();

            // Assert
            await Assert.ThrowsExactlyAsync<TaskCanceledException>(() => task);
        }

        [TestMethod]
        public async Task LongRunningCancellableOperation_WithNegativeLoop_ShouldReturnZero()
        {
            // Arrange
            const int loop = -5;
            using var cts = new CancellationTokenSource();

            // Act
            var result = await AsyncMockService.LongRunningCancellableOperation(loop, cts.Token);

            // Assert
            Assert.AreEqual(0, result);
        }

        #endregion

        #region LongRunningOperation Tests

        [TestMethod]
        public async Task LongRunningOperation_WithValidInput_ShouldReturnExpectedResult()
        {
            // Arrange
            const int loop = 5;
            const decimal expectedResult = 0 + 1 + 2 + 3 + 4; // Sum of 0 to 4

            // Act
            var result = await _asyncMockService.LongRunningOperation(loop);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public async Task LongRunningOperation_WithZeroLoop_ShouldReturnZero()
        {
            // Arrange
            const int loop = 0;

            // Act
            var result = await _asyncMockService.LongRunningOperation(loop);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public async Task LongRunningOperation_WithNegativeLoop_ShouldReturnZero()
        {
            // Arrange
            const int loop = -3;

            // Act
            var result = await _asyncMockService.LongRunningOperation(loop);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public async Task LongRunningOperation_WithLargeLoop_ShouldCompleteSuccessfully()
        {
            // Arrange
            const int loop = 100;
            const decimal expectedResult = 4950; // Sum of 0 to 99

            // Act
            var result = await _asyncMockService.LongRunningOperation(loop);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        #endregion

        #region LongRunningOperationWithCancellationTokenAsync Tests

        [TestMethod]
        public async Task LongRunningOperationWithCancellationTokenAsync_WithValidInput_ShouldReturnExpectedResult()
        {
            // Arrange
            const int loop = 5;
            const decimal expectedResult = 0 + 1 + 2 + 3 + 4;
            using var cts = new CancellationTokenSource();

            // Act
            var result = await _asyncMockService.LongRunningOperationWithCancellationTokenAsync(loop, cts.Token);

            // Assert
            Assert.AreEqual(expectedResult, result);
        }

        [TestMethod]
        public async Task LongRunningOperationWithCancellationTokenAsync_WithCancellation_ShouldThrowTaskCancelledException()
        {
            // Arrange
            const int loop = 1000; // Large number to ensure we can cancel
            using var cts = new CancellationTokenSource();

            // Act
            var task = _asyncMockService.LongRunningOperationWithCancellationTokenAsync(loop, cts.Token);

            // Cancel immediately
            cts.Cancel();

            // Assert
            await Assert.ThrowsExactlyAsync<TaskCanceledException>(() => task);
        }

        [TestMethod]
        public async Task LongRunningOperationWithCancellationTokenAsync_WithZeroLoop_ShouldReturnZero()
        {
            // Arrange
            const int loop = 0;
            using var cts = new CancellationTokenSource();

            // Act
            var result = await _asyncMockService.LongRunningOperationWithCancellationTokenAsync(loop, cts.Token);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public async Task LongRunningOperationWithCancellationTokenAsync_WithAlreadyCancelledToken_ShouldThrowTaskCancelledException()
        {
            // Arrange
            const int loop = 5;
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsExactlyAsync<TaskCanceledException>(
                () => _asyncMockService.LongRunningOperationWithCancellationTokenAsync(loop, cts.Token));
        }

        #endregion

        #region LongRunningTask Tests

        [TestMethod]
        public async Task LongRunningTask_WithValidParameters_ShouldCompleteSuccessfully()
        {
            // Arrange
            const string taskName = "TestTask";
            const int delay = 10;
            const int iterations = 3;
            const bool throwEx = false;
            using var cts = new CancellationTokenSource();

            // Act
            await AsyncMockService.LongRunningTask(taskName, delay, iterations, throwEx, _mockLogger.Object, cts.Token);

            // Assert
            _mockLogger.Verify(x => x.TrackEvent(It.Is<string>(s => s.Contains("TestTask completed"))), Times.Once);
        }

        [TestMethod]
        public async Task LongRunningTask_WithException_ShouldThrowAndLogException()
        {
            // Arrange
            const string taskName = "TestTaskWithException";
            const int delay = 10;
            const int iterations = 2;
            const bool throwEx = true;
            using var cts = new CancellationTokenSource();

            // Act & Assert
            await Assert.ThrowsExactlyAsync<Exception>(
                () => AsyncMockService.LongRunningTask(taskName, delay, iterations, throwEx, _mockLogger.Object, cts.Token));

            // Verify exception was logged
            _mockLogger.Verify(x => x.TrackException(
                It.IsAny<Exception>(),
                It.Is<string>(s => s.Contains("TestTaskWithException Exception"))),
                Times.Once);
        }

        [TestMethod]
        public async Task LongRunningTask_WithCancellation_ShouldThrowTaskCancelledExceptionAndLog()
        {
            // Arrange
            const string taskName = "TestTaskCancelled";
            const int delay = 100;
            const int iterations = 10;
            const bool throwEx = false;
            using var cts = new CancellationTokenSource();

            // Act
            var task = AsyncMockService.LongRunningTask(taskName, delay, iterations, throwEx, _mockLogger.Object, cts.Token);

            // Cancel after a short delay
            await Task.Delay(50);
            cts.Cancel();

            // Assert
            await Assert.ThrowsExactlyAsync<TaskCanceledException>(() => task);

            // Verify cancellation was logged
            _mockLogger.Verify(x => x.TrackEvent(
                It.Is<string>(s => s.Contains("TestTaskCancelled TaskCanceledException"))),
                Times.Once);
        }

        [TestMethod]
        public async Task LongRunningTask_WithCancellationTokenRequested_ShouldThrowCancellationException()
        {
            // Arrange
            const string taskName = "TestTaskTimeout";
            const int delay = 10;
            const int iterations = 5;
            const bool throwEx = false;
            using var cts = new CancellationTokenSource();

            // Start the task and then request cancellation
            var task = AsyncMockService.LongRunningTask(taskName, delay, iterations, throwEx, _mockLogger.Object, cts.Token);
            cts.Cancel();

            // Act & Assert
            // The method can throw either TaskCanceledException or TimeoutException depending on timing
            var exception = await Assert.ThrowsExactlyAsync<TaskCanceledException>(() => task);

            // Verify that some form of cancellation logging occurred
            _mockLogger.Verify(x => x.TrackEvent(
                It.Is<string>(s => s.Contains(taskName) && (s.Contains("TaskCanceledException") || s.Contains("IsCancellationRequested")))),
                Times.AtLeastOnce);
        }

        [TestMethod]
        public async Task LongRunningTask_WithZeroIterations_ShouldCompleteImmediately()
        {
            // Arrange
            const string taskName = "TestTaskZeroIterations";
            const int delay = 10;
            const int iterations = 0;
            const bool throwEx = false;
            using var cts = new CancellationTokenSource();

            // Act
            await AsyncMockService.LongRunningTask(taskName, delay, iterations, throwEx, _mockLogger.Object, cts.Token);

            // Assert
            _mockLogger.Verify(x => x.TrackEvent(It.Is<string>(s => s.Contains("TestTaskZeroIterations completed"))), Times.Once);
        }

        #endregion

        #region PerformTaskAsync Tests

        [TestMethod]
        public async Task PerformTaskAsync_WithValidParameters_ShouldCompleteSuccessfully()
        {
            // Arrange
            const string taskName = "TestPerformTask";
            const int delay = 50;
            const bool throwEx = false;
            using var cts = new CancellationTokenSource();

            // Act
            var stopwatch = Stopwatch.StartNew();
            await AsyncMockService.PerformTaskAsync(taskName, delay, throwEx, cts.Token);
            stopwatch.Stop();

            // Assert
            Assert.IsTrue(stopwatch.ElapsedMilliseconds >= delay - 10); // Allow some tolerance
        }

        [TestMethod]
        public async Task PerformTaskAsync_WithException_ShouldThrowException()
        {
            // Arrange
            const string taskName = "TestPerformTaskException";
            const int delay = 10;
            const bool throwEx = true;
            using var cts = new CancellationTokenSource();

            // Act & Assert
            var exception = await Assert.ThrowsExactlyAsync<Exception>(
                () => AsyncMockService.PerformTaskAsync(taskName, delay, throwEx, cts.Token));

            Assert.IsTrue(exception.Message.Contains("TestPerformTaskException PerformTaskAsync Exception"));
        }

        [TestMethod]
        public async Task PerformTaskAsync_WithCancellation_ShouldThrowTaskCancelledException()
        {
            // Arrange
            const string taskName = "TestPerformTaskCancelled";
            const int delay = 1000; // Long delay to ensure cancellation happens first
            const bool throwEx = false;
            using var cts = new CancellationTokenSource();

            // Act
            var task = AsyncMockService.PerformTaskAsync(taskName, delay, throwEx, cts.Token);

            // Cancel immediately
            cts.Cancel();

            // Assert
            await Assert.ThrowsExactlyAsync<TaskCanceledException>(() => task);
        }

        [TestMethod]
        public async Task PerformTaskAsync_WithDefaultCancellationToken_ShouldCompleteSuccessfully()
        {
            // Arrange
            const string taskName = "TestPerformTaskDefault";
            const int delay = 10;
            const bool throwEx = false;

            // Act & Assert (should not throw)
            await AsyncMockService.PerformTaskAsync(taskName, delay, throwEx);
        }

        [TestMethod]
        public async Task PerformTaskAsync_WithZeroDelay_ShouldCompleteImmediately()
        {
            // Arrange
            const string taskName = "TestPerformTaskZeroDelay";
            const int delay = 0;
            const bool throwEx = false;
            using var cts = new CancellationTokenSource();

            // Act
            var stopwatch = Stopwatch.StartNew();
            await AsyncMockService.PerformTaskAsync(taskName, delay, throwEx, cts.Token);
            stopwatch.Stop();

            // Assert
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 50); // Should complete very quickly
        }

        #endregion

        #region Cleanup

        [TestCleanup]
        public void Cleanup()
        {
            _mockLogger?.Reset();
        }

        #endregion
    }
}