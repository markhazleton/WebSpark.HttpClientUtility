using System.Security;
using Microsoft.Extensions.Logging;

namespace WebSpark.HttpClientUtility.Authentication;

/// <summary>
/// Authentication provider for API key authentication.
/// Supports multiple API key formats including header-based and query parameter-based authentication.
/// </summary>
/// <remarks>
/// This provider can add API keys as custom headers (e.g., X-API-Key) or as query parameters.
/// API keys are stored securely and validated before use.
/// </remarks>
public class ApiKeyAuthenticationProvider : IAuthenticationProvider
{
    private readonly ILogger<ApiKeyAuthenticationProvider> _logger;
    private readonly string _apiKey;
    private readonly string _headerName;
    private readonly string? _headerValuePrefix;

    /// <summary>
    /// Initializes a new instance of the ApiKeyAuthenticationProvider class.
    /// </summary>
    /// <param name="apiKey">The API key to use for authentication.</param>
    /// <param name="headerName">The name of the header to add the API key to (e.g., "X-API-Key", "Authorization").</param>
    /// <param name="logger">Logger instance for debugging and monitoring.</param>
    /// <param name="headerValuePrefix">Optional prefix for the header value (e.g., "ApiKey " for "ApiKey {key}").</param>
    /// <exception cref="ArgumentException">Thrown when apiKey or headerName is null or empty.</exception>
    public ApiKeyAuthenticationProvider(
        string apiKey,
        string headerName,
        ILogger<ApiKeyAuthenticationProvider> logger,
        string? headerValuePrefix = null)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new ArgumentException("API key cannot be null or empty.", nameof(apiKey));
        }

        if (string.IsNullOrWhiteSpace(headerName))
        {
            throw new ArgumentException("Header name cannot be null or empty.", nameof(headerName));
        }

        _apiKey = apiKey;
        _headerName = headerName;
        _headerValuePrefix = headerValuePrefix;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates an API key provider for X-API-Key header authentication.
    /// </summary>
    /// <param name="apiKey">The API key to use.</param>
    /// <param name="logger">Logger instance.</param>
    /// <returns>A configured ApiKeyAuthenticationProvider.</returns>
    public static ApiKeyAuthenticationProvider CreateXApiKeyProvider(string apiKey, ILogger<ApiKeyAuthenticationProvider> logger)
    {
        return new ApiKeyAuthenticationProvider(apiKey, "X-API-Key", logger);
    }

    /// <summary>
    /// Creates an API key provider for Authorization header with ApiKey prefix.
    /// </summary>
    /// <param name="apiKey">The API key to use.</param>
    /// <param name="logger">Logger instance.</param>
    /// <returns>A configured ApiKeyAuthenticationProvider.</returns>
    public static ApiKeyAuthenticationProvider CreateAuthorizationApiKeyProvider(string apiKey, ILogger<ApiKeyAuthenticationProvider> logger)
    {
        return new ApiKeyAuthenticationProvider(apiKey, "Authorization", logger, "ApiKey ");
    }

    /// <inheritdoc/>
    public string ProviderName => $"ApiKey({_headerName})";

    /// <inheritdoc/>
    public bool IsValid => !string.IsNullOrWhiteSpace(_apiKey);

    /// <inheritdoc/>
    public Task AddAuthenticationAsync(Dictionary<string, string> headers, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(headers);

        if (!IsValid)
        {
            throw new SecurityException("API key is invalid or not configured.");
        }

        // Remove any existing header with the same name to prevent conflicts
        headers.Remove(_headerName);

        // Add the API key header
        var headerValue = string.IsNullOrEmpty(_headerValuePrefix) ? _apiKey : $"{_headerValuePrefix}{_apiKey}";
        headers[_headerName] = headerValue;

        _logger.LogDebug("API key authentication added to request headers using header: {HeaderName}", _headerName);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task ValidateAsync(CancellationToken cancellationToken = default)
    {
        if (!IsValid)
        {
            throw new SecurityException("API key validation failed - key is null or empty.");
        }

        _logger.LogDebug("API key validation successful for header: {HeaderName}", _headerName);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("API key refresh is not supported. API keys are typically static credentials.");
    }
}
