---
layout: layouts/base.njk
title: About
description: About WebSpark.HttpClientUtility - purpose, architecture, and contributors.
permalink: /about/
---

# About WebSpark.HttpClientUtility

A production-ready NuGet library that eliminates boilerplate HTTP code while providing enterprise-grade features.

## Purpose

WebSpark.HttpClientUtility was created to solve common challenges when working with `HttpClient` in .NET applications:

- **Reduce Boilerplate**: Eliminate repetitive HTTP client setup code
- **Add Resilience**: Built-in retry logic and circuit breakers
- **Enable Caching**: Automatic response caching with minimal configuration
- **Improve Observability**: Comprehensive telemetry and distributed tracing
- **Simplify Testing**: Mock service for easy unit testing

## Architecture

The library uses a **decorator pattern** to compose features. This design allows you to:

- Enable only the features you need
- Maintain clean separation of concerns
- Extend with custom decorators
- Test each layer independently

### Decorator Chain

```
User Code
    ↓
IHttpRequestResultService (Interface)
    ↓
HttpRequestResultServiceTelemetry (Optional - Outermost)
    ↓
HttpRequestResultServicePolly (Optional - Resilience)
    ↓
HttpRequestResultServiceCache (Optional - Caching)
    ↓
HttpRequestResultService (Base - Core HTTP)
```

Each decorator wraps the layer below it, adding functionality while maintaining the same interface.

## Technology Stack

- **Frameworks**: .NET 8.0 (LTS), .NET 9.0
- **Resilience**: Polly for retry and circuit breaker patterns
- **Caching**: Microsoft.Extensions.Caching.Memory
- **Telemetry**: OpenTelemetry with OTLP support
- **Testing**: MSTest with Moq for mocking

## Version History

### v1.4.0 (Current)
- Added .NET 8 LTS support alongside .NET 9
- Simplified DI registration with multiple quick-start methods
- Improved documentation and test coverage

### v1.3.2
- Fixed test reliability and improved mock patterns
- Enhanced code quality with stricter analysis

### v1.3.1
- Enhanced CurlCommandSaver with batch processing
- Added file rotation for CURL command storage

### v1.3.0
- Removed deprecated Jaeger exporter
- Modernized OpenTelemetry to use OTLP standard
- Breaking change: Updated telemetry configuration

## Quality Assurance

- **252+ Passing Tests**: Comprehensive test coverage with MSTest
- **Strong Naming**: Assembly signed for enterprise scenarios
- **Nullable Reference Types**: Full nullability annotations
- **Code Analysis**: Warning level 5 with .NET analyzers enabled
- **Multi-Targeting**: Both .NET 8 LTS and .NET 9 supported

## Contributing

We welcome contributions! Here's how you can help:

1. **Report Issues**: Found a bug? [Open an issue](https://github.com/markhazleton/WebSpark.HttpClientUtility/issues)
2. **Suggest Features**: Have an idea? [Start a discussion](https://github.com/markhazleton/WebSpark.HttpClientUtility/discussions)
3. **Submit PRs**: Want to contribute code? [Read the contributing guide](https://github.com/markhazleton/WebSpark.HttpClientUtility/blob/main/documentation/CONTRIBUTING.md)

### Development Setup

```bash
# Clone the repository
git clone https://github.com/markhazleton/WebSpark.HttpClientUtility.git

# Restore dependencies
dotnet restore

# Build
dotnet build --configuration Release

# Run tests
dotnet test --configuration Release
```

## Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](https://github.com/markhazleton/WebSpark.HttpClientUtility/blob/main/CODE_OF_CONDUCT.md).

## License

MIT License - free for commercial and open-source use.

```
Copyright (c) 2024 Mark Hazleton

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
```

## Credits

### Author

**Mark Hazleton**
- GitHub: [@markhazleton](https://github.com/markhazleton)
- Website: [markhazleton.com](https://markhazleton.com)

### Built With

- [Polly](https://github.com/App-vNext/Polly) - Resilience and transient-fault-handling library
- [OpenTelemetry](https://opentelemetry.io/) - Observability framework
- [Microsoft.Extensions.Caching](https://docs.microsoft.com/en-us/dotnet/core/extensions/caching) - Caching abstractions
- [MSTest](https://github.com/microsoft/testfx) - Testing framework

## Acknowledgments

Thank you to all contributors and users who have helped make this library better through feedback, bug reports, and pull requests.

## Project Status

**Active**: This project is actively maintained and accepting contributions.

- ✅ Regular updates and bug fixes
- ✅ Support for latest .NET versions
- ✅ Responsive to issues and questions
- ✅ Open to feature requests

## Community

- 📦 [NuGet Package](https://www.nuget.org/packages/WebSpark.HttpClientUtility)
- 💻 [GitHub Repository](https://github.com/markhazleton/WebSpark.HttpClientUtility)
- 📖 [Documentation](https://markhazleton.github.io/WebSpark.HttpClientUtility/)
- 🐛 [Issue Tracker](https://github.com/markhazleton/WebSpark.HttpClientUtility/issues)
- 💬 [Discussions](https://github.com/markhazleton/WebSpark.HttpClientUtility/discussions)

## Statistics

- **Version**: {{ nuget.version }}
- **Total Downloads**: {{ nuget.displayDownloads }}
- **Target Frameworks**: .NET 8.0, .NET 9.0
- **Test Coverage**: 252+ tests passing
- **License**: MIT

## Next Steps

<div class="cta-buttons">
  <a href="{{ '/getting-started/' | url }}" class="button primary">Get Started</a>
  <a href="https://github.com/markhazleton/WebSpark.HttpClientUtility" class="button secondary">View on GitHub</a>
</div>
