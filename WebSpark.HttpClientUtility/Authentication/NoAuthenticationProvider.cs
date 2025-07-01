using Microsoft.Extensions.Logging;

namespace WebSpark.HttpClientUtility.Authentication;

/// <summary>
/// A no-operation authentication provider that doesn't add any authentication.
/// Useful for endpoints that don't require authentication or as a default fallback.
/// </summary>
public class NoAuthenticationProvider : IAuthenticationProvider
{
    private readonly ILogger<NoAuthenticationProvider> _logger;

    /// <summary>
    /// Initializes a new instance of the NoAuthenticationProvider class.
    /// </summary>
    /// <param name="logger">Logger instance for debugging and monitoring.</param>
    public NoAuthenticationProvider(ILogger<NoAuthenticationProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc/>
    public string ProviderName => "None";

    /// <inheritdoc/>
    public bool IsValid => true; // Always valid since no authentication is required

    /// <inheritdoc/>
    public Task AddAuthenticationAsync(Dictionary<string, string> headers, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(headers);

        _logger.LogDebug("No authentication provider - no headers added");
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task ValidateAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("No authentication provider - validation always succeeds");
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("No authentication provider - refresh not required");
        return Task.CompletedTask;
    }
}
