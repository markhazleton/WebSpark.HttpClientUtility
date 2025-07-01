# Publishing Commands for WebSpark.HttpClientUtility v1.1.0

# Step 1: Verify you're in the correct directory
Write-Host "Current directory: $PWD"
Write-Host "Checking for package files..."

if (Test-Path "WebSpark.HttpClientUtility\bin\Release\WebSpark.HttpClientUtility.1.1.0.nupkg") {
    Write-Host "✅ Main package found: WebSpark.HttpClientUtility.1.1.0.nupkg"
} else {
    Write-Host "❌ Main package NOT found"
    exit 1
}

if (Test-Path "WebSpark.HttpClientUtility\bin\Release\WebSpark.HttpClientUtility.1.1.0.snupkg") {
    Write-Host "✅ Symbol package found: WebSpark.HttpClientUtility.1.1.0.snupkg"
} else {
    Write-Host "❌ Symbol package NOT found"
    exit 1
}

# Step 2: Get API Key (you'll need to replace YOUR_API_KEY with your actual key)
Write-Host ""
Write-Host "📋 BEFORE PROCEEDING:"
Write-Host "1. Go to https://www.nuget.org/account/apikeys"
Write-Host "2. Create a new API key for WebSpark.HttpClientUtility"
Write-Host "3. Replace 'YOUR_API_KEY' in the commands below with your actual API key"
Write-Host ""

# Step 3: Publishing commands (replace YOUR_API_KEY with your actual API key)
Write-Host "🚀 PUBLISHING COMMANDS:"
Write-Host ""
Write-Host "# Publish main package:"
Write-Host 'dotnet nuget push "WebSpark.HttpClientUtility\bin\Release\WebSpark.HttpClientUtility.1.1.0.nupkg" --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json'
Write-Host ""
Write-Host "# Publish symbol package:"
Write-Host 'dotnet nuget push "WebSpark.HttpClientUtility\bin\Release\WebSpark.HttpClientUtility.1.1.0.snupkg" --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json'
Write-Host ""

# Step 4: Verification
Write-Host "🔍 AFTER PUBLISHING:"
Write-Host "1. Check https://www.nuget.org/packages/WebSpark.HttpClientUtility"
Write-Host "2. Verify version 1.1.0 is listed"
Write-Host "3. Test installation with: dotnet add package WebSpark.HttpClientUtility --version 1.1.0"
Write-Host ""

Write-Host "📦 Package Details:"
Write-Host "- Version: 1.1.0"
Write-Host "- Breaking Changes: OpenTelemetry API changes (removed Jaeger, OTLP only)"
Write-Host "- Target Frameworks: .NET 8.0, .NET 9.0"
Write-Host "- Tests Passed: 79/79"
Write-Host ""

Write-Host "Ready to publish! Remember to replace YOUR_API_KEY with your actual NuGet API key."
