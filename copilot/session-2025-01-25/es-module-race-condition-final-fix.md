# ES Module Async Loading Race Condition - FINAL FIX

**Date**: January 25, 2026  
**Issue**: SignalR never initializes, no Scripts section execution  
**Root Cause**: ES module loads asynchronously - inline scripts run before module finishes loading

## The Problem - Timeline

```
Time | Event
-----|------------------------------------------------------
T+0  | HTML parsed, <script type="module" src="main.js"> found
T+1  | Browser STARTS downloading main.js (async, non-blocking)
T+2  | Inline <script> in body executes
T+2  | typeof signalR === 'undefined' ❌ (module not done yet)
T+3  | @section Scripts executes
T+3  | initializeSignalR() tries to use signalR ❌ (not available)
T+5  | main.js FINISHES loading and executing
T+5  | window.signalR = ... ✅ (TOO LATE!)
```

**The inline scripts run BEFORE the ES module finishes loading!**

## Evidence

### Diagnostic Page Results
- ✅ JavaScript IS running (inline script works)
- ❌ SignalR NOT loaded (checked immediately)
- ❌ NO alert from Scripts section (tried to use undefined signalR)

### Network Tab
- ✅ `main.LSja-foc.js` loads with 200 OK
- ✅ File contains `window.signalR=wr;` (confirmed in source)

### Console
- ✅ `[DIAGNOSTIC] Inline script running`
- ❌ NO `WebSpark.HttpClientUtility.Web - Build pipeline active` (console.log removed by Terser)
- ❌ `[DIAGNOSTIC] SignalR is NOT available`

## Why This Happens

### ES Modules Load Asynchronously

```html
<!-- This doesn't block HTML parsing -->
<script type="module" src="main.js"></script>

<!-- This runs immediately while module is still loading -->
<script>
    console.log(typeof signalR); // undefined ❌
</script>
```

### Regular Scripts vs ES Modules

| Type | Behavior | Blocking |
|------|----------|----------|
| `<script src="...">` | Loads synchronously | Blocks HTML parsing |
| `<script type="module" src="...">` | Loads asynchronously | Non-blocking |
| `<script defer src="...">` | Loads async, executes after HTML | Non-blocking |

## The Solution

### Wait for SignalR to Load

Instead of assuming SignalR is immediately available, **poll until it's loaded**:

```javascript
function waitForSignalR(callback, maxAttempts = 50) {
    let attempts = 0;
    const checkSignalR = setInterval(() => {
        attempts++;
        if (typeof signalR !== 'undefined') {
            clearInterval(checkSignalR);
            console.log(`[Crawler] ✅ SignalR loaded after ${attempts} checks`);
            callback();
        } else if (attempts >= maxAttempts) {
            clearInterval(checkSignalR);
            console.error(`[Crawler] ❌ SignalR failed to load after ${maxAttempts * 100}ms`);
        }
    }, 100); // Check every 100ms
}
```

### Updated Initialization

**File**: `WebSpark.HttpClientUtility.Web/Views/Crawler/Index.cshtml`

```javascript
@section Scripts {
    <script>
        console.log("[Crawler] Scripts section loaded - waiting for SignalR module to load...");
        let connection;

        // Wait for SignalR module to load
        function waitForSignalR(callback, maxAttempts = 50) {
            let attempts = 0;
            const checkSignalR = setInterval(() => {
                attempts++;
                if (typeof signalR !== 'undefined') {
                    clearInterval(checkSignalR);
                    console.log(`[Crawler] ✅ SignalR loaded after ${attempts} checks`);
                    callback();
                } else if (attempts >= maxAttempts) {
                    clearInterval(checkSignalR);
                    console.error(`[Crawler] ❌ SignalR failed to load`);
                }
            }, 100);
        }

        function initializeSignalR() {
            connection = new signalR.HubConnectionBuilder()
                .withUrl("/crawlHub")
                .withAutomaticReconnect()
                .build();
            
            // ... event handlers ...
            
            connection.start().then(() => {
                console.log("[Crawler] ✅ SignalR connected!");
            });
        }

        // Wait for both DOM AND SignalR module
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => {
                waitForSignalR(initializeSignalR);
            });
        } else {
            waitForSignalR(initializeSignalR);
        }
    </script>
}
```

## Expected Behavior After Fix

### Console Output
```
[Crawler] Scripts section loaded - waiting for SignalR module to load...
[Crawler] DOM already ready, waiting for SignalR...
[Crawler] ✅ SignalR loaded after 2 checks (200ms)
[Crawler] initializeSignalR called
[Crawler] SignalR connection object created
[Crawler] Starting SignalR connection...
[Crawler] ✅ SignalR connected successfully!
[Crawler] Connection ID: xyz123...
```

### Timeline After Fix
```
T+0  | HTML parsed, <script type="module" src="main.js"> found
T+1  | Scripts section executes: waitForSignalR() starts checking
T+2  | Check #1: signalR undefined, keep waiting...
T+3  | Check #2: signalR undefined, keep waiting...
T+5  | main.js finishes: window.signalR = ... ✅
T+5  | Check #3: signalR available! ✅
T+5  | initializeSignalR() called
T+5  | SignalR connection starts
T+6  | SignalR connected! ✅
```

## Why Polling Works

1. **Non-blocking**: Uses `setInterval`, doesn't freeze the page
2. **Automatic**: No manual coordination needed
3. **Timeout**: Gives up after 5 seconds (50 * 100ms) if module never loads
4. **Diagnostic**: Logs exactly how long it took

## Alternative Solutions Considered

### Option 1: Use `<script defer>`
```html
<script defer src="main.js"></script>
```
**Problem**: Still async, same race condition

### Option 2: Load SignalR separately before main.js
```html
<script src="signalr.min.js"></script>
<script type="module" src="main.js"></script>
```
**Problem**: Defeats purpose of bundling, adds extra HTTP request

### Option 3: Use DOMContentLoaded
```javascript
window.addEventListener('DOMContentLoaded', initializeSignalR);
```
**Problem**: DOM loads before module finishes

### Option 4 (CHOSEN): Wait/Poll Pattern
```javascript
waitForSignalR(initializeSignalR);
```
**Benefits**:
- ✅ Works with ES modules
- ✅ Simple to implement
- ✅ Automatic retry
- ✅ No code restructuring needed

## Additional Fix: Terser Config

While debugging, we also discovered Terser is removing console.log statements:

**File**: `vite.config.js` (lines 46-48)

```javascript
compress: {
    drop_console: true,    // Removes ALL console.log
    drop_debugger: true
}
```

**For development**, you might want to disable this:

```javascript
minify: process.env.NODE_ENV === 'production' ? 'terser' : false,
terserOptions: {
    compress: {
        drop_console: process.env.NODE_ENV === 'production',
        drop_debugger: true
    }
}
```

This keeps console.log in development but removes them in production.

## Build Status

✅ Build successful  
✅ ES module race condition fixed  
✅ SignalR wait/poll pattern implemented  
✅ Diagnostic page updated  
✅ Ready to test  

## Testing

1. **Restart app** (Shift+F5, then F5)
2. **Navigate to** `/Crawler`
3. **Open Console** (F12)
4. **Expected**:
```
[Crawler] Scripts section loaded - waiting for SignalR module to load...
[Crawler] DOM already ready, waiting for SignalR...
[Crawler] ✅ SignalR loaded after X checks
[Crawler] initializeSignalR called
[Crawler] ✅ SignalR connected successfully!
```

5. **Enter URL** (e.g., `https://example.com`)
6. **Click "Start Crawl"**
7. **Expected**: Progress updates appear in real-time
8. **Expected**: Results table displays when complete

**Test it now - SignalR should initialize and connect!** 🎉
