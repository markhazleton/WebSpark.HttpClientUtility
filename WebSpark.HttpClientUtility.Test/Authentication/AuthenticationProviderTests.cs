using Microsoft.Extensions.Logging;
using Moq;
using WebSpark.HttpClientUtility.Authentication;

namespace WebSpark.HttpClientUtility.Test.Authentication;

[TestClass]
public class AuthenticationProviderTests
{
    private Mock<ILogger<ApiKeyAuthenticationProvider>>? _apiKeyLogger;
    private Mock<ILogger<BasicAuthenticationProvider>>? _basicLogger;
    private Mock<ILogger<BearerTokenAuthenticationProvider>>? _bearerLogger;
    private Mock<ILogger<NoAuthenticationProvider>>? _noAuthLogger;

    [TestInitialize]
    public void Setup()
    {
        _apiKeyLogger = new Mock<ILogger<ApiKeyAuthenticationProvider>>();
        _basicLogger = new Mock<ILogger<BasicAuthenticationProvider>>();
        _bearerLogger = new Mock<ILogger<BearerTokenAuthenticationProvider>>();
        _noAuthLogger = new Mock<ILogger<NoAuthenticationProvider>>();
    }

    #region ApiKeyAuthenticationProvider Tests

    [TestMethod]
    public void ApiKeyAuthenticationProvider_Constructor_ValidParameters_Succeeds()
    {
        // Arrange & Act
        var provider = new ApiKeyAuthenticationProvider("test-key", "X-API-Key", _apiKeyLogger!.Object);

        // Assert
        Assert.AreEqual("ApiKey(X-API-Key)", provider.ProviderName);
        Assert.IsTrue(provider.IsValid);
    }

    [TestMethod]
    public void ApiKeyAuthenticationProvider_Constructor_NullApiKey_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() =>
            new ApiKeyAuthenticationProvider(null!, "X-API-Key", _apiKeyLogger!.Object));
    }

    [TestMethod]
    public void ApiKeyAuthenticationProvider_Constructor_EmptyHeaderName_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() =>
            new ApiKeyAuthenticationProvider("test-key", "", _apiKeyLogger!.Object));
    }

    [TestMethod]
    public async Task ApiKeyAuthenticationProvider_AddAuthenticationAsync_AddsCorrectHeader()
    {
        // Arrange
        var provider = new ApiKeyAuthenticationProvider("test-key", "X-API-Key", _apiKeyLogger!.Object);
        var headers = new Dictionary<string, string>();

        // Act
        await provider.AddAuthenticationAsync(headers);

        // Assert
        Assert.IsTrue(headers.ContainsKey("X-API-Key"));
        Assert.AreEqual("test-key", headers["X-API-Key"]);
    }

    [TestMethod]
    public async Task ApiKeyAuthenticationProvider_AddAuthenticationAsync_WithPrefix_AddsCorrectHeader()
    {
        // Arrange
        var provider = new ApiKeyAuthenticationProvider("test-key", "Authorization", _apiKeyLogger!.Object, "ApiKey ");
        var headers = new Dictionary<string, string>();

        // Act
        await provider.AddAuthenticationAsync(headers);

        // Assert
        Assert.IsTrue(headers.ContainsKey("Authorization"));
        Assert.AreEqual("ApiKey test-key", headers["Authorization"]);
    }

    [TestMethod]
    public void ApiKeyAuthenticationProvider_CreateXApiKeyProvider_CreatesCorrectProvider()
    {
        // Act
        var provider = ApiKeyAuthenticationProvider.CreateXApiKeyProvider("test-key", _apiKeyLogger!.Object);

        // Assert
        Assert.AreEqual("ApiKey(X-API-Key)", provider.ProviderName);
        Assert.IsTrue(provider.IsValid);
    }

    [TestMethod]
    public void ApiKeyAuthenticationProvider_CreateAuthorizationApiKeyProvider_CreatesCorrectProvider()
    {
        // Act
        var provider = ApiKeyAuthenticationProvider.CreateAuthorizationApiKeyProvider("test-key", _apiKeyLogger!.Object);

        // Assert
        Assert.AreEqual("ApiKey(Authorization)", provider.ProviderName);
        Assert.IsTrue(provider.IsValid);
    }

    [TestMethod]
    public async Task ApiKeyAuthenticationProvider_ValidateAsync_ValidKey_Succeeds()
    {
        // Arrange
        var provider = new ApiKeyAuthenticationProvider("test-key", "X-API-Key", _apiKeyLogger!.Object);

        // Act & Assert
        await provider.ValidateAsync();
    }

    [TestMethod]
    public async Task ApiKeyAuthenticationProvider_RefreshAsync_ThrowsNotSupportedException()
    {
        // Arrange
        var provider = new ApiKeyAuthenticationProvider("test-key", "X-API-Key", _apiKeyLogger!.Object);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => provider.RefreshAsync());
    }

    #endregion

    #region BasicAuthenticationProvider Tests

    [TestMethod]
    public void BasicAuthenticationProvider_Constructor_ValidCredentials_Succeeds()
    {
        // Arrange & Act
        var provider = new BasicAuthenticationProvider("username", "password", _basicLogger!.Object);

        // Assert
        Assert.AreEqual("BasicAuth", provider.ProviderName);
        Assert.IsTrue(provider.IsValid);
    }

    [TestMethod]
    public void BasicAuthenticationProvider_Constructor_NullUsername_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() =>
            new BasicAuthenticationProvider(null!, "password", _basicLogger!.Object));
    }

    [TestMethod]
    public void BasicAuthenticationProvider_Constructor_EmptyPassword_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() =>
            new BasicAuthenticationProvider("username", "", _basicLogger!.Object));
    }

    [TestMethod]
    public async Task BasicAuthenticationProvider_AddAuthenticationAsync_AddsCorrectHeader()
    {
        // Arrange
        var provider = new BasicAuthenticationProvider("testuser", "testpass", _basicLogger!.Object);
        var headers = new Dictionary<string, string>();

        // Act
        await provider.AddAuthenticationAsync(headers);

        // Assert
        Assert.IsTrue(headers.ContainsKey("Authorization"));
        Assert.IsTrue(headers["Authorization"].StartsWith("Basic "));

        // Verify the base64 encoding is correct
        var expectedCredentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("testuser:testpass"));
        Assert.AreEqual($"Basic {expectedCredentials}", headers["Authorization"]);
    }

    [TestMethod]
    public async Task BasicAuthenticationProvider_ValidateAsync_ValidCredentials_Succeeds()
    {
        // Arrange
        var provider = new BasicAuthenticationProvider("username", "password", _basicLogger!.Object);

        // Act & Assert
        await provider.ValidateAsync();
    }

    [TestMethod]
    public async Task BasicAuthenticationProvider_RefreshAsync_ThrowsNotSupportedException()
    {
        // Arrange
        var provider = new BasicAuthenticationProvider("username", "password", _basicLogger!.Object);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => provider.RefreshAsync());
    }

    #endregion

    #region BearerTokenAuthenticationProvider Tests

    [TestMethod]
    public void BearerTokenAuthenticationProvider_Constructor_ValidToken_Succeeds()
    {
        // Arrange & Act
        var provider = new BearerTokenAuthenticationProvider("test-token", _bearerLogger!.Object);

        // Assert
        Assert.AreEqual("BearerToken", provider.ProviderName);
        Assert.IsTrue(provider.IsValid);
    }

    [TestMethod]
    public void BearerTokenAuthenticationProvider_Constructor_EmptyToken_ThrowsArgumentException()
    {
        // Act & Assert
        Assert.ThrowsExactly<ArgumentException>(() =>
            new BearerTokenAuthenticationProvider("", _bearerLogger!.Object));
    }

    [TestMethod]
    public async Task BearerTokenAuthenticationProvider_AddAuthenticationAsync_AddsCorrectHeader()
    {
        // Arrange
        var provider = new BearerTokenAuthenticationProvider("test-token", _bearerLogger!.Object);
        var headers = new Dictionary<string, string>();

        // Act
        await provider.AddAuthenticationAsync(headers);

        // Assert
        Assert.IsTrue(headers.ContainsKey("Authorization"));
        Assert.AreEqual("Bearer test-token", headers["Authorization"]);
    }

    [TestMethod]
    public async Task BearerTokenAuthenticationProvider_WithRefreshCallback_RefreshesToken()
    {
        // Arrange
        var refreshCallbackInvoked = false;
        Task<string> RefreshCallback(CancellationToken ct)
        {
            refreshCallbackInvoked = true;
            return Task.FromResult("new-token");
        }

        var provider = new BearerTokenAuthenticationProvider(RefreshCallback, _bearerLogger!.Object);

        // Act
        await provider.RefreshAsync();

        // Assert
        Assert.IsTrue(refreshCallbackInvoked);
        Assert.IsTrue(provider.IsValid);
    }

    [TestMethod]
    public async Task BearerTokenAuthenticationProvider_RefreshAsync_StaticToken_ThrowsNotSupportedException()
    {
        // Arrange
        var provider = new BearerTokenAuthenticationProvider("test-token", _bearerLogger!.Object);

        // Act & Assert
        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => provider.RefreshAsync());
    }

    #endregion

    #region NoAuthenticationProvider Tests

    [TestMethod]
    public void NoAuthenticationProvider_Constructor_ValidLogger_Succeeds()
    {
        // Arrange & Act
        var provider = new NoAuthenticationProvider(_noAuthLogger!.Object);

        // Assert
        Assert.AreEqual("None", provider.ProviderName);
        Assert.IsTrue(provider.IsValid);
    }

    [TestMethod]
    public async Task NoAuthenticationProvider_AddAuthenticationAsync_DoesNotModifyHeaders()
    {
        // Arrange
        var provider = new NoAuthenticationProvider(_noAuthLogger!.Object);
        var headers = new Dictionary<string, string> { { "Existing", "Header" } };
        var originalCount = headers.Count;

        // Act
        await provider.AddAuthenticationAsync(headers);

        // Assert
        Assert.AreEqual(originalCount, headers.Count);
        Assert.IsTrue(headers.ContainsKey("Existing"));
    }

    [TestMethod]
    public async Task NoAuthenticationProvider_ValidateAsync_AlwaysSucceeds()
    {
        // Arrange
        var provider = new NoAuthenticationProvider(_noAuthLogger!.Object);

        // Act & Assert
        await provider.ValidateAsync();
    }

    [TestMethod]
    public async Task NoAuthenticationProvider_RefreshAsync_AlwaysSucceeds()
    {
        // Arrange
        var provider = new NoAuthenticationProvider(_noAuthLogger!.Object);

        // Act & Assert
        await provider.RefreshAsync();
    }

    #endregion

    #region General Tests

    [TestMethod]
    public async Task AllProviders_AddAuthenticationAsync_NullHeaders_ThrowsArgumentNullException()
    {
        // Arrange
        var providers = new IAuthenticationProvider[]
        {
            new ApiKeyAuthenticationProvider("key", "header", _apiKeyLogger!.Object),
            new BasicAuthenticationProvider("user", "pass", _basicLogger!.Object),
            new BearerTokenAuthenticationProvider("token", _bearerLogger!.Object),
            new NoAuthenticationProvider(_noAuthLogger!.Object)
        };

        // Act & Assert
        foreach (var provider in providers)
        {
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(() =>
                provider.AddAuthenticationAsync(null!));
        }
    }

    #endregion
}
