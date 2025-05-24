using Microsoft.Extensions.Logging;
using System.Security;

namespace WebSpark.HttpClientUtility.Authentication;

/// <summary>
/// Authentication provider for basic HTTP authentication using username and password.
/// Implements RFC 7617 Basic Authentication scheme.
/// </summary>
/// <remarks>
/// This provider encodes credentials using Base64 and adds an Authorization header 
/// with the format "Basic {base64EncodedCredentials}".
/// Note: Basic authentication transmits credentials in an easily decoded format, 
/// so it should only be used over HTTPS connections.
/// </remarks>
public class BasicAuthenticationProvider : IAuthenticationProvider
{
    private readonly ILogger<BasicAuthenticationProvider> _logger;
    private readonly string _username;
    private readonly string _password;
    private readonly string _encodedCredentials;

    /// <summary>
    /// Initializes a new instance of the BasicAuthenticationProvider class.
    /// </summary>
    /// <param name="username">The username for authentication.</param>
    /// <param name="password">The password for authentication.</param>
    /// <param name="logger">Logger instance for debugging and monitoring.</param>
    /// <exception cref="ArgumentException">Thrown when username or password is null or empty.</exception>
    public BasicAuthenticationProvider(string username, string password, ILogger<BasicAuthenticationProvider> logger)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or empty.", nameof(username));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));

        _username = username;
        _password = password;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Pre-encode credentials for efficiency
        var credentials = $"{username}:{password}";
        var credentialsBytes = System.Text.Encoding.UTF8.GetBytes(credentials);
        _encodedCredentials = Convert.ToBase64String(credentialsBytes);
    }

    /// <inheritdoc/>
    public string ProviderName => "BasicAuth";

    /// <inheritdoc/>
    public bool IsValid => !string.IsNullOrWhiteSpace(_username) && !string.IsNullOrWhiteSpace(_password);

    /// <inheritdoc/>
    public Task AddAuthenticationAsync(Dictionary<string, string> headers, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(headers);

        if (!IsValid)
        {
            throw new SecurityException("Basic authentication credentials are invalid or not configured.");
        }

        // Remove any existing Authorization header to prevent conflicts
        headers.Remove("Authorization");

        // Add the basic authentication header
        headers["Authorization"] = $"Basic {_encodedCredentials}";

        _logger.LogDebug("Basic authentication added to request headers for user: {Username}", _username);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task ValidateAsync(CancellationToken cancellationToken = default)
    {
        if (!IsValid)
        {
            throw new SecurityException("Basic authentication validation failed - username or password is null or empty.");
        }

        _logger.LogDebug("Basic authentication validation successful for user: {Username}", _username);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException("Basic authentication refresh is not supported. Credentials are static.");
    }
}
