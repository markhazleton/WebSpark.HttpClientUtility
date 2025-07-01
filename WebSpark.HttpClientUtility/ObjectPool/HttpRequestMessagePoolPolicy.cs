using Microsoft.Extensions.ObjectPool;
using System;

namespace WebSpark.HttpClientUtility.ObjectPool;

/// <summary>
/// Object pool policy for HttpRequestMessage instances to reduce allocation pressure.
/// Provides efficient reuse of HttpRequestMessage objects with proper cleanup.
/// </summary>
public class HttpRequestMessagePoolPolicy : IPooledObjectPolicy<HttpRequestMessage>
{
    /// <summary>
    /// Creates a new HttpRequestMessage instance.
    /// </summary>
    /// <returns>A new HttpRequestMessage instance.</returns>
    public HttpRequestMessage Create()
    {
        return new HttpRequestMessage();
    }

    /// <summary>
    /// Resets the HttpRequestMessage for reuse by clearing all properties and headers.
    /// </summary>
    /// <param name="obj">The HttpRequestMessage to reset.</param>
    /// <returns>True if the object was successfully reset and can be reused; otherwise, false.</returns>
    public bool Return(HttpRequestMessage obj)
    {
        if (obj == null)
            return false;

        try
        {
            // Clear all headers
            obj.Headers.Clear();

            // Dispose and clear content
            obj.Content?.Dispose();
            obj.Content = null;

            // Reset properties to defaults
            obj.Method = HttpMethod.Get;
            obj.RequestUri = null;
            obj.Version = new Version(1, 1);            // Clear any options (HttpRequestOptions.Clear() is not available in all .NET versions)
                                                        // For pooling scenarios, leaving options as-is is generally acceptable

            return true;
        }
        catch
        {
            // If we can't reset the object safely, don't return it to the pool
            return false;
        }
    }
}
