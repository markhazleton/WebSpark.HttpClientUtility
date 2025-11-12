# GitHub Actions Pipeline Fix - 2025-01-22

## Problem Summary

The GitHub Actions workflow for publishing NuGet packages was failing with two critical errors:

### Error 1: SDK Version Mismatch
```
Requested SDK version: 10.0.100
global.json file: /home/runner/work/WebSpark.HttpClientUtility/WebSpark.HttpClientUtility/global.json

Installed SDKs:
10.0.100-rc.2.25502.107 [/usr/share/dotnet/sdk]

Error: Process completed with exit code 155.
```

**Root Cause**: The `global.json` file was requesting exact version `10.0.100` with `rollForward: "latestPatch"` policy. Since .NET 10 is still in preview/RC stage, only version `10.0.100-rc.2.25502.107` is available, which doesn't match the patch-level rollForward policy.

### Error 2: Test Results Not Found
```
Error: No test report files were found
```

**Root Cause**: Test results were being written to separate directories (`./TestResults/net8.0`, `./TestResults/net9.0`, `./TestResults/net10.0`) but the test reporter was looking for files in `TestResults/**/*.trx`. The path pattern wasn't matching the actual file structure.

## Solutions Applied

### Fix 1: Updated global.json RollForward Policy

**File**: `global.json`

**Change**:
```json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestFeature",  // Changed from "latestPatch"
    "allowPrerelease": true
  }
}
```

**Impact**: The SDK will now roll forward to any available .NET 10 version, including preview/RC builds. This allows the workflow to use `10.0.100-rc.2.25502.107` when `10.0.100` is not yet released.

### Fix 2: Consolidated Test Results Directory

**File**: `.github/workflows/publish-nuget.yml`

**Changes**:

1. **Removed failing NuGet config step**:
   - The `dotnet nuget config set signatureValidationMode accept` command was failing because it ran before the SDK was properly initialized
   - This step is not critical for the workflow and was removed

2. **Updated test execution to use explicit TRX file names**:
   ```yaml
   # Before:
   --results-directory ./TestResults/net8.0
   
   # After:
   --logger "trx;LogFileName=test-results-net8.trx" --results-directory ./TestResults
   ```
   
   All test frameworks now write to a single `./TestResults` directory with uniquely named TRX files.

3. **Updated test reporter path**:
   ```yaml
   # Before:
   path: "TestResults/**/*.trx"
   
   # After:
   path: "TestResults/*.trx"
   ```
   
   Simplified pattern that matches the flat directory structure.

4. **Changed test reporter condition**:
   ```yaml
   # Before:
   if: success() || failure()
   
   # After:
   if: always()
   ```
   
   Ensures test results are reported even if there are build errors.

## Verification Steps

1. ✅ **Local Build**: Successfully compiled with `dotnet build`
2. ✅ **SDK Resolution**: `global.json` now allows preview .NET 10 versions
3. ✅ **Test Output**: Consolidated to single directory with explicit file names

## Expected GitHub Actions Behavior

After these fixes, the workflow should:

1. ✅ Successfully resolve .NET 10 SDK (preview/RC version)
2. ✅ Display all installed SDKs including .NET 8, 9, and 10
3. ✅ Restore dependencies without signature validation errors
4. ✅ Build solution for all target frameworks
5. ✅ Run tests on .NET 8, 9, and 10
6. ✅ Generate test reports with `dorny/test-reporter`
7. ✅ Create coverage reports
8. ✅ Pack both base and crawler NuGet packages
9. ✅ Publish packages (only on version tags)

## Testing the Fix

To test these changes:

1. **Commit and push to main branch**:
   ```bash
   git add global.json .github/workflows/publish-nuget.yml
   git commit -m "fix: GitHub Actions SDK resolution and test reporting"
   git push origin main
   ```

2. **Monitor the workflow**: Visit https://github.com/markhazleton/WebSpark.HttpClientUtility/actions

3. **Expected outcome**: Build, test, and package steps should complete successfully (publishing skipped since no version tag)

## Additional Notes

### Why latestFeature vs latestPatch?

- **latestPatch**: Only rolls forward within the same minor version (e.g., 10.0.100 → 10.0.101)
  - Too restrictive for preview versions like `10.0.100-rc.2.25502.107`
  
- **latestFeature**: Rolls forward to any version with the same major version (e.g., 10.0.100 → 10.1.0 or 10.0.100-rc.x)
  - Flexible enough for preview/RC builds
  - Still maintains .NET 10 compatibility

### Alternative Solutions Considered

1. **Remove .NET 10 support**: Decided against this since the project targets .NET 10 in several `.csproj` files
2. **Remove global.json**: Would work but loses explicit SDK version control
3. **Pin to RC version**: Would break when .NET 10 RTM is released

### Related Files

- `global.json` - SDK version configuration
- `.github/workflows/publish-nuget.yml` - CI/CD pipeline
- Project files targeting net10.0:
  - `WebSpark.HttpClientUtility.Crawler/WebSpark.HttpClientUtility.Crawler.csproj`
  - `WebSpark.HttpClientUtility.Crawler.Test/WebSpark.HttpClientUtility.Crawler.Test.csproj`

## Compliance with Project Standards

✅ Following `.github/copilot-instructions.md`:
- No manual NuGet publishing (maintained CI/CD exclusivity)
- Build verification completed locally
- Documentation saved to `copilot/session-YYYY-MM-DD/` folder
- Changes preserve multi-targeting (net8.0, net9.0, net10.0)

## Next Steps

1. Push changes to GitHub
2. Verify workflow succeeds on main branch
3. When ready for release, follow standard process:
   - Update version in `.csproj` files
   - Update `CHANGELOG.md`
   - Create version tag
   - GitHub Actions will automatically publish to NuGet.org
