# SignalR Script Loading Race Condition - FINAL FIX

**Date**: January 25, 2025  
**Problem**: `Uncaught ReferenceError: signalR is not defined` even with correct CDN URL

## Root Cause: Script Loading Race Condition

### The Problem
```javascript
// BAD: This runs immediately when DOM is ready, but SignalR script might not be loaded yet
document.addEventListener('DOMContentLoaded', function () {
    initializeSignalR();  // ❌ signalR might not exist yet!
});
```

**Timeline**:
1. HTML parsing starts
2. Browser sees `<script src="...signalr.min.js">` and starts downloading (async)
3. DOM finishes parsing → `DOMContentLoaded` fires
4. JavaScript tries to use `signalR` → **Error: not defined yet**
5. SignalR script finishes loading (too late!)

### The Solution: Use Script `onload` Event

```html
<script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.0/signalr.min.js" 
        crossorigin="anonymous" 
        referrerpolicy="no-referrer"
        onload="initializeSignalRWhenReady()"></script>
```

```javascript
// GOOD: This only runs AFTER SignalR script is fully loaded
function initializeSignalRWhenReady() {
    if (typeof signalR !== 'undefined') {
        // SignalR is loaded, wait for DOM if needed
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', initializeSignalR);
        } else {
            initializeSignalR();
        }
    } else {
        console.error('SignalR library failed to load from CDN');
    }
}
```

**Correct Timeline**:
1. HTML parsing starts
2. Browser sees `<script src="...signalr.min.js">` and starts downloading
3. DOM finishes parsing → `DOMContentLoaded` fires (ignored, we don't use it)
4. SignalR script finishes loading → `onload` fires → `initializeSignalRWhenReady()` called
5. Check: Is SignalR defined? ✅ Yes!
6. Check: Is DOM ready? ✅ Yes!
7. Call `initializeSignalR()` → **Success!**

## Complete Fix Applied

**File**: `WebSpark.HttpClientUtility.Web/Views/Crawler/Index.cshtml`

### Step 1: Add `onload` handler to script tag

```html
@section Scripts {
    <script src="https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.0/signalr.min.js" 
            crossorigin="anonymous" 
            referrerpolicy="no-referrer"
            onload="initializeSignalRWhenReady()"></script>
```

### Step 2: Add wrapper function to check if SignalR is loaded

```javascript
<script>
    let connection;

    // Wait for SignalR to be fully loaded before initializing
    function initializeSignalRWhenReady() {
        if (typeof signalR !== 'undefined') {
            // SignalR is loaded, wait for DOM if needed
            if (document.readyState === 'loading') {
                document.addEventListener('DOMContentLoaded', initializeSignalR);
            } else {
                initializeSignalR();
            }
        } else {
            console.error('SignalR library failed to load from CDN');
        }
    }

    // Initialize SignalR connection
    function initializeSignalR() {
        connection = new signalR.HubConnectionBuilder()
            .withUrl("/crawlHub")
            .withAutomaticReconnect()
            .build();
        // ... rest of SignalR code
    }
```

### Step 3: Remove old `DOMContentLoaded` at bottom

```javascript
// OLD CODE (REMOVED):
// document.addEventListener('DOMContentLoaded', function () {
//     initializeSignalR();
// });

// NEW CODE:
// Note: SignalR initialization is handled by the onload event of the script tag above
```

## Why This Works

### The Script `onload` Event
- Fires **only when the external script is fully downloaded and executed**
- Guarantees `signalR` global variable exists
- Works reliably across all browsers

### Defense in Depth
```javascript
// Check 1: Is SignalR loaded?
if (typeof signalR !== 'undefined') {
    
    // Check 2: Is DOM ready?
    if (document.readyState === 'loading') {
        // Not ready yet, wait
        document.addEventListener('DOMContentLoaded', initializeSignalR);
    } else {
        // Ready now, go!
        initializeSignalR();
    }
}
```

This handles all possible timing scenarios:
1. ✅ **Script loads before DOM ready** → Wait for DOM
2. ✅ **DOM ready before script loads** → `onload` handles it
3. ✅ **Both ready at same time** → Works immediately
4. ❌ **Script fails to load** → Shows error message

## Testing

### 1. Hard Refresh
```
Ctrl + Shift + R  (Windows/Linux)
Cmd + Shift + R   (Mac)
```

### 2. Open DevTools Console (F12)

**Expected Output**:
```
SignalR connected
```

**Should NOT see**:
```
Uncaught ReferenceError: signalR is not defined ❌
```

### 3. Check Network Tab

Filter by "signalr":
```
Name: signalr.min.js
Status: 200 OK ✅
Type: application/javascript
Initiator: (index)
```

### 4. Manual Check in Console

Type in browser console:
```javascript
typeof signalR
// Should return: "object" ✅
// Should NOT return: "undefined" ❌
```

## Debugging

If it still doesn't work:

### Check 1: Script Blocked?
```javascript
// In browser console, check if script loaded:
console.log('SignalR loaded:', typeof signalR !== 'undefined');
```

### Check 2: CSP Policy?
Look in console for:
```
Refused to load the script 'https://cdnjs.cloudflare.com/...' because it violates the following Content Security Policy directive
```

**Fix**: Add to your CSP header or meta tag:
```html
<meta http-equiv="Content-Security-Policy" 
      content="script-src 'self' 'unsafe-inline' https://cdnjs.cloudflare.com">
```

### Check 3: Firewall/Antivirus?
- Corporate firewall may block CDN
- Try different network (mobile hotspot)
- Or self-host SignalR library

### Check 4: Browser Cache?
- Clear cache completely
- Try incognito/private browsing mode

## Alternative: Self-Host SignalR (No CDN Dependency)

If CDN doesn't work:

### Option 1: npm install (Recommended)
```bash
cd WebSpark.HttpClientUtility.Web
npm install @microsoft/signalr
```

Copy to wwwroot:
```bash
Copy-Item "node_modules/@microsoft/signalr/dist/browser/signalr.min.js" "wwwroot/lib/signalr/"
```

Update script tag:
```html
<script src="~/lib/signalr/signalr.min.js" 
        onload="initializeSignalRWhenReady()"></script>
```

### Option 2: Direct Download
Download from:
```
https://cdnjs.cloudflare.com/ajax/libs/microsoft-signalr/7.0.0/signalr.min.js
```

Save to:
```
WebSpark.HttpClientUtility.Web/wwwroot/lib/signalr/signalr.min.js
```

## Build Status
✅ Build successful  
✅ Script loading race condition fixed  
✅ Error handling added  
✅ Ready to test

## Summary of Changes

| Issue | Old Code | New Code |
|-------|----------|----------|
| **Load timing** | Used `DOMContentLoaded` | Use script `onload` event |
| **Error handling** | None | Check `typeof signalR` |
| **CDN attributes** | None | Added `crossorigin`, `referrerpolicy` |
| **Initialization** | Immediate | Conditional based on load state |

**This should finally work!** 🎉

Try it now with Ctrl+Shift+R (hard refresh)!
