# Security Policy

## Supported Versions

We release patches for security vulnerabilities for the following versions:

| Version | Supported       |
| ------- | ------------------ |
| 1.3.x   | :white_check_mark: |
| 1.2.x   | :white_check_mark: |
| 1.1.x   | :x:       |
| < 1.1   | :x:                |

## Reporting a Vulnerability

The WebSpark.HttpClientUtility team takes security bugs seriously. We appreciate your efforts to responsibly disclose your findings.

### How to Report a Security Vulnerability

**Please do not report security vulnerabilities through public GitHub issues.**

Instead, please report them via one of the following methods:

1. **GitHub Security Advisory** (Preferred)
   - Navigate to the repository's Security tab
   - Click "Report a vulnerability"
   - Fill out the form with detailed information

2. **Email**
   - Send an email to the repository owner
   - Include "SECURITY" in the subject line
   - Provide detailed information about the vulnerability

### What to Include

When reporting a vulnerability, please include:

- **Type of vulnerability** (e.g., SQL injection, XSS, authentication bypass)
- **Full paths of source file(s)** related to the vulnerability
- **Location of the affected source code** (tag/branch/commit or direct URL)
- **Step-by-step instructions** to reproduce the issue
- **Proof-of-concept or exploit code** (if possible)
- **Impact of the vulnerability** and how an attacker might exploit it
- **Possible mitigations** you've identified

### What to Expect

After submitting a vulnerability report:

1. **Acknowledgment**: We'll acknowledge receipt within 48 hours
2. **Assessment**: We'll investigate and assess the severity within 5 business days
3. **Fix Timeline**: We'll provide an expected timeline for a fix
4. **Updates**: We'll keep you informed of progress
5. **Credit**: With your permission, we'll credit you in the security advisory

### Response Timeline

- **Critical vulnerabilities**: Patch within 7 days
- **High severity**: Patch within 30 days
- **Medium/Low severity**: Patch in next scheduled release

## Security Best Practices for Users

When using WebSpark.HttpClientUtility:

### 1. Keep Updated
Always use the latest stable version to ensure you have the latest security patches.

```bash
dotnet add package WebSpark.HttpClientUtility
```

### 2. Sensitive Data Handling

**Never log sensitive data:**
```csharp
// DON'T do this
_logger.LogInformation("Request with API key: {ApiKey}", apiKey);

// DO this instead - the library sanitizes URLs automatically
_logger.LogInformation("Making request to {Url}", sanitizedUrl);
```

**Use secure authentication:**
```csharp
// Use proper authentication providers
services.AddScoped<IAuthenticationProvider, BearerTokenAuthenticationProvider>();

// Avoid hardcoding credentials
var token = configuration["ApiSettings:Token"]; // From secure configuration
```

### 3. Network Security

**Use HTTPS:**
```csharp
// Always use HTTPS endpoints in production
var request = new HttpRequestResult<T>
{
    RequestPath = "https://api.example.com/data", // Not http://
    // ...
};
```

**Validate SSL certificates:**
```csharp
// Don't disable certificate validation in production
services.AddHttpClient()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        // Don't set ServerCertificateCustomValidationCallback in production
    });
```

### 4. Input Validation

**Validate URLs:**
```csharp
if (!Uri.TryCreate(userInput, UriKind.Absolute, out var uri) || 
    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
{
    throw new ArgumentException("Invalid URL");
}
```

### 5. Rate Limiting & DoS Prevention

**Use appropriate timeouts:**
```csharp
var options = new CrawlerOptions
{
    TimeoutSeconds = 30, // Don't set this too high
    MaxConcurrentRequests = 3, // Prevent overwhelming servers
    DelayBetweenRequestsMs = 1000 // Be polite to servers
};
```

### 6. Dependency Management

**Scan for vulnerabilities:**
```bash
# Regularly check for vulnerable dependencies
dotnet list package --vulnerable
dotnet list package --outdated
```

### 7. Secure Configuration

**Use secret management:**
```csharp
// Don't store secrets in code or appsettings.json
// Use Azure Key Vault, AWS Secrets Manager, or environment variables

builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

### 8. Error Handling

**Don't expose sensitive information in errors:**
```csharp
try
{
    var result = await _httpService.GetAsync<T>(url);
}
catch (Exception ex)
{
    // Don't expose full stack traces to users
    _logger.LogError(ex, "Request failed"); // Log internally
  throw new ApplicationException("An error occurred processing your request"); // Generic message to user
}
```

## Known Security Considerations

### 1. HttpClient Lifecycle
The library uses `IHttpClientFactory` which properly manages `HttpClient` lifecycle to prevent socket exhaustion.

### 2. URL Sanitization
The library automatically sanitizes URLs in logs to prevent sensitive data exposure (API keys in query strings, etc.).

### 3. Strong Naming
The assembly is strong-named to prevent tampering.

### 4. Source Link
Source Link is enabled for transparency and debugging.

## Security-Related Configuration

### Recommended Settings for Production

```csharp
services.AddHttpClient("SecureClient")
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
   // Validate SSL certificates
   ServerCertificateCustomValidationCallback = null,
 
        // Use system proxy if needed
        UseProxy = true,
        
    // Follow redirects securely
        AllowAutoRedirect = true,
        MaxAutomaticRedirections = 3
    });

// Configure Polly for resilience
var pollyOptions = new HttpRequestResultPollyOptions
{
  MaxRetryAttempts = 3,
    RetryDelay = TimeSpan.FromSeconds(1),
    EnableCircuitBreaker = true,
    CircuitBreakerThreshold = 5,
    CircuitBreakerDuration = TimeSpan.FromSeconds(30)
};
```

## Vulnerability Disclosure Policy

We follow responsible disclosure principles:

1. Security researchers discover a vulnerability
2. Vulnerability is reported privately to maintainers
3. Maintainers acknowledge and investigate
4. A fix is developed and tested
5. A security advisory is published with:
 - Description of the vulnerability
   - Affected versions
   - Fixed versions
   - Workarounds (if any)
   - Credit to the researcher (with permission)
6. Users are notified to update

## Security Updates

Subscribe to security notifications:

- Watch the repository for security advisories
- Enable GitHub Dependabot alerts
- Follow NuGet package updates

## Contact

For security concerns, contact:
- GitHub Security Advisory: [Report a vulnerability](https://github.com/MarkHazleton/HttpClientUtility/security/advisories/new)

---

Thank you for helping keep WebSpark.HttpClientUtility and its users safe!
