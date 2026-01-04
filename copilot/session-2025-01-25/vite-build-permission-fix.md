# Visual Studio Debug Build Error Fix

**Date**: January 25, 2025  
**Issue**: Permission denied error when debugging web project in Visual Studio  
**Error**: `EPERM, Permission denied: wwwroot\dist`

## Problem Description

When launching the web project in Visual Studio debug mode, the build fails with:
```
Error: EPERM, Permission denied: \\?\C:\GitHub\...\wwwroot\dist
MSB3073: The command "npm.cmd run build" exited with code 1.
```

### Root Cause

Visual Studio's node.js process (used for debugging and hot reload) keeps file locks on the `wwwroot\dist` directory. When MSBuild tries to clean and rebuild using `npm run build`, the clean operation fails because:

1. Visual Studio has files locked in `wwwroot\dist` from previous debug session
2. `npm run build` → triggers `prebuild` → runs `npm run clean`
3. The clean script tries to delete the locked directory → Permission denied

## Solution Implemented

### 1. Updated `.csproj` Build Configuration

**File**: `WebSpark.HttpClientUtility.Web.csproj`

**Changes**:
- Modified `NpmClean` target to use npm script instead of MSBuild `RemoveDir`
- Added `IgnoreExitCode="true"` and `ContinueOnError="true"` to handle locked files gracefully
- Made clean operation non-blocking so build can proceed even if clean fails

```xml
<!-- Clean NPM artifacts with retry logic -->
<Target Name="NpmClean" AfterTargets="Clean">
  <Message Importance="high" Text="Cleaning Vite build artifacts..." />
  <!-- Use npm clean script which has better cross-platform support and error handling -->
  <Exec Command="$(NpmCommand) run clean" 
        WorkingDirectory="$(ProjectDir)" 
        IgnoreExitCode="true" 
        ContinueOnError="true" 
        Condition="Exists('package.json')" />
</Target>
```

### 2. Enhanced `package.json` Clean Script

**File**: `package.json`

**Changes**:
- Added retry logic to `fs.rmSync` (maxRetries: 3, retryDelay: 100ms)
- Added try-catch error handling to prevent build failures
- Added `build:fast` script option that skips clean for faster rebuilds

```json
"scripts": {
  "build": "npm run clean && npm run lint && vite build",
  "build:fast": "vite build",
  "clean": "node -e \"try { require('fs').rmSync('wwwroot/dist', { recursive: true, force: true, maxRetries: 3, retryDelay: 100 }); } catch (e) { if (e.code !== 'ENOENT') console.warn('Clean warning:', e.message); }\"",
  "prebuild": "npm run clean"
}
```

## How to Use

### Normal Development (Recommended)

Just debug as usual in Visual Studio:
1. Press **F5** or click **Debug**
2. The build will now succeed even if files are locked
3. Vite will rebuild only changed files

### If You Still See Issues

**Option 1: Stop All Node Processes**
```powershell
Get-Process | Where-Object { $_.ProcessName -like "*node*" } | Stop-Process -Force
```

**Option 2: Manual Clean Before Debug**
```powershell
cd WebSpark.HttpClientUtility.Web
npm run clean
```

**Option 3: Use Fast Build (Skip Clean)**

Edit `.csproj` temporarily to use fast build:
```xml
<Exec Command="$(NpmCommand) run build:fast" ... />
```

### For Production Builds

The regular `npm run build` script still performs full clean:
```powershell
cd WebSpark.HttpClientUtility.Web
npm run build
```

## Visual Studio-Specific Tips

### 1. Close Browser Tabs After Debug

Visual Studio's hot reload feature can keep locks active. Close browser tabs opened by the debugger when stopping debug sessions.

### 2. Disable Hot Reload (Optional)

If issues persist, disable hot reload in Visual Studio:
- Tools → Options → Debugging → Hot Reload
- Uncheck "Enable Hot Reload and Edit and Continue"

### 3. Stop IIS Express Properly

Make sure IIS Express stops cleanly:
- Use **Shift+F5** to stop debugging (not just close browser)
- Or stop via system tray icon

### 4. Clean Solution

If build is completely stuck:
```
Build → Clean Solution
Build → Rebuild Solution
```

## How the Fix Works

### Before (Fragile)
1. MSBuild calls NpmBuild target
2. npm run build → npm run clean (prebuild hook)
3. fs.rmSync tries to delete `wwwroot\dist`
4. ❌ Files locked by VS → Permission denied → Build fails

### After (Robust)
1. MSBuild calls NpmBuild target
2. npm run build → npm run clean (prebuild hook)
3. fs.rmSync tries to delete with retry logic
4. ✅ If locked: Warns but continues → Build succeeds
5. Vite overwrites files (works even if delete failed)

## Verification

To verify the fix is working:

```powershell
# Clean slate
cd WebSpark.HttpClientUtility.Web
npm run clean

# Build should succeed
dotnet build

# Debug in Visual Studio - should work now
# Press F5
```

## Additional Notes

### Why Vite Still Works Even If Clean Fails

Vite's build process:
1. Creates new content in `.vite` temp folder
2. Overwrites existing files in `dist` (doesn't require delete)
3. Only fails if it can't write (rare)

This means even if the clean step fails due to locks, Vite can usually still complete the build successfully.

### Performance Impact

The retry logic adds minimal overhead:
- Max 300ms delay (3 retries × 100ms) only when files are locked
- Most builds: no delay (instant clean)
- Failed cleans don't block the build pipeline

## Troubleshooting Checklist

If you still encounter build errors:

- [ ] Restart Visual Studio
- [ ] Close all browser tabs/windows
- [ ] Stop all node.js processes: `Get-Process node | Stop-Process -Force`
- [ ] Delete `wwwroot\dist` manually in File Explorer
- [ ] Run `dotnet clean` then `dotnet build`
- [ ] Check antivirus isn't locking files
- [ ] Run Visual Studio as Administrator (last resort)

## Related Files Modified

1. `WebSpark.HttpClientUtility.Web/WebSpark.HttpClientUtility.Web.csproj` - Build targets
2. `WebSpark.HttpClientUtility.Web/package.json` - NPM scripts

## References

- [Node.js fs.rmSync Options](https://nodejs.org/api/fs.html#fsrmsyncpath-options)
- [MSBuild Exec Task](https://learn.microsoft.com/en-us/visualstudio/msbuild/exec-task)
- [Vite Build](https://vitejs.dev/guide/build.html)
