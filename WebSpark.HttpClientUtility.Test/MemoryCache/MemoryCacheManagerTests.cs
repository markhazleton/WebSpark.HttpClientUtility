using Microsoft.Extensions.Caching.Memory;
using WebSpark.HttpClientUtility.MemoryCache;

namespace WebSpark.HttpClientUtility.Test.MemoryCache;

[TestClass]
public class MemoryCacheManagerTests
{
    private IMemoryCache memoryCache = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        memoryCache = new Microsoft.Extensions.Caching.Memory.MemoryCache(new Microsoft.Extensions.Caching.Memory.MemoryCacheOptions());
    }

    private MemoryCacheManager CreateManager()
    {
        return new MemoryCacheManager(memoryCache);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        memoryCache.Dispose();
    }

    [TestMethod]
    public void Set_ShouldAddItemToCache()
    {
        var manager = CreateManager();
        var key = "test-key";
        var value = "test-value";
        manager.Set(key, value, 10);
        Assert.IsTrue(memoryCache.TryGetValue(key, out var cachedValue));
        Assert.AreEqual(value, cachedValue);
    }

    [TestMethod]
    public void Get_ShouldReturnCachedValue_WhenExists()
    {
        var manager = CreateManager();
        var key = "test-key";
        var value = "cached-value";
        memoryCache.Set(key, value);
        var result = manager.Get(key, () => "new-value", 10);
        Assert.AreEqual(value, result);
    }

    [TestMethod]
    public void Get_ShouldAcquireAndCache_WhenNotExists()
    {
        var manager = CreateManager();
        var key = "test-key";
        var result = manager.Get(key, () => "new-value", 10);
        Assert.AreEqual("new-value", result);
        Assert.IsTrue(memoryCache.TryGetValue(key, out var cachedValue));
        Assert.AreEqual("new-value", cachedValue);
    }

    [TestMethod]
    public void Remove_ShouldRemoveItemFromCache()
    {
        var manager = CreateManager();
        var key = "test-key";
        manager.Set(key, "value", 10);
        manager.Remove(key);
        Assert.IsFalse(memoryCache.TryGetValue(key, out _));
    }

    [TestMethod]
    public void Clear_ShouldRemoveAllItemsAndResetKeys()
    {
        var manager = CreateManager();
        var key1 = "key1";
        var key2 = "key2";
        manager.Set(key1, "v1", 10);
        manager.Set(key2, "v2", 10);
        manager.Clear();
        Assert.IsFalse(memoryCache.TryGetValue(key1, out _));
        Assert.IsFalse(memoryCache.TryGetValue(key2, out _));
        Assert.AreEqual(0, manager.GetKeys().Count);
    }

    [TestMethod]
    public void IsSet_ShouldReturnTrueIfExists()
    {
        var manager = CreateManager();
        var key = "test-key";
        memoryCache.Set(key, "exists");
        Assert.IsTrue(manager.IsSet(key));
    }

    [TestMethod]
    public void IsSet_ShouldReturnFalseIfNotExists()
    {
        var manager = CreateManager();
        var key = "test-key";
        Assert.IsFalse(manager.IsSet(key));
    }

    [TestMethod]
    public void GetKeys_ShouldReturnAllKeys()
    {
        var manager = CreateManager();
        manager.Set("k1", "v1", 10);
        manager.Set("k2", "v2", 10);
        var keys = manager.GetKeys();
        Assert.IsTrue(keys.Contains("k1"));
        Assert.IsTrue(keys.Contains("k2"));
    }

    [TestMethod]
    public void PerformActionWithLock_ShouldAcquireLockAndRunAction()
    {
        var manager = CreateManager();
        var key = "lock-key";
        bool actionCalled = false;
        var result = manager.PerformActionWithLock(key, TimeSpan.FromSeconds(1), () => actionCalled = true);
        Assert.IsTrue(result);
        Assert.IsTrue(actionCalled);
        Assert.IsFalse(memoryCache.TryGetValue(key, out _));
    }

    [TestMethod]
    public void PerformActionWithLock_ShouldReturnFalseIfLockExists()
    {
        var manager = CreateManager();
        var key = "lock-key";
        manager.Set(key, "v", 1);
        var result = manager.PerformActionWithLock(key, TimeSpan.FromSeconds(1), () => { });
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Dispose_ShouldDisposeCancellationToken()
    {
        var manager = CreateManager();
        manager.Dispose();
        // No exception means success
        Assert.IsTrue(true);
    }
}
