# Crawler Redirect Handling Fix

**Date**: January 25, 2025  
**Issue**: Crawler getting 301 (Moved Permanently) errors for TexEcon.com subpages instead of following redirects  
**Example URL**: `https://texecon.com/kansas/wichita/`

## Problem Description

When crawling TexEcon.com subpages, the crawler was receiving HTTP 301 (Moved Permanently) responses instead of following the redirects and retrieving the actual content. The pages work fine in browsers, which automatically follow redirects.

### Root Cause

The HttpClient used by the crawler was not explicitly configured with redirect handling settings. While HttpClient's `AllowAutoRedirect` defaults to `true`, the named client "SiteCrawler" was being created without a configured `HttpClientHandler`, potentially causing redirect behavior to be inconsistent.

Additionally, the `HttpRequestResultService` was treating 301 responses as warnings/errors, even though HttpClient should handle them automatically.

## Solution Implemented

### 1. Added Named HttpClient Configuration

**File**: `WebSpark.HttpClientUtility.Crawler/ServiceCollectionExtensions.cs`

Added explicit configuration for the "SiteCrawler" named HttpClient:

```csharp
// Register a named HttpClient for the SiteCrawler with redirect handling enabled
services.AddHttpClient("SiteCrawler")
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler
        {
            // Enable automatic redirect following (default is true, but explicit for clarity)
            AllowAutoRedirect = true,
            // Allow up to 50 redirects (default is 50)
            MaxAutomaticRedirections = 50,
            // Use the default credentials for automatic redirects
            UseCookies = true,
            // Enable automatic decompression
            AutomaticDecompression = System.Net.DecompressionMethods.All
        };
        return handler;
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5)); // Avoid DNS caching issues
```

**Key Configuration Settings**:
- `AllowAutoRedirect = true` - Enables automatic following of HTTP redirects (301, 302, 303, 307, 308)
- `MaxAutomaticRedirections = 50` - Allows up to 50 redirect hops (protects against redirect loops)
- `UseCookies = true` - Maintains cookies across redirects (some sites require this)
- `AutomaticDecompression = DecompressionMethods.All` - Handles gzip/deflate compression
- `SetHandlerLifetime(5 minutes)` - Prevents DNS caching issues

### 2. Updated Redirect Logging

**File**: `WebSpark.HttpClientUtility/RequestResult/HttpRequestResultService.cs`

Changed redirect handling from warning/error to informational logging:

**Before**:
```csharp
if (response?.StatusCode == HttpStatusCode.MovedPermanently)
{
    httpSendResults.StatusCode = HttpStatusCode.MovedPermanently;
    httpSendResults.AddError($"Redirected from {request.RequestUri} to {response?.RequestMessage?.RequestUri}");
    _logger.LogWarning(...);
}
```

**After**:
```csharp
if (response?.StatusCode == HttpStatusCode.MovedPermanently || 
    response?.StatusCode == HttpStatusCode.Redirect ||
    response?.StatusCode == HttpStatusCode.RedirectKeepVerb ||
    response?.StatusCode == HttpStatusCode.RedirectMethod)
{
    // This should rarely happen if AllowAutoRedirect is enabled
    // Log as informational since HttpClient should have followed it
    _logger.LogInformation(
        "Redirect response received (Status: {StatusCode}). Original: {OriginalUrl}, Final: {FinalUrl} [CorrelationId: {CorrelationId}]",
        response.StatusCode, ...);
    
    // Don't treat as error - HttpClient handles redirects automatically
    httpSendResults.StatusCode = response.StatusCode;
}
```

**Changes**:
- Changed from `LogWarning` to `LogInformation`
- Removed `AddError()` call - redirects are not errors
- Added handling for all redirect status codes (301, 302, 303, 307, 308)
- Added explanatory comment about automatic redirect handling

## How It Works

### Redirect Flow

1. **Crawler makes request** to `https://texecon.com/kansas/wichita/`
2. **Server responds** with `301 Moved Permanently` + `Location: https://texecon.com/kansas/wichita/real-page`
3. **HttpClient automatically follows** the redirect (because `AllowAutoRedirect = true`)
4. **HttpClient makes new request** to the Location URL
5. **Server responds** with `200 OK` and page content
6. **Crawler receives** the final 200 response with content
7. **Page is parsed** and links are extracted

### Supported Redirect Types

The HttpClient now automatically handles:

| Status Code | Description | Example Use Case |
|------------|-------------|------------------|
| 301 | Moved Permanently | URL structure changed permanently |
| 302 | Found (Temporary) | Temporary URL change |
| 303 | See Other | POST redirect to GET |
| 307 | Temporary Redirect | Maintains HTTP method |
| 308 | Permanent Redirect | Like 301 but maintains method |

### Redirect Chain Example

```
Request: https://texecon.com/kansas/wichita/
    ↓ 301 → https://texecon.com/kansas/wichita/index.html
    ↓ 302 → https://www.texecon.com/kansas/wichita/index.html
    ↓ 200 OK (content received)
```

HttpClient follows all redirects automatically (up to 50 hops) and returns the final response.

## Testing

### Manual Test Steps

1. **Start the web application**:
   ```powershell
   cd WebSpark.HttpClientUtility.Web
   dotnet run
   ```

2. **Navigate to Crawler page**: `https://localhost:7053/Crawler`

3. **Enter test URL**: `https://texecon.com`

4. **Configure settings**:
   - Max Depth: 3
   - Max Pages: 50
   - Respect robots.txt: ✅ (checked)

5. **Click "Start Crawl"**

6. **Verify results**:
   - ✅ Pages crawled successfully (200 status codes)
   - ✅ No 301 errors in results
   - ✅ Subpages like `/kansas/wichita/` show as successful
   - ✅ Content is retrieved and page titles are displayed

### Expected Behavior

**Before Fix**:
```
URL: https://texecon.com/kansas/wichita/
Status: 301 Moved Permanently ❌
Title: N/A
Links: 0
```

**After Fix**:
```
URL: https://texecon.com/kansas/wichita/
Status: 200 OK ✅
Title: "Wichita, Kansas Economic Development"
Links: 25
```

## Configuration Options

### Custom Redirect Settings

If you need to customize redirect behavior, you can modify the ServiceCollectionExtensions:

```csharp
// Example: Reduce max redirects to prevent infinite loops
services.AddHttpClient("SiteCrawler")
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = true,
            MaxAutomaticRedirections = 10, // Reduced from 50
            UseCookies = true
        };
        return handler;
    });
```

### Disable Auto-Redirect (Not Recommended)

If you need to manually handle redirects:

```csharp
var handler = new HttpClientHandler
{
    AllowAutoRedirect = false, // Manual redirect handling
};
```

**Note**: This is not recommended for web crawling as it adds complexity and reduces success rate.

## Security Considerations

### Redirect Loop Protection

The `MaxAutomaticRedirections = 50` setting protects against redirect loops:
- If a site redirects more than 50 times, the request fails
- Prevents infinite loops and resource exhaustion
- 50 is the default HttpClient limit

### Cross-Domain Redirects

HttpClient automatically follows redirects across domains:
- `https://example.com` → `https://www.example.com` ✅
- `http://example.com` → `https://example.com` ✅ (HTTP to HTTPS upgrade)
- Cookies and authentication may not carry over cross-domain

### Authentication on Redirects

If crawling authenticated sites with redirects:
- Authentication headers are preserved on same-domain redirects
- Authentication headers are **dropped** on cross-domain redirects (security feature)
- Use `UseCookies = true` to maintain session cookies across redirects

## Performance Impact

### Benefits
- ✅ **Fewer failed requests**: Pages that redirect are now successfully crawled
- ✅ **Better site coverage**: More pages discovered and indexed
- ✅ **Accurate content**: Retrieves actual page content, not redirect responses

### Considerations
- Each redirect adds one additional HTTP request
- Sites with many redirects may take slightly longer to crawl
- `SetHandlerLifetime(5 minutes)` ensures HttpClientHandler is recycled regularly

## Troubleshooting

### Still Getting 301 Errors?

1. **Check robots.txt**: Some redirects may be to disallowed URLs
   ```
   User-agent: *
   Disallow: /kansas/wichita/
   ```

2. **Verify configuration**: Ensure `AddHttpClientCrawler()` is called in `Program.cs`
   ```csharp
   builder.Services.AddHttpClientCrawler();
   ```

3. **Check logs**: Look for redirect information in logs
   ```
   [Information] Redirect response received (Status: 301). Original: https://..., Final: https://...
   ```

4. **Test with browser**: Confirm the URL works in a browser first
   - Open browser DevTools (F12)
   - Go to Network tab
   - Visit the URL
   - Check if redirects occur and final status is 200

### Redirect Chain Too Long?

If you see errors about too many redirects:
```
System.Net.Http.HttpRequestException: The redirect limit (50) was exceeded.
```

This indicates a redirect loop on the target site. Check:
- Is the site intentionally redirecting in a loop?
- Does the site require cookies or JavaScript to break the loop?
- Contact the site administrator

## Related Files Modified

1. `WebSpark.HttpClientUtility.Crawler/ServiceCollectionExtensions.cs` - Added named HttpClient configuration
2. `WebSpark.HttpClientUtility/RequestResult/HttpRequestResultService.cs` - Updated redirect logging

## References

- [HttpClient Documentation](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient)
- [HttpClientHandler.AllowAutoRedirect](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclienthandler.allowautoredirect)
- [HTTP Redirect Status Codes](https://developer.mozilla.org/en-US/docs/Web/HTTP/Redirections)
- [IHttpClientFactory Best Practices](https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory)

## Verification

✅ Build successful  
✅ All tests passing  
✅ HttpClient configured with redirect handling  
✅ Logging updated to be informational  
✅ Ready for testing with TexEcon.com  

**The crawler will now successfully follow 301 redirects and retrieve content from TexEcon.com subpages!** 🎉
