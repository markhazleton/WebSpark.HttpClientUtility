# HttpClient Redirect Issue - HTTPS to HTTP Downgrade

**Date**: January 25, 2026  
**Critical Discovery**: TexEcon.com redirects from HTTPS to HTTP!

## The Root Cause

Testing with curl reveals the actual redirect:
```bash
$ curl -I "https://texecon.com/kansas/wichita"
HTTP/1.1 301 Moved Permanently
Location: http://texecon.com/kansas/wichita/
         ^^^^
         HTTP not HTTPS!
```

**All TexEcon.com URLs redirect from `https://` to `http://`** (protocol downgrade).

## Why This Matters

HttpClient with `AllowAutoRedirect=true` **should** follow these redirects, but logs show it's not happening:

```
Original: https://texecon.com/kansas/wichita
Final: https://texecon.com/kansas/wichita  ← Same URL = redirect NOT followed
```

This means the `AllowAutoRedirect` configuration is **still not being applied**.

## Changes Made for Debugging

### File: `WebSpark.HttpClientUtility.Crawler/ServiceCollectionExtensions.cs`

1. **Moved registration removal to the beginning**:
```csharp
// FIRST: Remove any existing IHttpRequestResultService registrations
// This must happen BEFORE configuring HttpClients
var existingRegistrations = services.Where(sd => sd.ServiceType == typeof(IHttpRequestResultService)).ToList();
foreach (var registration in existingRegistrations)
{
    services.Remove(registration);
}
```

2. **Added detailed console logging**:
```csharp
Console.WriteLine($"======================================");
Console.WriteLine($"[SiteCrawler] HttpClientHandler Configuration:");
Console.WriteLine($"  AllowAutoRedirect: {handler.AllowAutoRedirect}");
Console.WriteLine($"  MaxAutomaticRedirections: {handler.MaxAutomaticRedirections}");
Console.WriteLine($"  UseCookies: {handler.UseCookies}");
Console.WriteLine($"======================================");
```

3. **Simplified handler creation** (removed ConfigureHttpClientDefaults which doesn't work as expected)

## Testing Steps

### Step 1: Start the Application

Run the web project and check the **console output** (not the browser, the Visual Studio console).

**Look for**:
```
======================================
[SiteCrawler] HttpClientHandler Configuration:
  AllowAutoRedirect: True
  MaxAutomaticRedirections: 50
  UseCookies: True
======================================
[SiteCrawler] Creating HttpRequestResultService with SiteCrawler client
```

### Step 2: Analyze Results

**If you see the configuration message**:
- ✅ Handler is being created with correct settings
- ❌ But redirects still not working → Something else is preventing redirects

**If you DON'T see the configuration message**:
- ❌ Handler factory is not being called
- Issue: HttpClient is using a different handler or default handler

### Step 3: Test URL Manually

In browser, visit:
```
https://texecon.com/kansas/wichita
```

You should be redirected to:
```
http://texecon.com/kansas/wichita/
```

(Note: HTTP not HTTPS, and trailing slash added)

## Possible Issues

### Issue 1: Handler Not Being Applied

**Symptom**: No console message about handler configuration

**Possible Causes**:
- HttpClientFactory is using a different handler
- Middleware is replacing the handler
- Named client not being used

**Solution**: Verify HttpClient is created with name "SiteCrawler"

### Issue 2: Handler Applied But Not Working

**Symptom**: Console shows `AllowAutoRedirect: True` but still getting 301

**Possible Causes**:
- Delegating handler in the pipeline is blocking redirects
- HttpClient logging middleware is intercepting before redirect
- Request message has properties preventing redirect

**Debug Steps**:
1. Check if there are any `AddHttpMessageHandler` calls
2. Check if telemetry/logging decorators are in the pipeline
3. Try creating HttpClient directly without factory

### Issue 3: HTTPS to HTTP Security Block

**Symptom**: Redirects to HTTPS URLs work, but not to HTTP URLs

**Note**: HttpClient **should** follow HTTPS→HTTP redirects with `AllowAutoRedirect=true`. If it doesn't, there might be a security policy preventing it.

**Workaround**: Use HTTP URLs directly:
```csharp
// Instead of: https://texecon.com/kansas/wichita
// Use: http://texecon.com/kansas/wichita
```

## Next Steps

1. **Run the app and check console for configuration message**
2. **If message appears**: The handler is configured, but something is preventing its use
3. **If message doesn't appear**: The handler factory is not being called

### If Handler is Configured But Not Working

We need to trace the request pipeline. Add logging in `HttpRequestResultService.cs` to check:

```csharp
var request = CreateHttpRequest(httpSendResults);
Console.WriteLine($"Request AllowAutoRedirect: {httpClient.???}");  // HttpClient doesn't expose this
```

Unfortunately, HttpClient doesn't expose the handler settings, so we can't verify them at runtime.

### Alternative Approach

If the handler configuration continues to not work, we might need to:

1. **Create HttpClient manually** instead of using factory:
```csharp
var handler = new HttpClientHandler { AllowAutoRedirect = true, ... };
var httpClient = new HttpClient(handler);
```

2. **Use a different pattern** that doesn't rely on IHttpClientFactory

3. **Follow redirects manually** by checking for 301/302 and making a new request

## Build Status

✅ Build successful  
✅ Console logging added  
✅ Registration removal moved to beginning  
✅ Simplified handler configuration  
⏳ Awaiting test results  

**Check the console output when you run the app!**
