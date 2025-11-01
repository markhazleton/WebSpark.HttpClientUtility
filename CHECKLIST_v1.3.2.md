# v1.3.2 Release Checklist

## Pre-Release ✅
- [x] Version updated to 1.3.2 in .csproj
- [x] CHANGELOG.md updated
- [x] README.md updated
- [x] Build successful
- [x] All 252 tests passing
- [x] CurlCommandSaver tests fixed

## Publishing Steps

### 1. Commit Changes
```powershell
git add .
git commit -m "Release v1.3.2: Fix CurlCommandSaver tests and improve test reliability"
git push origin main
```
- [ ] Changes committed
- [ ] Pushed to main

### 2. Create and Push Tag
```powershell
git tag -a v1.3.2 -m "Version 1.3.2 - Test fixes and improved reliability"
git push origin v1.3.2
```
- [ ] Tag created
- [ ] Tag pushed (THIS TRIGGERS PIPELINE)

### 3. Monitor Pipeline
- [ ] GitHub Actions workflow started
- [ ] Restore dependencies successful
- [ ] Build successful
- [ ] Tests passed (252/252)
- [ ] NuGet pack successful
- [ ] Publish to NuGet.org successful
- [ ] GitHub Release created

### 4. Verify Publication
- [ ] Package appears on NuGet.org (15 min wait)
- [ ] GitHub Release created with files
- [ ] Test installation works

## Quick Commands

```powershell
cd C:\GitHub\MarkHazleton\WebSpark.HttpClientUtility
git add .
git commit -m "Release v1.3.2: Fix CurlCommandSaver tests and improve test reliability"
git push origin main
git tag -a v1.3.2 -m "Version 1.3.2 - Test fixes and improved reliability"
git push origin v1.3.2
start https://github.com/markhazleton/WebSpark.HttpClientUtility/actions
```

## Why It Will Work This Time

✅ **Tag Push**: We're pushing the v1.3.2 tag, which triggers `refs/tags/v*` condition
✅ **NuGet Config**: Workflow already has `signatureValidationMode accept` fix
✅ **Tests**: All 252 tests passing
✅ **Build**: Clean build with no errors

## Important Notes

⚠️ **v1.3.1 was skipped because:**
- The tag wasn't pushed (only main branch was pushed)
- Workflow only publishes when tag matches `refs/tags/v*.*.*`

✅ **v1.3.2 will publish because:**
- We're explicitly pushing the v1.3.2 tag
- Pipeline condition will match: `startsWith(github.ref, 'refs/tags/v')`

## Monitoring

**GitHub Actions:**
https://github.com/markhazleton/WebSpark.HttpClientUtility/actions

**NuGet Package:**
https://www.nuget.org/packages/WebSpark.HttpClientUtility/1.3.2

**GitHub Releases:**
https://github.com/markhazleton/WebSpark.HttpClientUtility/releases/tag/v1.3.2

## Success Criteria

- ✅ Workflow shows "Publish NuGet package" step completed
- ✅ Package 1.3.2 appears on NuGet.org
- ✅ GitHub Release v1.3.2 created
- ✅ Can install: `dotnet add package WebSpark.HttpClientUtility --version 1.3.2`
