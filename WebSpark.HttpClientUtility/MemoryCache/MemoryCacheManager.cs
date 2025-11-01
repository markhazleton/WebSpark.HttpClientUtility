using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace WebSpark.HttpClientUtility.MemoryCache;

/// <summary>
/// Provides advanced management capabilities for memory caching operations.
/// </summary>
/// <remarks>
/// This class extends the basic IMemoryCache functionality with additional features:
/// - Tracking of all cached keys
/// - Global cache clear capabilities
/// - Automatic cleanup of expired entries
/// - Atomic operations with locking
/// </remarks>
/// <param name="cache">The memory cache implementation</param>
public class MemoryCacheManager(IMemoryCache cache) : IMemoryCacheManager, IDisposable
{
    /// <summary>
    /// All keys of cache
    /// </summary>
    /// <remarks>Dictionary value indicating whether a key still exists in cache</remarks> 
    protected readonly ConcurrentDictionary<string, bool> _allKeys = new();

    /// <summary>
    /// Cancellation token for clear cache
    /// </summary>
    protected CancellationTokenSource _cancellationTokenSource = new();

    /// <summary>
    /// Remove all keys marked as not existing
    /// </summary>
    private void ClearKeys()
    {
        foreach (var key in _allKeys.Where(p => !p.Value).Select(p => p.Key).ToList())
        {
            RemoveKey(key);
        }
    }

    /// <summary>
    /// Post eviction callback that maintains the keys dictionary when items are removed from cache.
    /// </summary>
    /// <param name="key">Key of cached item</param>
    /// <param name="value">Value of cached item</param>
    /// <param name="reason">Eviction reason</param>
    /// <param name="state">State</param>
    private void PostEviction(object key, object? value, EvictionReason reason, object? state)
    {
        // if cached item just changed, then do nothing
        if (reason == EvictionReason.Replaced)
        {
            return;
        }

        // try to remove all keys marked as not existing
        ClearKeys();

        // try to remove this key from dictionary
        TryRemoveKey(key?.ToString() ?? string.Empty);
    }

    /// <summary>
    /// Add key to dictionary
    /// </summary>
    /// <param name="key">Key of cached item</param>
    /// <returns>Itself key</returns>
    protected string AddKey(string key)
    {
        _allKeys.TryAdd(key, true);
        return key;
    }

    /// <summary>
    /// Create entry options for item of memory cache
    /// </summary>
    /// <param name="cacheTime">Cache time</param>
    /// <returns>Configured MemoryCacheEntryOptions</returns>
    protected MemoryCacheEntryOptions GetMemoryCacheEntryOptions(TimeSpan cacheTime)
    {
        var options = new MemoryCacheEntryOptions()
            // add cancellation token for clear cache
            .AddExpirationToken(new CancellationChangeToken(_cancellationTokenSource.Token))
            // add post eviction callback
            .RegisterPostEvictionCallback(PostEviction);

        // set cache time
        options.AbsoluteExpirationRelativeToNow = cacheTime;

        return options;
    }

    /// <summary>
    /// Remove key from dictionary
    /// </summary>
    /// <param name="key">Key of cached item</param>
    /// <returns>Itself key</returns>
    protected string RemoveKey(string key)
    {
        TryRemoveKey(key);
        return key;
    }

    /// <summary>
    /// Try to remove a key from dictionary, or mark a key as not existing in cache
    /// </summary>
    /// <param name="key">Key of cached item</param>
    protected void TryRemoveKey(string key)
    {
        // try to remove key from dictionary
        if (!_allKeys.TryRemove(key, out _))
        {
            // if not possible to remove key from dictionary, then try to mark key as not existing in cache
            _allKeys.TryUpdate(key, false, true);
        }
    }

    /// <summary>
    /// Clear all cache data
    /// </summary>
    /// <remarks>
    /// This method will remove all items from the cache and reset the cancellation token
    /// to ensure any cached items using the token will be invalidated.
    /// </remarks>
    public virtual void Clear()
    {
        foreach (var key in _allKeys.Keys.ToList())
        {
            cache.Remove(key);
        }
        _allKeys.Clear();        // Safely dispose the old token source
        var oldTokenSource = _cancellationTokenSource;
        _cancellationTokenSource = new CancellationTokenSource();
        oldTokenSource.Cancel();
        oldTokenSource.Dispose();
    }

    /// <summary>
    /// Get a cached item. If it's not in the cache yet, then load and cache it
    /// </summary>
    /// <typeparam name="T">Type of cached item</typeparam>
    /// <param name="key">Cache key</param>
    /// <param name="acquire">Function to load item if it's not in the cache yet</param>
    /// <param name="cacheTime">Cache time in minutes; pass 0 to not cache; pass null to use the default time (30 minutes)</param>
    /// <returns>The cached value associated with the specified key</returns>
    /// <remarks>
    /// This method implements the "get or add" pattern, where an item is retrieved from cache if available,
    /// or created using the provided factory method and added to the cache if not available.
    /// </remarks>
    public virtual T Get<T>(string key, Func<T> acquire, int? cacheTime = null)
    {
        // item already is in cache, so return it
        if (cache.TryGetValue(key, out T? value) && value is not null)
        {
            return value;
        }

        // or create it using passed function
        var result = acquire();

        // and set in cache (if cache time is defined)
        if ((cacheTime ?? 30) > 0 && result is not null)
        {
            Set(key, result, cacheTime ?? 30);
        }

        return result;
    }

    /// <summary>
    /// Get All Keys currently in the cache
    /// </summary>
    /// <returns>A list of all cache keys</returns>
    /// <remarks>
    /// This method returns all keys tracked by the cache manager, which may include
    /// keys that have been marked for removal but not yet removed.
    /// </remarks>
    public IList<string> GetKeys() => _allKeys.Keys.ToList();

    /// <summary>
    /// Gets a value indicating whether the value associated with the specified key is cached
    /// </summary>
    /// <param name="key">Key of cached item</param>
    /// <returns>True if item already is in cache; otherwise false</returns>
    public virtual bool IsSet(string key)
    {
        return cache.TryGetValue(key, out object? _); // Use nullable object type to address CS8601
    }

    /// <summary>
    /// Perform some action with exclusive in-memory lock
    /// </summary>
    /// <param name="key">The key we are locking on</param>
    /// <param name="expirationTime">The time after which the lock will automatically expire</param>
    /// <param name="action">Action to be performed with locking</param>
    /// <returns>True if lock was acquired and action was performed; otherwise false</returns>
    /// <remarks>
    /// This method implements a simple in-memory lock using the cache itself.
    /// It's useful for coordinating access to resources across multiple processes or threads.
    /// The lock automatically expires after the specified time to prevent deadlocks.
    /// </remarks>
    public bool PerformActionWithLock(string key, TimeSpan expirationTime, Action action)
    {
        // ensure that lock is acquired
        if (!_allKeys.TryAdd(key, true))
        {
            return false;
        }

        try
        {
            cache.Set(key, key, GetMemoryCacheEntryOptions(expirationTime));

            // perform action
            action();

            return true;
        }
        finally
        {
            // release lock even if action fails
            Remove(key);
        }
    }

    /// <summary>
    /// Removes the value with the specified key from the cache
    /// </summary>
    /// <param name="key">Key of cached item</param>
    /// <remarks>
    /// This method removes the item from both the underlying cache and the tracking dictionary.
    /// </remarks>
    public virtual void Remove(string key)
    {
        cache.Remove(RemoveKey(key));
    }

    /// <summary>
    /// Adds the specified key and object to the cache
    /// </summary>
    /// <param name="key">Key of cached item</param>
    /// <param name="data">Value for caching</param>
    /// <param name="cacheTimeMinutes">Cache time in minutes</param>
    /// <remarks>
    /// This method adds an item to the cache with the specified expiration time in minutes.
    /// It also adds the key to the tracking dictionary for management purposes.    /// </remarks>
    public virtual void Set(string key, object data, int cacheTimeMinutes)
    {
        if (data is not null)
        {
            cache.Set(AddKey(key), data, GetMemoryCacheEntryOptions(TimeSpan.FromMinutes(cacheTimeMinutes)));
        }
    }

    /// <summary>
    /// Disposes of the MemoryCacheManager and cleans up resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected implementation of Dispose pattern.
    /// </summary>
    /// <param name="disposing">true if called from Dispose(); false if called from finalizer</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }
    }
}
