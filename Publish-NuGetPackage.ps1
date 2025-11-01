# Publish-NuGetPackage.ps1
# Automated NuGet Publishing Script for WebSpark.HttpClientUtility v1.3.1

param(
    [Parameter(Mandatory=$false)]
    [string]$ApiKey = $env:NUGET_API_KEY,
    
 [Parameter(Mandatory=$false)]
 [switch]$DryRun = $false,
  
    [Parameter(Mandatory=$false)]
    [switch]$SkipTests = $false,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipGit = $false
)

$ErrorActionPreference = "Stop"
$Version = "1.3.1"
$ProjectPath = "C:\GitHub\MarkHazleton\WebSpark.HttpClientUtility\WebSpark.HttpClientUtility"
$PackageName = "WebSpark.HttpClientUtility"

# Color functions
function Write-Success { Write-Host "✓ $args" -ForegroundColor Green }
function Write-Info { Write-Host "ℹ $args" -ForegroundColor Cyan }
function Write-Warning { Write-Host "⚠ $args" -ForegroundColor Yellow }
function Write-Error { Write-Host "✗ $args" -ForegroundColor Red }
function Write-Step { Write-Host "`n=== $args ===" -ForegroundColor Magenta }

# Banner
Write-Host "`n" -NoNewline
Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║               ║" -ForegroundColor Cyan
Write-Host "║ WebSpark.HttpClientUtility NuGet Publisher v1.3.1     ║" -ForegroundColor Cyan
Write-Host "║        ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

if ($DryRun) {
 Write-Warning "DRY RUN MODE - No actual publishing will occur"
}

# Check API Key
Write-Step "Checking Prerequisites"
if ([string]::IsNullOrEmpty($ApiKey)) {
    Write-Error "NuGet API Key not found!"
  Write-Info "Set it with: `$env:NUGET_API_KEY = 'your-key'"
    Write-Info "Or pass it as parameter: -ApiKey 'your-key'"
    exit 1
}
Write-Success "API Key found"

# Navigate to project
Write-Step "Navigating to Project Directory"
if (-not (Test-Path $ProjectPath)) {
    Write-Error "Project path not found: $ProjectPath"
    exit 1
}
Set-Location $ProjectPath
Write-Success "Located project at: $ProjectPath"

# Clean previous builds
Write-Step "Cleaning Previous Builds"
if (Test-Path "./nupkg") {
    Remove-Item -Path "./nupkg" -Recurse -Force
    Write-Success "Cleaned nupkg directory"
}
if (Test-Path "./bin") {
    Remove-Item -Path "./bin" -Recurse -Force
    Write-Success "Cleaned bin directory"
}
if (Test-Path "./obj") {
    Remove-Item -Path "./obj" -Recurse -Force
    Write-Success "Cleaned obj directory"
}

# Restore dependencies
Write-Step "Restoring Dependencies"
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Error "Restore failed"
    exit 1
}
Write-Success "Dependencies restored"

# Build
Write-Step "Building Project (Release Configuration)"
dotnet build -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed"
    exit 1
}
Write-Success "Build completed successfully"

# Run tests
if (-not $SkipTests) {
 Write-Step "Running Tests"
    dotnet test -c Release --no-build
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Tests failed"
        exit 1
    }
    Write-Success "All tests passed"
} else {
    Write-Warning "Skipping tests (not recommended for production)"
}

# Pack
Write-Step "Creating NuGet Package"
dotnet pack -c Release -o ./nupkg --no-build
if ($LASTEXITCODE -ne 0) {
  Write-Error "Pack failed"
    exit 1
}
Write-Success "Package created"

# Verify package files
$mainPackage = "./nupkg/$PackageName.$Version.nupkg"
$symbolsPackage = "./nupkg/$PackageName.$Version.snupkg"

if (-not (Test-Path $mainPackage)) {
    Write-Error "Main package not found: $mainPackage"
    exit 1
}
if (-not (Test-Path $symbolsPackage)) {
    Write-Warning "Symbols package not found: $symbolsPackage"
}

$mainSize = (Get-Item $mainPackage).Length
$symbolsSize = if (Test-Path $symbolsPackage) { (Get-Item $symbolsPackage).Length } else { 0 }

Write-Success "Main package: $([math]::Round($mainSize/1KB, 2)) KB"
if ($symbolsSize -gt 0) {
    Write-Success "Symbols package: $([math]::Round($symbolsSize/1KB, 2)) KB"
}

# Publish
if (-not $DryRun) {
    Write-Step "Publishing to NuGet.org"
    Write-Warning "This action cannot be undone!"
    
    $confirmation = Read-Host "Type 'yes' to confirm publishing v$Version to NuGet.org"
    if ($confirmation -ne 'yes') {
        Write-Warning "Publishing cancelled by user"
   exit 0
    }
    
    Write-Info "Publishing main package..."
dotnet nuget push $mainPackage --api-key $ApiKey --source https://api.nuget.org/v3/index.json
    if ($LASTEXITCODE -ne 0) {
     Write-Error "Failed to publish main package"
        exit 1
    }
    Write-Success "Main package published"
    
    if (Test-Path $symbolsPackage) {
        Write-Info "Publishing symbols package..."
        dotnet nuget push $symbolsPackage --api-key $ApiKey --source https://api.nuget.org/v3/index.json
        if ($LASTEXITCODE -ne 0) {
         Write-Warning "Failed to publish symbols package (non-critical)"
        } else {
       Write-Success "Symbols package published"
        }
    }
} else {
    Write-Info "DRY RUN: Would publish packages to NuGet.org"
}

# Git operations
if (-not $SkipGit -and -not $DryRun) {
    Write-Step "Git Operations"
    
    # Check for uncommitted changes
  $gitStatus = git status --porcelain
    if ($gitStatus) {
        Write-Info "Uncommitted changes detected. Committing..."
        git add .
 git commit -m "Release v$Version`: Enhanced CurlCommandSaver with batch processing and sanitization"
    if ($LASTEXITCODE -eq 0) {
         Write-Success "Changes committed"
        }
    } else {
     Write-Info "No uncommitted changes"
    }
    
    # Create tag
    Write-Info "Creating git tag v$Version..."
    git tag -a "v$Version" -m "Version $Version - Enhanced CurlCommandSaver"
    if ($LASTEXITCODE -eq 0) {
      Write-Success "Tag created"
    }
    
    # Push
  $pushConfirm = Read-Host "Push changes to origin? (yes/no)"
    if ($pushConfirm -eq 'yes') {
        Write-Info "Pushing to origin..."
        git push origin "v$Version"
        git push origin main
     if ($LASTEXITCODE -eq 0) {
         Write-Success "Pushed to origin"
        }
    }
} elseif ($SkipGit) {
    Write-Warning "Skipping git operations"
} else {
    Write-Info "DRY RUN: Would perform git operations"
}

# Summary
Write-Step "Publication Summary"
Write-Success "Package: $PackageName"
Write-Success "Version: $Version"
Write-Success "Status: " + $(if ($DryRun) { "DRY RUN COMPLETE" } else { "PUBLISHED" })

if (-not $DryRun) {
    Write-Host "`n" -NoNewline
    Write-Info "Next steps:"
    Write-Info "1. Wait 5-15 minutes for NuGet indexing"
    Write-Info "2. Verify at: https://www.nuget.org/packages/$PackageName/$Version"
    Write-Info "3. Create GitHub release: https://github.com/markhazleton/WebSpark.HttpClientUtility/releases/new"
    Write-Info "4. Test installation: dotnet add package $PackageName --version $Version"
}

Write-Host "`n" -NoNewline
Write-Host "╔════════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║         ║" -ForegroundColor Green
Write-Host "║        ✓ Process Complete!     ║" -ForegroundColor Green
Write-Host "║      ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""

# Examples of usage
<#
.SYNOPSIS
    Publishes WebSpark.HttpClientUtility NuGet package

.DESCRIPTION
    Automates the process of building, testing, packing, and publishing
    the WebSpark.HttpClientUtility NuGet package.

.PARAMETER ApiKey
    NuGet API key (optional if NUGET_API_KEY environment variable is set)

.PARAMETER DryRun
    Run all steps except actual publishing (for testing the script)

.PARAMETER SkipTests
    Skip running tests (not recommended for production)

.PARAMETER SkipGit
    Skip git commit, tag, and push operations

.EXAMPLE
    .\Publish-NuGetPackage.ps1
 # Uses environment variable for API key, runs all steps

.EXAMPLE
    .\Publish-NuGetPackage.ps1 -ApiKey "your-key-here"
    # Uses provided API key

.EXAMPLE
    .\Publish-NuGetPackage.ps1 -DryRun
    # Test run without actually publishing

.EXAMPLE
    .\Publish-NuGetPackage.ps1 -SkipTests -SkipGit
    # Publish without running tests or git operations (not recommended)
#>
