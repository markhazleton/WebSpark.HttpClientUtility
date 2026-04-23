# SignalR CDN Loading Issue - FINAL FIX

**Date**: January 25, 2026  
**Issue**: SignalR library never loads from CDN, causing infinite retry loop  
**Root Cause**: CDN script blocked or unavailable

## The Problem - Evidence from Console

```
[Crawler] Scripts section loaded
[Crawler] Waiting for DOM...
[Crawler] initializeSignalR called
[Crawler] ❌ SignalR library not loaded yet, retrying in 100ms...
[Crawler] initializeSignalR called
[Crawler] ❌ SignalR library not loaded yet, retrying in 100ms...
[Crawler] initializeSignalR called
[Crawler] ❌ SignalR library not loaded yet, retrying in 100ms...
... (infinite loop - retries forever)
```

The CDN script `https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.0/signalr.min.js` **never loads**!

Possible causes:
- Network/firewall blocking the CDN
- Content Security Policy (CSP) blocking external scripts
- CDN is down or unreachable
- Corporate proxy issues

## The Solution - Bundle SignalR Locally

Instead of relying on an external CDN, **bundle SignalR with the application** using npm and Vite.

### Step 1: Install SignalR Package

```bash
npm install @microsoft/signalr --save
```

**Result**: SignalR is now in `node_modules` and `package.json`.

### Step 2: Import in main.js

**File**: `WebSpark.HttpClientUtility.Web/ClientApp/src/main.js`

```javascript
// SignalR for real-time communication
import * as signalR from '@microsoft/signalr';

// Make SignalR globally available for inline scripts
window.signalR = signalR;

// eslint-disable-next-line no-console
console.log('SignalR loaded:', typeof window.signalR !== 'undefined');
```

**Why global?** The inline script in `Index.cshtml` needs access to `signalR`. Making it a `window` property allows inline scripts to use it.

### Step 3: Remove CDN Script Tag

**File**: `WebSpark.HttpClientUtility.Web/Views/Crawler/Index.cshtml`

**Before**:
```html
@section Scripts {
    <script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.0/signalr.min.js" 
            crossorigin="anonymous" 
            referrerpolicy="no-referrer"></script>
    <script>
        // SignalR initialization with retry loop
    </script>
}
```

**After**:
```html
@section Scripts {
    <script>
        console.log("[Crawler] SignalR available:", typeof signalR !== 'undefined');
        // SignalR initialization WITHOUT retry loop
    </script>
}
```

**Removed**:
- CDN `<script>` tag
- Retry loop logic (`setTimeout` calls)
- `onload` event handler

### Step 4: Simplify Initialization

**Before**: Infinite retry loop checking if `signalR` exists:
```javascript
function initializeSignalR() {
    if (typeof signalR === 'undefined') {
        setTimeout(initializeSignalR, 100);  // Retry forever ❌
        return;
    }
    // Initialize...
}
```

**After**: Simple check and fail fast:
```javascript
function initializeSignalR() {
    if (typeof signalR === 'undefined') {
        console.error("[Crawler] ❌ SignalR library not loaded! Check main.js import.");
        return;  // Fail fast ✅
    }
    // Initialize...
}
```

## Build Output Changes

### Before (CDN):
```
../wwwroot/dist/js/main.RKJwACq7.js   80.61 kB │ gzip: 23.73 kB
```

### After (Bundled):
```
../wwwroot/dist/js/main.LSja-foc.js   137.05 kB │ gzip: 37.85 kB
```

**Difference**: +57KB (~15KB gzipped) - This is SignalR!

## Benefits

### CDN Approach (Old) ❌
- ❌ Depends on external service
- ❌ Blocked by CSP or firewall
- ❌ Network latency affects load time
- ❌ CDN could be down
- ❌ Extra HTTP request
- ❌ Requires retry logic

### Bundled Approach (New) ✅
- ✅ Self-contained (no external deps)
- ✅ Works offline
- ✅ No CSP issues
- ✅ Loads with rest of app
- ✅ Single HTTP request
- ✅ Cached with app bundle
- ✅ Version controlled in package.json

## Expected Console Output

### Before Fix:
```
[Crawler] Scripts section loaded
[Crawler] Waiting for DOM...
[Crawler] initializeSignalR called
[Crawler] ❌ SignalR library not loaded yet, retrying in 100ms...
[Crawler] initializeSignalR called
[Crawler] ❌ SignalR library not loaded yet, retrying in 100ms...
... (infinite loop)
```

### After Fix:
```
WebSpark.HttpClientUtility.Web - Build pipeline active
SignalR loaded: true ✅
[Crawler] Scripts section loaded
[Crawler] SignalR available: true ✅
[Crawler] DOM already ready, initializing SignalR immediately
[Crawler] initializeSignalR called
[Crawler] ✅ SignalR library is available
[Crawler] SignalR connection object created
[Crawler] Starting SignalR connection...
[Crawler] ✅ SignalR connected successfully!
[Crawler] Connection ID: xyz123...
```

## Testing

### Test 1: Verify SignalR is Bundled

1. **Hard refresh** (Ctrl+Shift+R)
2. Open DevTools (F12) → Console
3. **Type**: `signalR`
4. **Press Enter**
5. **Expected**: See object with `HubConnectionBuilder`, `LogLevel`, etc.

### Test 2: Verify Connection

1. Navigate to `/Crawler`
2. Open Console (F12)
3. **Expected**: See `SignalR loaded: true`
4. **Expected**: See `SignalR connected successfully!`

### Test 3: Verify Real-Time Updates

1. Start a crawl of TexEcon.com
2. Watch Console
3. **Expected**: See messages appearing during crawl:
```
[Crawler] SignalR UrlFound: Crawled: 1 | Queue: 5 | Found: https://texecon.com/
[Crawler] SignalR UrlFound: Crawled: 2 | Queue: 8 | Found: https://texecon.com/about
...
[Crawler] SignalR CrawlResults: {startUrl: "...", totalPages: 13, ...}
```

### Test 4: Network Tab Verification

1. Open DevTools → Network tab
2. Reload page
3. **Expected**: NO request to `cdnjs.cloudflare.com`
4. **Expected**: `main.LSja-foc.js` (or similar hash) loaded
5. **Expected**: File size ~137KB (includes SignalR)

## Package.json Update

```json
{
  "dependencies": {
    "@microsoft/signalr": "^8.0.11",  ← Added!
    "@popperjs/core": "^2.11.8",
    "bootstrap": "^5.3.3",
    "bootstrap-icons": "^1.11.3"
  }
}
```

**Version**: SignalR 8.0.11 (latest stable compatible with ASP.NET Core 8/9/10)

## Production Considerations

### Advantages
- **Reliability**: No external dependencies
- **Performance**: One less HTTP request, better caching
- **Security**: No CDN compromise risk
- **Compliance**: Easier to audit (all code is local)

### Disadvantages
- **Bundle Size**: +57KB (~15KB gzipped) added to main bundle
- **Updates**: Must manually update package.json (but we do this for everything else)

### When to Use CDN Instead
- Large public sites with CDN caching benefits
- Sites already using shared CDNs (SRI, subresource integrity)
- Bandwidth-constrained servers

For this application, **bundling is the right choice** because:
1. It's an enterprise/internal tool (not public mega-site)
2. Reliability > 57KB bandwidth savings
3. We already bundle Bootstrap, icons, etc. (same pattern)

## Build Status

✅ npm install successful  
✅ SignalR imported in main.js  
✅ CDN script removed  
✅ Retry loop removed  
✅ Build successful (137KB bundle)  
✅ Ready to test  

## Migration Checklist

For other pages using SignalR:

- [ ] Remove CDN `<script>` tags
- [ ] Remove retry logic
- [ ] Verify `window.signalR` is available
- [ ] Test connection in browser console
- [ ] Verify real-time updates work

**Test it now - SignalR should connect immediately!** 🎉
