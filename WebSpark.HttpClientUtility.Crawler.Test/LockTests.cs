using WebSpark.HttpClientUtility.Crawler;
using CrawlerLock = WebSpark.HttpClientUtility.Crawler.Lock;

namespace WebSpark.HttpClientUtility.Test.Crawler;

[TestClass]
public class LockTests
{
    [TestMethod]
    public async Task AcquireAsync_SameKey_SerializesAccess()
    {
        // Arrange
        const string key = "lock-key-1";
        using var first = await CrawlerLock.AcquireAsync(key);

        // Act
        var secondAcquireTask = CrawlerLock.TryAcquireAsync(key, TimeSpan.FromMilliseconds(50));
        var beforeRelease = await secondAcquireTask;

        // Assert (cannot acquire while first lock is held)
        Assert.IsFalse(beforeRelease.Acquired);
        Assert.IsNull(beforeRelease.Lock);

        // Act 2 (after release)
        first.Dispose();
        var afterRelease = await CrawlerLock.TryAcquireAsync(key, TimeSpan.FromMilliseconds(200));

        // Assert
        Assert.IsTrue(afterRelease.Acquired);
        Assert.IsNotNull(afterRelease.Lock);
        afterRelease.Lock!.Dispose();
    }

    [TestMethod]
    public async Task TryAcquireAsync_DifferentKeys_AllowsParallelAcquisition()
    {
        // Arrange
        const string key1 = "lock-key-2";
        const string key2 = "lock-key-3";

        // Act
        var result1 = await CrawlerLock.TryAcquireAsync(key1, TimeSpan.FromMilliseconds(100));
        var result2 = await CrawlerLock.TryAcquireAsync(key2, TimeSpan.FromMilliseconds(100));

        // Assert
        Assert.IsTrue(result1.Acquired);
        Assert.IsNotNull(result1.Lock);
        Assert.IsTrue(result2.Acquired);
        Assert.IsNotNull(result2.Lock);

        result1.Lock!.Dispose();
        result2.Lock!.Dispose();
    }

    [TestMethod]
    public void CleanupUnusedLocks_RemovesUnlockedEntries()
    {
        // Arrange
        const string key = "cleanup-key";
        var sem = CrawlerLock.GetLock(key);
        Assert.IsNotNull(sem);

        // Act
        CrawlerLock.CleanupUnusedLocks();
        var afterCleanup = CrawlerLock.GetLock(key);

        // Assert
        Assert.IsNotNull(afterCleanup);
        Assert.AreNotSame(sem, afterCleanup);
    }
}
