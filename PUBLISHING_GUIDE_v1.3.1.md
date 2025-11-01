# Publishing Guide for WebSpark.HttpClientUtility v1.3.1

## Release Summary

**Version:** 1.3.1 (Patch Release)  
**Release Date:** January 2025  
**Target Framework:** .NET 9.0

### Key Changes in This Release

1. **Enhanced CurlCommandSaver**
   - Batch processing with configurable batch size (default: 100)
   - Automatic file rotation at 10MB (configurable)
   - Sensitive data sanitization (headers and JSON content)
   - Retry logic with exponential backoff
   - Comprehensive configuration options

2. **Improved Resource Management**
   - Proper disposal patterns with IDisposable
   - Timer cleanup in batch processing
   - Thread-safe file operations

3. **Better Error Handling**
   - Detailed logging with correlation IDs
   - Robust retry mechanism
   - Graceful degradation when file logging is disabled

## Pre-Publishing Checklist

### ✅ Code Changes Complete
- [x] Version incremented to 1.3.1 in .csproj file
- [x] PackageReleaseNotes updated in .csproj file
- [x] CHANGELOG.md updated with v1.3.1 details
- [x] README.md updated with new version and features
- [x] Build successful with zero errors

### 🔍 Quality Checks

Before publishing, verify the following:

1. **Run All Tests**
   ```powershell
   dotnet test
   ```

2. **Build in Release Configuration**
   ```powershell
   dotnet build -c Release
   ```

3. **Pack the NuGet Package**
   ```powershell
   cd WebSpark.HttpClientUtility
   dotnet pack -c Release -o ./nupkg
   ```

4. **Inspect the Package**
   - Check the generated .nupkg file in `./nupkg` folder
   - Verify version number is 1.3.1
   - Verify package includes README.md, icon.png, and documentation files
   - Check that symbols package (.snupkg) is generated

## Publishing Process

### Step 1: Verify NuGet API Key

Ensure you have a valid NuGet API key:

```powershell
# Check if you have the key set
dotnet nuget list source
```

If you need to add your API key:

```powershell
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org -u YOUR_USERNAME -p YOUR_API_KEY --store-password-in-clear-text
```

Or set it as an environment variable:

```powershell
$env:NUGET_API_KEY = "your-api-key-here"
```

### Step 2: Clean Previous Builds

```powershell
# Navigate to the project directory
cd C:\GitHub\MarkHazleton\WebSpark.HttpClientUtility\WebSpark.HttpClientUtility

# Clean previous builds
dotnet clean
Remove-Item -Path ./nupkg -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path ./bin -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path ./obj -Recurse -Force -ErrorAction SilentlyContinue
```

### Step 3: Build and Pack

```powershell
# Restore dependencies
dotnet restore

# Build in Release mode
dotnet build -c Release

# Run tests
dotnet test -c Release

# Pack the NuGet package
dotnet pack -c Release -o ./nupkg
```

Expected output should include:
```
Successfully created package 'C:\GitHub\MarkHazleton\WebSpark.HttpClientUtility\WebSpark.HttpClientUtility\nupkg\WebSpark.HttpClientUtility.1.3.1.nupkg'.
Successfully created package 'C:\GitHub\MarkHazleton\WebSpark.HttpClientUtility\WebSpark.HttpClientUtility\nupkg\WebSpark.HttpClientUtility.1.3.1.snupkg'.
```

### Step 4: Validate Package Locally

Before publishing, test the package locally:

```powershell
# Add a local NuGet source
dotnet nuget add source ./nupkg -n local

# Create a test project
mkdir TestPackage
cd TestPackage
dotnet new console
dotnet add package WebSpark.HttpClientUtility --version 1.3.1 --source ../WebSpark.HttpClientUtility/nupkg

# Test if package installs correctly
dotnet restore
dotnet build

# Clean up test project
cd ..
Remove-Item -Path TestPackage -Recurse -Force
```

### Step 5: Publish to NuGet.org

**IMPORTANT:** Once published to NuGet.org, packages cannot be deleted, only unlisted!

```powershell
# Navigate back to project directory if needed
cd C:\GitHub\MarkHazleton\WebSpark.HttpClientUtility\WebSpark.HttpClientUtility

# Publish the package (replace YOUR_API_KEY with your actual key)
dotnet nuget push ./nupkg/WebSpark.HttpClientUtility.1.3.1.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json

# Also push the symbols package for debugging support
dotnet nuget push ./nupkg/WebSpark.HttpClientUtility.1.3.1.snupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

Or using environment variable:

```powershell
dotnet nuget push ./nupkg/WebSpark.HttpClientUtility.1.3.1.nupkg --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json
dotnet nuget push ./nupkg/WebSpark.HttpClientUtility.1.3.1.snupkg --api-key $env:NUGET_API_KEY --source https://api.nuget.org/v3/index.json
```

### Step 6: Verify Publication

1. **Check NuGet.org**
   - Visit: https://www.nuget.org/packages/WebSpark.HttpClientUtility/
   - Verify version 1.3.1 appears (may take 5-15 minutes to index)

2. **Check Package Details**
   - Verify package metadata is correct
   - Check that README displays properly
   - Verify icon appears
   - Check dependencies are listed correctly

3. **Test Installation**
   ```powershell
   # In a new test project
   dotnet new console -n TestInstall
   cd TestInstall
   dotnet add package WebSpark.HttpClientUtility --version 1.3.1
   dotnet build
   ```

## Post-Publishing Tasks

### Git Tagging and Release

```powershell
# Commit all changes
git add .
git commit -m "Release v1.3.1: Enhanced CurlCommandSaver with batch processing and sanitization"

# Create and push tag
git tag -a v1.3.1 -m "Version 1.3.1 - Enhanced CurlCommandSaver"
git push origin v1.3.1
git push origin main
```

### GitHub Release

1. Go to: https://github.com/markhazleton/WebSpark.HttpClientUtility/releases/new
2. Select tag: v1.3.1
3. Release title: `v1.3.1 - Enhanced CurlCommandSaver`
4. Description:

```markdown
## What's New in v1.3.1

### Enhanced CurlCommandSaver
- ✨ Batch processing with configurable batch size (default: 100 commands)
- 📁 Automatic file rotation at configurable size (default: 10MB)
- 🔒 Sensitive data sanitization for headers and JSON content
- 🔄 Retry logic with exponential backoff for file operations
- ⚙️ Comprehensive configuration options via `CurlCommandSaverOptions`

### Improvements
- Better resource management with proper disposal patterns
- Enhanced error handling with detailed logging
- Thread-safe file operations using SemaphoreSlim
- Improved performance with batch processing timer

### Configuration Example

```csharp
services.Configure<CurlCommandSaverOptions>(options =>
{
    options.OutputFolder = "logs/curl";
    options.MaxFileSize = 20 * 1024 * 1024; // 20MB
    options.UseBatchProcessing = true;
    options.BatchSize = 100;
    options.SanitizeSensitiveInfo = true;
});
```

See [CHANGELOG.md](CHANGELOG.md) for full details.

## Installation

```bash
dotnet add package WebSpark.HttpClientUtility --version 1.3.1
```

## Packages

- **Main Package**: WebSpark.HttpClientUtility.1.3.1.nupkg
- **Symbols Package**: WebSpark.HttpClientUtility.1.3.1.snupkg
```

5. Attach the following files:
   - WebSpark.HttpClientUtility.1.3.1.nupkg
   - WebSpark.HttpClientUtility.1.3.1.snupkg
   - Source code (zip)
   - Source code (tar.gz)

6. Click "Publish release"

### Update Documentation

1. Update project website if applicable
2. Update any integration guides or tutorials
3. Notify stakeholders or community:
   - Blog post
   - Twitter/LinkedIn announcement
   - Discord/Slack channels
   - Email newsletter

## Troubleshooting

### Common Issues

**Issue:** Package already exists at version 1.3.1
```
Response status code does not indicate success: 409 (Conflict - The package version already exists)
```
**Solution:** You cannot republish the same version. Increment to 1.3.2 and try again.

---

**Issue:** API key is invalid
```
Response status code does not indicate success: 403 (Forbidden)
```
**Solution:** 
1. Verify your API key is correct
2. Check that the API key has "Push new packages" permission
3. Generate a new API key if needed: https://www.nuget.org/account/apikeys

---

**Issue:** Package is too large
```
The package is too large. The maximum allowed size is 250 MB.
```
**Solution:** 
1. Ensure you're not including unnecessary files
2. Check .nuspec or .csproj for files being included
3. Remove large test files or assets

---

**Issue:** Missing required metadata
```
The package is missing required metadata properties
```
**Solution:** Verify all required properties in .csproj:
- PackageId
- Version
- Authors
- Description
- PackageLicenseExpression or PackageLicenseFile

## Rollback Plan

If you discover a critical issue after publishing:

1. **Unlist the Package** (doesn't delete, but hides from search)
   ```powershell
   dotnet nuget delete WebSpark.HttpClientUtility 1.3.1 --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json --non-interactive
   ```

2. **Fix the Issue**
   - Make necessary code changes
   - Increment version to 1.3.2
   - Update CHANGELOG.md

3. **Publish Fixed Version**
   - Follow the publishing process for v1.3.2
   - Include note in release notes about the fix

## Version History

| Version | Date | Status | Notes |
|---------|------|--------|-------|
| 1.3.1 | 2025-01-XX | ✅ Current | Enhanced CurlCommandSaver |
| 1.3.0 | 2025-07-01 | ✅ Released | .NET 9 only, 75+ new tests |
| 1.2.0 | 2025-XX-XX | ✅ Released | Updated dependencies |
| 1.1.0 | 2025-07-01 | ✅ Released | OpenTelemetry modernization |

## Support

- **Issues**: https://github.com/markhazleton/WebSpark.HttpClientUtility/issues
- **Discussions**: https://github.com/markhazleton/WebSpark.HttpClientUtility/discussions
- **Email**: mark@markhazleton.com

## License

This project is licensed under the MIT License.
