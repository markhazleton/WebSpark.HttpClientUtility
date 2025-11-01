using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace WebSpark.HttpClientUtility.ObjectPool;

/// <summary>
/// Object pool policy for StringBuilder instances to reduce allocation pressure when building strings.
/// Provides efficient reuse of StringBuilder objects with proper capacity management.
/// </summary>
public class StringBuilderPoolPolicy : IPooledObjectPolicy<StringBuilder>
{
    private readonly int _initialCapacity;
    private readonly int _maxCapacity;

    /// <summary>
    /// Initializes a new instance of the StringBuilderPoolPolicy class.
    /// </summary>
    /// <param name="initialCapacity">The initial capacity for new StringBuilder instances.</param>
    /// <param name="maxCapacity">The maximum capacity allowed before discarding the StringBuilder.</param>
    public StringBuilderPoolPolicy(int initialCapacity = 256, int maxCapacity = 8192)
    {
        _initialCapacity = initialCapacity;
        _maxCapacity = maxCapacity;
    }

    /// <summary>
    /// Creates a new StringBuilder instance with the configured initial capacity.
    /// </summary>
    /// <returns>A new StringBuilder instance.</returns>
    public StringBuilder Create()
    {
        return new StringBuilder(_initialCapacity);
    }

    /// <summary>
    /// Resets the StringBuilder for reuse by clearing its content.
    /// </summary>
    /// <param name="obj">The StringBuilder to reset.</param>
    /// <returns>True if the object was successfully reset and can be reused; otherwise, false.</returns>
    public bool Return(StringBuilder obj)
    {
        if (obj == null)
        {
            return false;
        }

        // Don't return to pool if capacity has grown too large
        if (obj.Capacity > _maxCapacity)
        {
            return false;
        }

        // Clear the content but keep the capacity for reuse
        obj.Clear();
        return true;
    }
}
