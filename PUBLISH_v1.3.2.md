# Publishing Guide for v1.3.2

## Version 1.3.2 - Test Fix Release

### What Changed
- ✅ Fixed failing CurlCommandSaver tests
- ✅ Improved mock setup patterns (strict → loose)
- ✅ All 252 tests now passing
- ✅ Better test reliability and maintainability

## Quick Publish Commands

### Step 1: Commit and Push Changes
```powershell
cd C:\GitHub\MarkHazleton\WebSpark.HttpClientUtility

# Add all changes
git add .

# Commit with descriptive message
git commit -m "Release v1.3.2: Fix CurlCommandSaver tests and improve test reliability"

# Push to main
git push origin main
```

### Step 2: Create and Push Tag
```powershell
# Create annotated tag
git tag -a v1.3.2 -m "Version 1.3.2 - Test fixes and improved reliability"

# Push the tag (this triggers GitHub Actions)
git push origin v1.3.2
```

### Step 3: Monitor GitHub Actions
The workflow will automatically:
1. ✅ Restore dependencies (with NuGet signature validation fix)
2. ✅ Build in Release mode
3. ✅ Run all 252 tests
4. ✅ Pack NuGet packages (.nupkg and .snupkg)
5. ✅ Publish to NuGet.org (when tag is pushed)
6. ✅ Create GitHub Release with CHANGELOG

**Monitor at:** https://github.com/markhazleton/WebSpark.HttpClientUtility/actions

## Why v1.3.1 Was Skipped

The publish step was likely skipped because:
1. ❌ The tag wasn't pushed (`git push origin v1.3.1`)
2. ❌ Only main branch was pushed, not the tag
3. ⚠️ Workflow only publishes when `refs/tags/v*` is detected

## Verification After Publishing

### 1. Check GitHub Actions (5-10 minutes)
```powershell
start https://github.com/markhazleton/WebSpark.HttpClientUtility/actions
```

### 2. Verify NuGet.org (15-20 minutes for indexing)
```powershell
start https://www.nuget.org/packages/WebSpark.HttpClientUtility/1.3.2
```

### 3. Test Installation
```powershell
mkdir TestV132
cd TestV132
dotnet new console
dotnet add package WebSpark.HttpClientUtility --version 1.3.2
dotnet build
cd ..
Remove-Item -Path TestV132 -Recurse -Force
```

### 4. Verify GitHub Release
```powershell
start https://github.com/markhazleton/WebSpark.HttpClientUtility/releases/tag/v1.3.2
```

## Complete Command Sequence

Copy and paste this entire block:

```powershell
# Navigate to repository
cd C:\GitHub\MarkHazleton\WebSpark.HttpClientUtility

# Ensure we're on main and up to date
git checkout main
git pull origin main

# Stage all changes
git add .

# Commit
git commit -m "Release v1.3.2: Fix CurlCommandSaver tests and improve test reliability"

# Push to main
git push origin main

# Create tag
git tag -a v1.3.2 -m "Version 1.3.2 - Test fixes and improved reliability"

# Push tag (THIS TRIGGERS THE WORKFLOW)
git push origin v1.3.2

# Open GitHub Actions to watch
start https://github.com/markhazleton/WebSpark.HttpClientUtility/actions

Write-Host "`n✅ Tag v1.3.2 pushed! GitHub Actions will now:" -ForegroundColor Green
Write-Host "   1. Build the solution" -ForegroundColor Cyan
Write-Host "   2. Run all tests" -ForegroundColor Cyan
Write-Host "   3. Pack NuGet packages" -ForegroundColor Cyan
Write-Host "   4. Publish to NuGet.org" -ForegroundColor Cyan
Write-Host "   5. Create GitHub Release" -ForegroundColor Cyan
Write-Host "`nMonitor progress at the link that just opened!" -ForegroundColor Yellow
```

## Troubleshooting

### If Workflow Doesn't Trigger
```powershell
# Verify tag exists locally
git tag -l "v1.3.2"

# Verify tag was pushed to remote
git ls-remote --tags origin | Select-String "v1.3.2"

# If tag wasn't pushed
git push origin v1.3.2
```

### If Workflow Fails
1. Check logs in GitHub Actions
2. Look for specific error messages
3. Common issues:
   - ❌ NuGet signature validation (already fixed in workflow)
 - ❌ Test failures (all passing now)
   - ❌ Missing NUGET_API_KEY secret
   - ❌ Package already exists at that version

### If Package Already Exists
If you already published 1.3.2 and need to republish:
```powershell
# You CANNOT republish the same version
# You must increment to 1.3.3
# Edit WebSpark.HttpClientUtility.csproj and change:
# <Version>1.3.3</Version>
# Then follow the process again with v1.3.3
```

## Expected Timeline

| Step | Time | Status Check |
|------|------|--------------|
| Tag push | Immediate | `git ls-remote --tags origin` |
| Workflow starts | 10-30 seconds | GitHub Actions page |
| Build & Test | 2-5 minutes | GitHub Actions logs |
| Pack & Publish | 1-2 minutes | GitHub Actions logs |
| NuGet indexing | 5-15 minutes | NuGet.org package page |
| Available for install | 15-20 minutes | `dotnet add package` |

## Success Indicators

✅ **GitHub Actions:**
- All steps show green checkmarks
- "Publish NuGet package" step completed
- "Create GitHub Release" step completed

✅ **NuGet.org:**
- Package version 1.3.2 appears in version list
- Download count starts at 0
- Package details show correct metadata

✅ **GitHub Releases:**
- Release v1.3.2 exists with tag
- CHANGELOG.md content in release notes
- .nupkg and .snupkg files attached

## Post-Release

After successful publish:

1. ✅ Update any integration documentation
2. ✅ Announce in relevant channels (optional)
3. ✅ Monitor for any issues in first 24 hours
4. ✅ Check download counts after a few days

## Notes

- 📌 This is a patch release (bug fixes only)
- 📌 No breaking changes
- 📌 Fully backward compatible with 1.3.0 and 1.3.1
- 📌 Primary change: Test reliability improvements
- 📌 All 252 tests passing

## Links

- **Repository:** https://github.com/markhazleton/WebSpark.HttpClientUtility
- **NuGet Package:** https://www.nuget.org/packages/WebSpark.HttpClientUtility/
- **GitHub Actions:** https://github.com/markhazleton/WebSpark.HttpClientUtility/actions
- **Releases:** https://github.com/markhazleton/WebSpark.HttpClientUtility/releases

---

**Ready to publish v1.3.2!** 🚀
