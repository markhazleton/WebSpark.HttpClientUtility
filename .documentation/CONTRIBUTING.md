# Contributing to WebSpark.HttpClientUtility

First off, thank you for considering contributing to WebSpark.HttpClientUtility! It's people like you that make this library better for everyone.

## Code of Conduct

This project and everyone participating in it is governed by our commitment to providing a welcoming and inspiring community for all. Please be respectful and constructive in your interactions.

## How Can I Contribute?

### Reporting Bugs

Before creating bug reports, please check the existing issues to avoid duplicates. When you create a bug report, include as many details as possible:

- **Use a clear and descriptive title**
- **Describe the exact steps to reproduce the problem**
- **Provide specific examples** to demonstrate the steps
- **Describe the behavior you observed** and explain why it's a problem
- **Explain the expected behavior**
- **Include code samples** and stack traces if applicable
- **Include your environment details**: .NET version, OS, etc.

### Suggesting Enhancements

Enhancement suggestions are tracked as GitHub issues. When creating an enhancement suggestion:

- **Use a clear and descriptive title**
- **Provide a detailed description** of the suggested enhancement
- **Explain why this enhancement would be useful**
- **List any similar features** in other libraries if applicable
- **Include code examples** if possible

### Pull Requests

1. **Fork the repository** and create your branch from `main`
2. **Make your changes** following our coding standards
3. **Add tests** if you're adding or changing functionality
4. **Ensure all tests pass**: `dotnet test`
5. **Ensure the build succeeds**: `dotnet build`
6. **Update documentation** if you're changing functionality
7. **Write a clear commit message** following conventional commits
8. **Submit your pull request**

## Development Setup

### Prerequisites

- .NET 9 SDK
- Visual Studio 2022+ or Visual Studio Code with C# extension
- Git

### Getting Started

```bash
# Clone your fork
git clone https://github.com/YOUR-USERNAME/HttpClientUtility.git
cd HttpClientUtility

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run tests
dotnet test
```

## Coding Standards

### General Guidelines

- **Follow C# coding conventions**: Use PascalCase for public members, camelCase for private fields
- **Enable nullable reference types**: All new code must handle nullability
- **Add XML documentation**: All public APIs must have XML documentation comments
- **Use meaningful names**: Variables, methods, and classes should have descriptive names
- **Keep methods focused**: Each method should do one thing well
- **Avoid magic numbers**: Use named constants instead

### Code Style

```csharp
// Good example
/// <summary>
/// Sends an HTTP GET request to the specified URL.
/// </summary>
/// <param name="url">The target URL for the request.</param>
/// <param name="cancellationToken">Token to cancel the operation.</param>
/// <returns>The HTTP response result.</returns>
public async Task<HttpRequestResult<T>> GetAsync<T>(
    string url, 
    CancellationToken cancellationToken = default)
{
    ArgumentNullException.ThrowIfNull(url);
 
    // Implementation
}
```

### Testing Standards

- **Write comprehensive tests**: Cover happy paths, edge cases, and error conditions
- **Use descriptive test names**: `MethodName_Scenario_ExpectedBehavior`
- **Follow AAA pattern**: Arrange, Act, Assert
- **Use MSTest assertions**: Prefer modern assertions like `Assert.Contains`
- **Mock external dependencies**: Use Moq or similar for isolation

```csharp
[TestMethod]
public async Task GetAsync_WithValidUrl_ReturnsSuccessResult()
{
    // Arrange
    var service = CreateService();
    var url = "https://api.example.com/data";
    
    // Act
    var result = await service.GetAsync<TestData>(url);
    
    // Assert
    Assert.IsTrue(result.IsSuccessStatusCode);
    Assert.IsNotNull(result.ResponseResults);
}
```

### Error Handling

- **Use appropriate exception types**: Don't catch generic `Exception` unless necessary
- **Provide context in exceptions**: Include relevant information
- **Use ConfigureAwait(false)**: For library code to avoid deadlocks
- **Pass CancellationToken**: Support cancellation in async methods

### Async/Await Best Practices

- **Avoid async void**: Except for event handlers
- **Use ConfigureAwait(false)**: In library code
- **Cancel properly**: Support and honor CancellationToken
- **Don't block async code**: Never use `.Result` or `.Wait()`

## Documentation

### XML Documentation

All public APIs require XML documentation:

```csharp
/// <summary>
/// Brief description of what the method does.
/// </summary>
/// <param name="paramName">Description of the parameter.</param>
/// <returns>Description of what is returned.</returns>
/// <exception cref="ArgumentNullException">When parameter is null.</exception>
/// <remarks>
/// Additional information, usage examples, or important notes.
/// </remarks>
public void MyMethod(string paramName)
{
    // Implementation
}
```

### README Updates

If your changes affect usage:

- Update the README.md with new examples
- Add new features to the "Key Features" section
- Update code samples to reflect changes

### CHANGELOG Updates

Add an entry to CHANGELOG.md under `[Unreleased]`:

```markdown
### Added
- New feature description (#PR-number)

### Changed
- Description of change (#PR-number)

### Fixed
- Bug fix description (#PR-number)
```

## Commit Message Guidelines

We follow [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- **feat**: A new feature
- **fix**: A bug fix
- **docs**: Documentation only changes
- **style**: Code style changes (formatting, missing semi-colons, etc.)
- **refactor**: Code change that neither fixes a bug nor adds a feature
- **perf**: Performance improvements
- **test**: Adding or updating tests
- **chore**: Maintenance tasks, dependency updates

### Examples

```
feat(crawler): add support for custom user agents

- Add UserAgent property to CrawlerOptions
- Update documentation with new option
- Add tests for custom user agent

Closes #123
```

```
fix(cache): resolve race condition in cache expiration

The cache manager was not thread-safe when checking expiration.
This fix adds proper locking around the expiration check.

Fixes #456
```

## Release Process

Releases are managed by maintainers:

1. Update version in `.csproj`
2. Update CHANGELOG.md with release date
3. Create a GitHub release with tag
4. NuGet package is published automatically via CI/CD

## Questions?

- Open an issue with the `question` label
- Reach out to maintainers via GitHub discussions

## Recognition

Contributors will be recognized in:
- GitHub contributors page
- Release notes for significant contributions
- README acknowledgments section (for major features)

---

Thank you for contributing to WebSpark.HttpClientUtility! ??
