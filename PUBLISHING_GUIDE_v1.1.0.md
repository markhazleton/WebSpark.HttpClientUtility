# WebSpark.HttpClientUtility v1.1.0 Publishing Guide

## Overview
This guide covers the complete process for publishing WebSpark.HttpClientUtility version 1.1.0 to NuGet.org.

## What's New in v1.1.0

### Breaking Changes
- **OpenTelemetry Modernization**: Removed deprecated `OpenTelemetry.Exporter.Jaeger` package
- **API Changes**: Updated `AddWebSparkOpenTelemetryWithExporters` method signature:
  - Removed `jaegerEndpoint` parameter
  - Simplified to use `otlpEndpoint` parameter only (supports Jaeger, Zipkin, and other OTLP-compatible systems)

### Improvements
- **Modern OTLP**: Migrated to OpenTelemetry Protocol (OTLP) exporter for better compatibility
- **Reduced Dependencies**: Eliminated deprecated package dependencies
- **Enhanced Documentation**: Updated OpenTelemetry configuration documentation
- **Build Stability**: Resolved compilation issues

## Pre-Publishing Checklist

✅ Version updated to 1.1.0 in WebSpark.HttpClientUtility.csproj
✅ CHANGELOG.md updated with v1.1.0 details
✅ All tests passing (79 tests succeeded)
✅ Release build successful
✅ NuGet package generated: WebSpark.HttpClientUtility.1.1.0.nupkg
✅ Symbol package generated: WebSpark.HttpClientUtility.1.1.0.snupkg

## Publishing Steps

### 1. Get NuGet API Key
- Go to [nuget.org](https://www.nuget.org/account/apikeys)
- Create a new API key with push permissions for WebSpark.HttpClientUtility
- Copy the API key for use in publishing

### 2. Publish to NuGet.org

```powershell
# Navigate to the project directory
cd "c:\GitHub\MarkHazleton\WebSpark.HttpClientUtility"

# Publish the main package
dotnet nuget push "WebSpark.HttpClientUtility\bin\Release\WebSpark.HttpClientUtility.1.1.0.nupkg" --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json

# Publish the symbol package (for debugging support)
dotnet nuget push "WebSpark.HttpClientUtility\bin\Release\WebSpark.HttpClientUtility.1.1.0.snupkg" --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

### 3. Verify Package Upload
- Check [nuget.org/packages/WebSpark.HttpClientUtility](https://www.nuget.org/packages/WebSpark.HttpClientUtility)
- Verify version 1.1.0 appears in the version history
- Confirm package metadata and dependencies are correct

### 4. Test Installation
```powershell
# Test installing the new version in a test project
dotnet add package WebSpark.HttpClientUtility --version 1.1.0
```

## Migration Guide for Users

### Breaking Changes in v1.1.0

#### OpenTelemetry Configuration Changes

**Before (v1.0.x):**
```csharp
services.AddWebSparkOpenTelemetryWithExporters(
    environment, 
    jaegerEndpoint: "http://localhost:14268/api/traces",
    otlpEndpoint: "http://localhost:4317"
);
```

**After (v1.1.0):**
```csharp
// For Jaeger users - use OTLP endpoint
services.AddWebSparkOpenTelemetryWithExporters(
    environment, 
    otlpEndpoint: "http://localhost:4317"  // Jaeger OTLP endpoint
);

// For other OTLP-compatible systems
services.AddWebSparkOpenTelemetryWithExporters(
    environment, 
    otlpEndpoint: "your-otlp-endpoint"
);
```

#### Jaeger Configuration

If you were using Jaeger, you now need to configure it to accept OTLP:

**Jaeger with OTLP:**
```yaml
# docker-compose.yml example
services:
  jaeger:
    image: jaegertracing/all-in-one:latest
    ports:
      - "16686:16686"  # Jaeger UI
      - "4317:4317"    # OTLP gRPC
      - "4318:4318"    # OTLP HTTP
    environment:
      - COLLECTOR_OTLP_ENABLED=true
```

## Post-Publishing Tasks

### 1. Update Documentation
- [ ] Update README.md with v1.1.0 examples
- [ ] Update any migration guides
- [ ] Update API documentation if hosted separately

### 2. GitHub Release
- [ ] Create a new GitHub release for v1.1.0
- [ ] Include changelog content in release notes
- [ ] Tag the release appropriately

### 3. Communication
- [ ] Announce breaking changes to users
- [ ] Update any tutorials or blog posts
- [ ] Consider creating migration guide for existing users

## File Locations

- **NuGet Package**: `WebSpark.HttpClientUtility\bin\Release\WebSpark.HttpClientUtility.1.1.0.nupkg`
- **Symbol Package**: `WebSpark.HttpClientUtility\bin\Release\WebSpark.HttpClientUtility.1.1.0.snupkg`
- **Project File**: `WebSpark.HttpClientUtility\WebSpark.HttpClientUtility.csproj`
- **Changelog**: `CHANGELOG.md`

## Support

For issues with v1.1.0:
- GitHub Issues: [Repository Issues](https://github.com/MarkHazleton/HttpClientUtility/issues)
- Documentation: [Project Documentation](https://MarkHazleton.com/MarkHazleton/HttpClientUtility)
