using System.Security;

namespace WebSpark.HttpClientUtility.Authentication;

/// <summary>
/// Interface for authentication providers that can add authentication headers to HTTP requests.
/// Provides a contract for different authentication mechanisms like Bearer tokens, API keys, etc.
/// </summary>
public interface IAuthenticationProvider
{
    /// <summary>
    /// Gets the name of the authentication provider for logging and identification purposes.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets a value indicating whether the authentication provider is currently valid and can provide credentials.
    /// </summary>
    bool IsValid { get; }

    /// <summary>
    /// Adds authentication headers to the provided dictionary.
    /// </summary>
    /// <param name="headers">The headers dictionary to add authentication information to.</param>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="SecurityException">Thrown when authentication fails or credentials are invalid.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the provider is not properly configured.</exception>
    Task AddAuthenticationAsync(Dictionary<string, string> headers, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the current authentication configuration and credentials.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A task representing the asynchronous validation operation.</returns>
    /// <exception cref="SecurityException">Thrown when validation fails.</exception>
    Task ValidateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes authentication credentials if supported by the provider.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for async operations.</param>
    /// <returns>A task representing the asynchronous refresh operation.</returns>
    /// <exception cref="NotSupportedException">Thrown when the provider doesn't support credential refresh.</exception>
    Task RefreshAsync(CancellationToken cancellationToken = default);
}
