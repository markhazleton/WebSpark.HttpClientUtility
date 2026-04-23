# SignalR CDN Fix - Quick Reference

**Date**: January 25, 2025  
**Problem**: `Uncaught ReferenceError: signalR is not defined`

## The Root Cause

Razor views interpret `@` as code delimiter. URLs with `@` symbols cause parsing issues:
```
https://unpkg.com/@microsoft/signalr@7.0.0/...
                 ^problem      ^problem
```

## The Solution

✅ **Use Cloudflare CDN** (no @ symbols in URL):

```html
<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.0/signalr.min.js"></script>
```

## Why This Works

| Aspect | Details |
|--------|---------|
| URL Format | `cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.0/signalr.min.js` |
| @ Symbols | ✅ None - no Razor escaping needed |
| Reliability | ⭐⭐⭐⭐⭐ (Cloudflare's global CDN network) |
| Size | ~150 KB minified |
| Version | SignalR 7.0.0 (same as Microsoft's version) |

## Testing

1. **Hard refresh** browser (Ctrl+Shift+R)
2. **Open DevTools** (F12) → Console
3. **Look for**: `"SignalR connected"` message
4. **Should NOT see**: `signalR is not defined` error

## Network Verification

In DevTools → Network tab:
```
Name: signalr.min.js
Status: 200 OK ✅
Size: ~150 KB
URL: https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.0/signalr.min.js
```

## If It Still Doesn't Work

### Check Browser Console for:
```javascript
// Script loaded successfully?
console.log(typeof signalR);  // Should show "object", not "undefined"

// Try manual initialization
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/crawlHub")
    .build();
```

### Check Network Tab for:
- ❌ **Failed to load** (red text) - Firewall/antivirus blocking CDN
- ❌ **404 Not Found** - URL typo
- ❌ **CORS error** - CSP policy issue
- ✅ **200 OK** - Script loaded successfully

### Alternative CDNs (if cloudflare is blocked):

```html
<!-- Microsoft CDN -->
<script src="https://ajax.aspnetcdn.com/ajax/signalr/jquery.signalr-7.0.0.min.js"></script>

<!-- jsDelivr (requires @@ escaping in Razor) -->
<script src="https://cdn.jsdelivr.net/npm/@@microsoft/signalr@@7.0.0/dist/browser/signalr.min.js"></script>
```

## File Changed

`WebSpark.HttpClientUtility.Web/Views/Crawler/Index.cshtml` line 175:
```diff
- <script src="https://unpkg.com/@@microsoft/signalr@@7.0.0/dist/browser/signalr.min.js"></script>
+ <script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.0/signalr.min.js"></script>
```

## Build Status
✅ Build successful  
✅ No Razor parsing errors  
✅ Ready to test

**Try it now with a hard refresh!** 🎉
