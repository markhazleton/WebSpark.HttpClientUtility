using System.Security;
using Microsoft.Extensions.Logging;

namespace WebSpark.HttpClientUtility.Authentication;

/// <summary>
/// Authentication provider for Bearer token authentication.
/// Provides secure handling of bearer tokens with automatic header injection.
/// </summary>
/// <remarks>
/// This provider adds an Authorization header with the format "Bearer {token}".
/// Tokens are validated before use and can be refreshed if a refresh callback is provided.
/// </remarks>
public class BearerTokenAuthenticationProvider : IAuthenticationProvider
{
    private readonly ILogger<BearerTokenAuthenticationProvider> _logger;
    private readonly Func<CancellationToken, Task<string>>? _tokenRefreshCallback;
    private string? _currentToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    /// <summary>
    /// Initializes a new instance of the BearerTokenAuthenticationProvider class with a static token.
    /// </summary>
    /// <param name="token">The bearer token to use for authentication.</param>
    /// <param name="logger">Logger instance for debugging and monitoring.</param>
    /// <exception cref="ArgumentException">Thrown when token is null or empty.</exception>
    public BearerTokenAuthenticationProvider(string token, ILogger<BearerTokenAuthenticationProvider> logger)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Bearer token cannot be null or empty.", nameof(token));
        }

        _currentToken = token;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tokenExpiry = DateTime.MaxValue; // Static tokens don't expire
    }

    /// <summary>
    /// Initializes a new instance of the BearerTokenAuthenticationProvider class with a token refresh callback.
    /// </summary>
    /// <param name="tokenRefreshCallback">Callback function to refresh the token when needed.</param>
    /// <param name="logger">Logger instance for debugging and monitoring.</param>
    /// <param name="initialToken">Optional initial token to use before the first refresh.</param>
    /// <param name="tokenExpiryDuration">Duration after which the token should be refreshed. Defaults to 55 minutes.</param>
    /// <exception cref="ArgumentNullException">Thrown when tokenRefreshCallback is null.</exception>
    public BearerTokenAuthenticationProvider(
        Func<CancellationToken, Task<string>> tokenRefreshCallback,
        ILogger<BearerTokenAuthenticationProvider> logger,
        string? initialToken = null,
        TimeSpan? tokenExpiryDuration = null)
    {
        _tokenRefreshCallback = tokenRefreshCallback ?? throw new ArgumentNullException(nameof(tokenRefreshCallback));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentToken = initialToken;

        // Default to 55 minutes to allow for refresh before typical 60-minute expiry
        var expiry = tokenExpiryDuration ?? TimeSpan.FromMinutes(55);
        _tokenExpiry = string.IsNullOrEmpty(initialToken) ? DateTime.MinValue : DateTime.UtcNow.Add(expiry);
    }

    /// <inheritdoc/>
    public string ProviderName => "BearerToken";

    /// <inheritdoc/>
    public bool IsValid => !string.IsNullOrWhiteSpace(_currentToken) && DateTime.UtcNow < _tokenExpiry;

    /// <inheritdoc/>
    public async Task AddAuthenticationAsync(Dictionary<string, string> headers, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(headers);

        // Ensure we have a valid token
        if (!IsValid)
        {
            if (_tokenRefreshCallback != null)
            {
                _logger.LogDebug("Token expired or invalid, attempting refresh");
                await RefreshAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new SecurityException("Bearer token is invalid or expired and no refresh mechanism is available.");
            }
        }

        if (string.IsNullOrWhiteSpace(_currentToken))
        {
            throw new SecurityException("Unable to obtain a valid bearer token for authentication.");
        }

        // Remove any existing Authorization header to prevent conflicts
        headers.Remove("Authorization");

        // Add the bearer token
        headers["Authorization"] = $"Bearer {_currentToken}";

        _logger.LogDebug("Bearer token authentication added to request headers");
    }

    /// <inheritdoc/>
    public async Task ValidateAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_currentToken))
        {
            if (_tokenRefreshCallback != null)
            {
                await RefreshAsync(cancellationToken).ConfigureAwait(false);
            }
            else
            {
                throw new SecurityException("No bearer token available and no refresh mechanism configured.");
            }
        }

        if (!IsValid)
        {
            throw new SecurityException("Bearer token validation failed - token is expired or invalid.");
        }

        _logger.LogDebug("Bearer token validation successful");
    }

    /// <inheritdoc/>
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (_tokenRefreshCallback == null)
        {
            throw new NotSupportedException("Token refresh is not supported for this bearer token provider instance.");
        }

        try
        {
            _logger.LogDebug("Refreshing bearer token");

            var newToken = await _tokenRefreshCallback(cancellationToken).ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(newToken))
            {
                throw new SecurityException("Token refresh callback returned null or empty token.");
            }

            _currentToken = newToken;
            _tokenExpiry = DateTime.UtcNow.Add(TimeSpan.FromMinutes(55)); // Reset expiry

            _logger.LogInformation("Bearer token refreshed successfully");
        }
        catch (Exception ex) when (ex is not SecurityException)
        {
            _logger.LogError(ex, "Failed to refresh bearer token");
            throw new SecurityException("Token refresh failed. See inner exception for details.", ex);
        }
    }
}
