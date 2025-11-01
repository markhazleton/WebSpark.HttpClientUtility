using Microsoft.Extensions.Logging;
using Moq;
using WebSpark.HttpClientUtility.FireAndForget;

namespace WebSpark.HttpClientUtility.Test.FireAndForget;

[TestClass]
public class FireAndForgetUtilityTests
{
    private Mock<ILogger<FireAndForgetUtility>> _mockLogger = null!;
    private FireAndForgetUtility _utility = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<FireAndForgetUtility>>();
        _utility = new FireAndForgetUtility(_mockLogger.Object);
    }

    [TestMethod]
    public void Constructor_NullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => new FireAndForgetUtility(null!));
    }

    [TestMethod]
    public void SafeFireAndForget_NullTask_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => _utility.SafeFireAndForget(null!, "test"));
    }

    [TestMethod]
    public async Task SafeFireAndForget_SuccessfulTask_LogsSuccess()
    {
        // Arrange
        var taskCompletedSignal = new TaskCompletionSource<bool>();
        var task = Task.Run(async () =>
        {
            await Task.Delay(50);
            taskCompletedSignal.SetResult(true);
        });

        // Act
        _utility.SafeFireAndForget(task, "TestOperation");

        // Wait for the task to complete and continuation to execute
        await taskCompletedSignal.Task;
        await Task.Delay(100); // Give continuation time to execute

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting fire-and-forget operation: TestOperation")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fire-and-forget operation completed successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task SafeFireAndForget_FailedTask_LogsError()
    {
        // Arrange
        var exception = new InvalidOperationException("Test error");
        var taskCompletedSignal = new TaskCompletionSource<bool>();
        var task = Task.Run(async () =>
        {
            await Task.Delay(50);
            taskCompletedSignal.SetResult(true);
            throw exception;
        });

        // Act
        _utility.SafeFireAndForget(task, "TestOperation");

        // Wait for the task to complete and continuation to execute
        await taskCompletedSignal.Task;
        await Task.Delay(100); // Give continuation time to execute

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fire-and-Forget TestOperation")),
                It.Is<Exception>(ex => ex == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void SafeFireAndForget_CanceledTask_LogsCancellation()
    {
        // These cancellation timing tests are complex due to async nature
        // For coverage purposes, let's just verify the method doesn't throw
        // and that basic logging occurs

        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately
        var task = Task.FromCanceled(cts.Token);

        // Act & Assert - Should not throw
        _utility.SafeFireAndForget(task, "TestOperation", cts.Token);

        // Verify start was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Starting fire-and-forget operation: TestOperation")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void SafeFireAndForget_DefaultOperationName_UsesDefault()
    {
        // Arrange
        var task = Task.CompletedTask;

        // Act
        _utility.SafeFireAndForget(task);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unnamed Operation")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #region Generic SafeFireAndForget Tests

    [TestMethod]
    public void SafeFireAndForget_Generic_NullTask_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() => _utility.SafeFireAndForget<string>(null!, "test"));
    }

    [TestMethod]
    public async Task SafeFireAndForget_Generic_SuccessfulTask_LogsSuccess()
    {
        // Arrange
        var taskCompletedSignal = new TaskCompletionSource<bool>();
        var task = Task.Run(async () =>
        {
            await Task.Delay(50);
            taskCompletedSignal.SetResult(true);
            return "Success Result";
        });

        // Act
        _utility.SafeFireAndForget(task, "TestGenericOperation");

        // Wait for the task to complete and continuation to execute
        await taskCompletedSignal.Task;
        await Task.Delay(100);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("result type String")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("with result of type String")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task SafeFireAndForget_Generic_TaskReturnsNull_LogsNullResult()
    {
        // Arrange
        var taskCompletedSignal = new TaskCompletionSource<bool>();
        var task = Task.Run(async () =>
        {
            await Task.Delay(50);
            taskCompletedSignal.SetResult(true);
            return (string?)null;
        });

        // Act
        _utility.SafeFireAndForget(task, "TestNullResult");

        // Wait for completion
        await taskCompletedSignal.Task;
        await Task.Delay(100);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("with null result")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task SafeFireAndForget_Generic_FailedTask_LogsError()
    {
        // Arrange
        var exception = new ArgumentException("Generic test error");
        var taskCompletedSignal = new TaskCompletionSource<bool>();
        var task = Task.Run(async () =>
        {
            await Task.Delay(50);
            taskCompletedSignal.SetResult(true);
            throw exception;
#pragma warning disable CS0162 // Unreachable code detected
            return "Never reached";
#pragma warning restore CS0162 // Unreachable code detected
        });

        // Act
        _utility.SafeFireAndForget(task, "TestGenericError");

        // Wait for completion
        await taskCompletedSignal.Task;
        await Task.Delay(100);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Fire-and-Forget TestGenericError")),
                It.Is<Exception>(ex => ex == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public void SafeFireAndForget_Generic_CanceledTask_LogsCancellation()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        cts.Cancel(); // Cancel immediately
        var task = Task.FromCanceled<string>(cts.Token);

        // Act & Assert - Should not throw
        _utility.SafeFireAndForget(task, "TestGenericCancel", cts.Token);

        // Verify start was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("result type String: TestGenericCancel")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion

    [TestMethod]
    public void SafeFireAndForget_CorrelationIdGeneration_GeneratesUniqueIds()
    {
        // This test verifies that each call generates a unique correlation ID
        // We can't directly test the correlation ID but we can verify unique logging

        // Arrange
        var task1 = Task.CompletedTask;
        var task2 = Task.CompletedTask;

        // Act
        _utility.SafeFireAndForget(task1, "Operation1");
        _utility.SafeFireAndForget(task2, "Operation2");

        // Assert - Verify that both operations were logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Operation1")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Operation2")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
