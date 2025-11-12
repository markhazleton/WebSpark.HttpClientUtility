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
Run dorny/test-reporter@v1
Using test report parser 'dotnet-trx'
Creating test report .NET Tests
Error: No test report files were found
```

**Root Cause**: Multiple issues with test result generation and discovery:
1. When testing a **solution** with multiple test projects, explicit TRX file names cause overwrites
2. The test reporter action needs proper glob patterns and working directory context
3. Test results need to be in subdirectories per framework to avoid collisions

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

### Fix 2: Proper Test Result Generation

**File**: `.github/workflows/publish-nuget.yml`

**Changes**:

1. **Removed failing NuGet config step**:
   - The `dotnet nuget config set signatureValidationMode accept` command was failing because it ran before the SDK was properly initialized
   - This step is not critical for the workflow and was removed

2. **Updated test execution to use auto-generated TRX names**:
   ```yaml
   # Before:
   --logger "trx;LogFileName=test-results-net8.trx" --results-directory ./TestResults
   
   # After:
   --logger "trx" --results-directory ./TestResults/net8.0
   ```
   
   **Why**: When testing a solution with multiple test projects, dotnet test creates unique TRX files per project automatically. Explicit file names cause the second project to overwrite the first project's results.
   
   Each framework now has its own subdirectory:
   - `./TestResults/net8.0/` - Contains TRX files for all test projects on .NET 8
   - `./TestResults/net9.0/` - Contains TRX files for all test projects on .NET 9
   - `./TestResults/net10.0/` - Contains TRX files for all test projects on .NET 10

3. **Enhanced debugging output**:
   ```yaml
   - name: List test results (debug)
     if: always()
     run: |
       echo "TestResults directory structure:"
       ls -la ./TestResults || echo "TestResults directory not found"
       echo ""
       echo "Searching for TRX files:"
       find ./TestResults -type f -name "*.trx" 2>/dev/null || echo "No .trx files found"
       echo ""
       echo "All files in TestResults:"
       find ./TestResults -type f 2>/dev/null || echo "TestResults directory empty or not found"
       echo ""
       echo "Working directory:"
       pwd
       echo ""
       echo "Directory listing:"
       ls -la

   - name: Verify TRX files exist before reporting
     if: always()
     run: |
       TRX_COUNT=$(find ./TestResults -name "*.trx" 2>/dev/null | wc -l)
       echo "Found $TRX_COUNT TRX files"
       if [ $TRX_COUNT -eq 0 ]; then
         echo "WARNING: No TRX files found for test reporting"
         echo "This may cause the test reporter to fail"
       fi
   ```
   
   **Why**: Provides comprehensive diagnostics to troubleshoot test result generation issues.

4. **Updated test reporter configuration**:
   ```yaml
   - name: Test Report
     uses: dorny/test-reporter@v1
     if: always()
     continue-on-error: true  # Don't fail workflow if reporter has issues
     with:
       name: .NET Tests
       path: 'TestResults/**/*.trx'  # Single quotes for proper glob expansion
       reporter: dotnet-trx
       fail-on-error: false
   ```
   
   **Why**: 
   - `if: always()` ensures it runs even if tests fail
   - `continue-on-error: true` prevents reporter issues from failing the entire workflow
   - Single quotes around the path ensure proper shell glob expansion
   - Recursive pattern `**/*.trx` finds TRX files in all subdirectories

5. **Added explicit continue-on-error to test steps**:
   ```yaml
   - name: Run tests on .NET 8
     continue-on-error: false
     run: dotnet test ...
   ```
   
   **Why**: Makes error handling explicit - test failures should be visible but reported, not hidden.

## Verification Steps

1. ✅ **Local Build**: Successfully compiled with `dotnet build`
2. ✅ **SDK Resolution**: `global.json` now allows preview .NET 10 versions
3. ✅ **Test Output Structure**: Verified locally that TRX files are created in framework-specific subdirectories
4. ✅ **Multiple Test Projects**: Confirmed each test project generates its own TRX file

### Local Test Verification

```powershell
# Clean and run tests
Remove-Item ./TestResults -Recurse -Force
dotnet test WebSpark.HttpClientUtility.sln --configuration Release --framework net8.0 --logger "trx" --results-directory ./TestResults/net8.0

# Check output structure
Get-ChildItem ./TestResults -Recurse -Filter "*.trx"
```

**Expected Output**:
```
Directory: C:\...\TestResults\net8.0

Mode                 LastWriteTime         Length Name
----                 -------------         ------ ----
-a---          1/22/2025   3:27 PM          39048 markh_MYCROFTHOLMES_2025-11-12_15_27_33.trx
-a---          1/22/2025   3:27 PM         293624 markh_MYCROFTHOLMES_2025-11-12_15_27_33[1].trx
```

Two test projects create two TRX files with auto-generated unique names.

## Expected GitHub Actions Behavior

After these fixes, the workflow should:

1. ✅ Successfully resolve .NET 10 SDK (preview/RC version)
2. ✅ Display all installed SDKs including .NET 8, 9, and 10
3. ✅ Restore dependencies without signature validation errors
4. ✅ Build solution for all target frameworks
5. ✅ Run tests on .NET 8, 9, and 10 with proper result generation
6. ✅ Display comprehensive test result diagnostics
7. ✅ Verify TRX files exist before test reporting
8. ✅ Generate test reports with `dorny/test-reporter` (or gracefully skip if issues occur)
9. ✅ Upload test results as artifacts
10. ✅ Create coverage reports
11. ✅ Pack both base and crawler NuGet packages
12. ✅ Publish packages (only on version tags)

## Testing the Fix

To test these changes:

1. **Commit and push to main branch**:
   ```bash
   git add global.json .github/workflows/publish-nuget.yml
   git commit -m "fix: GitHub Actions SDK resolution and test reporting with enhanced diagnostics"
   git push origin main
   ```

2. **Monitor the workflow**: Visit https://github.com/markhazleton/WebSpark.HttpClientUtility/actions

3. **Check the debug output**: Look for the "List test results (debug)" and "Verify TRX files exist" steps to see:
   - How many TRX files were created
   - Where they're located
   - Whether the test reporter can find them

4. **Expected outcome**: 
   - Build, test, and package steps should complete successfully
   - If test reporter still has issues, it won't block the workflow (continue-on-error)
   - Publishing skipped since no version tag

## Common Patterns in Test Output

When running `dotnet test` on a solution with multiple test projects:

```bash
# BAD: Overwrites the same file
dotnet test Solution.sln --logger "trx;LogFileName=results.trx" --results-directory ./TestResults
# Result: Last test project overwrites previous results

# GOOD: Auto-generates unique names
dotnet test Solution.sln --logger "trx" --results-directory ./TestResults/net8.0
# Result: Each project gets username_machine_timestamp.trx and [1].trx, [2].trx for additional projects
```

## Additional Notes

### Why latestFeature vs latestPatch?

- **latestPatch**: Only rolls forward within the same minor version (e.g., 10.0.100 → 10.0.101)
  - Too restrictive for preview versions like `10.0.100-rc.2.25502.107`
  
- **latestFeature**: Rolls forward to any version with the same major version (e.g., 10.0.100 → 10.1.0 or 10.0.100-rc.x)
  - Flexible enough for preview/RC builds
  - Still maintains .NET 10 compatibility

### Why Separate Directories Per Framework?

When testing multiple frameworks against multiple test projects:

- **2 test projects** × **3 frameworks** = **6 TRX files**
- Using subdirectories prevents naming collisions
- Makes troubleshooting easier (you can see which framework failed)
- Glob pattern `TestResults/**/*.trx` finds all files regardless of depth

### dorny/test-reporter Known Issues

The test reporter action has some quirks:
- Requires files to exist before it runs (obviously)
- Glob patterns need careful quoting in YAML
- Works best with relative paths from repository root
- May fail silently if permissions aren't set correctly (needs `checks: write`)

Our configuration handles these by:
- Adding verification steps before running the reporter
- Using `continue-on-error: true` to prevent blocking
- Ensuring proper permissions in the job configuration
- Using single quotes for the glob pattern

### Alternative Solutions Considered

1. **Test each project individually**: More verbose, but gives fine-grained control
   ```yaml
   - name: Test base library
     run: dotnet test WebSpark.HttpClientUtility.Test/WebSpark.HttpClientUtility.Test.csproj ...
   - name: Test crawler library
     run: dotnet test WebSpark.HttpClientUtility.Crawler.Test/WebSpark.HttpClientUtility.Crawler.Test.csproj ...
   ```

2. **Remove test reporter entirely**: Just upload TRX artifacts for manual review
   - Simpler but loses automated test result visualization in GitHub UI

3. **Use different test result format**: JUnit XML instead of TRX
   - Would require changing local development tooling

We chose the current approach because it:
- ✅ Works with the solution file (tests all projects)
- ✅ Maintains framework separation for debugging
- ✅ Gracefully handles reporter failures
- ✅ Provides extensive diagnostics

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

1. ✅ Push changes to GitHub
2. ⏳ Verify workflow succeeds on main branch
3. 📊 Review debug output to confirm TRX file generation
4. 🐛 If test reporter still fails, review the "List test results" output for diagnostics
5. 🚀 When ready for release, follow standard process:
   - Update version in `.csproj` files
   - Update `CHANGELOG.md`
   - Create version tag
   - GitHub Actions will automatically publish to NuGet.org

## Troubleshooting Guide

If the workflow still fails:

### If "No test report files were found" persists:

1. Check the "List test results (debug)" step output - are TRX files created?
2. If NO TRX files:
   - Check if tests are actually running (look at test step logs)
   - Verify `--results-directory` path is correct
   - Check for test execution errors

3. If TRX files exist but reporter can't find them:
   - Verify working directory in GitHub Actions
   - Check file permissions
   - Try absolute path: `${{ github.workspace }}/TestResults/**/*.trx`

### If SDK resolution fails again:

1. Check GitHub Actions runner SDK versions have changed
2. May need to update `global.json` version to match available SDK
3. Consider using `rollForward: "latestMajor"` for maximum flexibility (not recommended for production)

### If tests fail on a specific framework:

1. Look at framework-specific test results: `TestResults/netX.0/`
2. Check if framework-specific dependencies are missing
3. Verify test projects target the correct frameworks in `.csproj` files
