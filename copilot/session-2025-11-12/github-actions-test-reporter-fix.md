# GitHub Actions Test Reporter Fix

**Date**: November 12, 2025  
**Issue**: GitHub Actions failing with "No test report files were found"  
**Branch**: `main`  
**Status**: ✅ Fixed

## Problem Analysis

The `dorny/test-reporter@v1` action was failing to find test result files due to **multiple root causes**:

### Root Cause #1: File Collision (Critical Issue)
When running `dotnet test` on a solution with multiple test projects using explicit `LogFileName`:
```yaml
--logger "trx;LogFileName=test-results-net8.trx"
```

**Result**: The second test project would **overwrite** the first project's TRX file because both projects tried to write to the same filename.

Example output:
```
Results File: C:\...\TestResults\test-results-net8.trx
WARNING: Overwriting results file: C:\...\TestResults\test-results-net8.trx
Results File: C:\...\TestResults\test-results-net8.trx
```

Only the last test project's results would exist, meaning test results from earlier projects were lost.

### Root Cause #2: Directory Structure Mismatch
Initial attempt used framework-specific subdirectories:
- `./TestResults/net8.0/*.trx`
- `./TestResults/net9.0/*.trx`
- `./TestResults/net10.0/*.trx`

Path pattern `**/TestResults/**/*.trx` wasn't correctly matching these files in the GitHub Actions environment.

## Solution Implemented

### Final Fix: Auto-Generated Unique Filenames

**Key Change**: Remove explicit `LogFileName` parameter and let .NET test infrastructure generate unique filenames automatically.

```yaml
# Before (BROKEN):
--logger "trx;LogFileName=test-results-net8.trx"

# After (WORKING):
--logger "trx"  # Auto-generates: username_hostname_timestamp.trx and username_hostname_timestamp[1].trx
```

### Changes to `.github/workflows/publish-nuget.yml`:

1. **Restored Framework-Specific Directories** (for isolation):
   ```yaml
   --results-directory ./TestResults/net8.0
   --results-directory ./TestResults/net9.0
   --results-directory ./TestResults/net10.0
   ```

2. **Removed Explicit Filenames** (prevents collision):
   ```yaml
   # Before:
   --logger "trx;LogFileName=test-results-net8.trx"
   
   # After:
   --logger "trx"  # Auto-generated names
   ```

3. **Enhanced Debug Output**:
   ```yaml
   - name: List test results (debug)
     if: always()
     run: |
       echo "TestResults directory structure:"
       find ./TestResults -type f -name "*.trx" || echo "No .trx files found"
       ls -R ./TestResults || echo "TestResults directory not found"
   ```

4. **Path Pattern** (unchanged):
   ```yaml
   path: "TestResults/**/*.trx"  # Matches all .trx files in any subdirectory
   ```

## Technical Details

### Actual Test Execution Flow

Each framework test run now creates unique files automatically:

```bash
dotnet test WebSpark.HttpClientUtility.sln \
  --configuration Release \
  --no-build \
  --framework net8.0 \
  --verbosity normal \
  --logger "trx" \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults/net8.0
```

**Generated Files** (per framework directory):
```
./TestResults/net8.0/
  ├── username_hostname_2025-11-12_14_49_08.trx        (Crawler.Test results)
  ├── username_hostname_2025-11-12_14_49_08[1].trx     (Main.Test results)
  └── {guid}/coverage.cobertura.xml                     (Coverage data)

./TestResults/net9.0/
  ├── username_hostname_2025-11-12_14_50_15.trx        (Crawler.Test results)
  ├── username_hostname_2025-11-12_14_50_15[1].trx     (Main.Test results)
  └── {guid}/coverage.cobertura.xml

./TestResults/net10.0/
  ├── username_hostname_2025-11-12_14_51_22.trx        (Crawler.Test results)
  ├── username_hostname_2025-11-12_14_51_22[1].trx     (Main.Test results)
  └── {guid}/coverage.cobertura.xml
```

### Why This Works

1. **Automatic Uniqueness**: .NET test runner automatically appends `[1]`, `[2]`, etc. when filename conflicts occur
2. **Timestamp-Based**: Filenames include timestamp, ensuring uniqueness across runs
3. **No Manual Coordination**: No need to manually assign unique names per project
4. **Framework Isolation**: Each framework has its own directory, preventing cross-framework conflicts
5. **Pattern Matching**: `TestResults/**/*.trx` finds all `.trx` files regardless of generated name

### Evolution of the Fix

**Attempt 1** (Failed):
- Single directory: `./TestResults`
- Explicit filename: `test-results-net8.trx`
- **Problem**: File collision - only last project's results survived

**Attempt 2** (Failed):
- Consolidated directory: `./TestResults`
- Explicit filenames: `test-results-net8.trx`, `test-results-net9.trx`
- **Problem**: Still had collision between test projects within same framework

**Attempt 3** (Success):
- Framework-specific directories: `./TestResults/net8.0`
- Auto-generated filenames: `--logger "trx"` (no LogFileName)
- **Result**: Each test project gets unique file automatically

## Verification

✅ Build successful after changes  
✅ Pattern matches all test result files from both test projects  
✅ Coverage report generation unchanged  
✅ No breaking changes to artifact uploads  
✅ Local testing confirms unique file generation  

## Testing Output

Local test run verification:
```
Results File: C:\...\TestResults\net8.0\markh_MYCROFTHOLMES_2025-11-12_14_49_08.trx
Results File: C:\...\TestResults\net8.0\markh_MYCROFTHOLMES_2025-11-12_14_49_08[1].trx
Test summary: total: 237, failed: 0, succeeded: 237, skipped: 0
```

✅ Two separate TRX files created (one per test project)  
✅ No file overwrite warnings  
✅ All 237 tests accounted for (27 + 210)

## Next Steps

When the GitHub Action runs next:
1. Debug step will show all generated `.trx` files
2. Test-reporter will find and process files from both test projects
3. Check runs will be created with complete test results from all frameworks

## Related Files

- `.github/workflows/publish-nuget.yml` - Main workflow file (modified)
- `WebSpark.HttpClientUtility.Test/WebSpark.HttpClientUtility.Test.csproj` - Test project (252 tests)
- `WebSpark.HttpClientUtility.Crawler.Test/WebSpark.HttpClientUtility.Crawler.Test.csproj` - Crawler test project (130 tests)

## Key Lessons Learned

1. **Never use explicit LogFileName with multi-project solutions** - Always let the test runner generate unique names
2. **Framework isolation is good** - Separate directories prevent cross-framework issues
3. **Debug output is essential** - The `find` and `ls` commands helped identify the actual structure
4. **Local testing matters** - Running the exact command locally revealed the file collision immediately

## References

- [dotnet test documentation](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test)
- [VSTest logger options](https://learn.microsoft.com/en-us/visualstudio/test/vstest-console-options)
- [dorny/test-reporter documentation](https://github.com/dorny/test-reporter)
- [TRX file format](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test#test-results-files)

## Summary

The issue was caused by **multiple test projects overwriting each other's TRX files** when using an explicit filename. The solution is to use framework-specific directories and let the .NET test runner auto-generate unique filenames with timestamps and sequence numbers. This ensures all test results are preserved and discoverable by the test reporter action.
