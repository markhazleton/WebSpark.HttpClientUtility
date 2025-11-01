# Quick Publishing Checklist - v1.3.1

## Pre-Flight Checks ✈️

- [x] Version updated to 1.3.1 in .csproj
- [x] CHANGELOG.md updated
- [x] README.md updated
- [x] Build successful
- [ ] All tests passing
- [ ] Local package validation complete

## Publishing Commands 🚀

### 1. Clean and Prepare
```powershell
cd C:\GitHub\MarkHazleton\WebSpark.HttpClientUtility\WebSpark.HttpClientUtility
dotnet clean
dotnet restore
```

### 2. Build and Test
```powershell
dotnet build -c Release
dotnet test -c Release
```

### 3. Pack
```powershell
dotnet pack -c Release -o ./nupkg
```

### 4. Publish to NuGet
```powershell
# Set your API key (one time)
$env:NUGET_API_KEY = "your-api-key-here"

# Push main package
dotnet nuget push ./nupkg/WebSpark.HttpClientUtility.1.3.1.nupkg --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json

# Push symbols package
dotnet nuget push ./nupkg/WebSpark.HttpClientUtility.1.3.1.snupkg --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json
```

### 5. Git Tag and Push
```powershell
git add .
git commit -m "Release v1.3.1: Enhanced CurlCommandSaver with batch processing and sanitization"
git tag -a v1.3.1 -m "Version 1.3.1 - Enhanced CurlCommandSaver"
git push origin v1.3.1
git push origin main
```

## Post-Publishing ✅

- [ ] Verify package on NuGet.org: https://www.nuget.org/packages/WebSpark.HttpClientUtility/1.3.1
- [ ] Create GitHub Release
- [ ] Test installation in new project
- [ ] Update project website/documentation
- [ ] Announce release (optional)

## Quick Test Installation
```powershell
mkdir TestInstall
cd TestInstall
dotnet new console
dotnet add package WebSpark.HttpClientUtility --version 1.3.1
dotnet build
cd ..
Remove-Item -Path TestInstall -Recurse -Force
```

## Emergency Rollback
```powershell
# Unlist package if critical issue found
dotnet nuget delete WebSpark.HttpClientUtility 1.3.1 --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json --non-interactive
```

## Notes
- NuGet indexing takes 5-15 minutes
- Packages cannot be deleted, only unlisted
- Always test locally before publishing
- Keep API key secure and never commit it

## Links
- Package: https://www.nuget.org/packages/WebSpark.HttpClientUtility/
- Repository: https://github.com/markhazleton/WebSpark.HttpClientUtility
- Issues: https://github.com/markhazleton/WebSpark.HttpClientUtility/issues
