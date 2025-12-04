# WebSpark.HttpClientUtility.Testing

**Test helpers and fakes for WebSpark.HttpClientUtility - Make HTTP testing simple**

## Installation

```bash
dotnet add package WebSpark.HttpClientUtility.Testing
```

## Quick Start

### Mocking HTTP Responses

```csharp
using WebSpark.HttpClientUtility.Testing.Fakes;

// Arrange - Create fake handler
var fakeHandler = new FakeHttpResponseHandler()
    .ForRequest("/api/weather")
    .RespondWith(HttpStatusCode.OK, new WeatherData("Seattle", 65));

var httpClient = new HttpClient(fakeHandler);
var service = new WeatherService(httpClient);

// Act
var result = await service.GetWeatherAsync("Seattle");

// Assert
Assert.Equal(65, result.Temp);
```

### Testing Retry Behavior

```csharp
// Arrange - First 2 calls fail, 3rd succeeds
var fakeHandler = new FakeHttpResponseHandler()
    .ForRequest("/api/weather")
    .RespondWith(HttpStatusCode.ServiceUnavailable)
    .ThenOnNextRequest()
    .RespondWith(HttpStatusCode.ServiceUnavailable)
    .ThenOnNextRequest()
    .RespondWith(HttpStatusCode.OK, new WeatherData("Seattle", 65));

// Act
var result = await service.GetWeatherAsync("Seattle");

// Assert
Assert.NotNull(result);
fakeHandler.VerifyRequestCount("/api/weather", Times.Exactly(3));
```

### Simulating Latency

```csharp
var fakeHandler = new FakeHttpResponseHandler()
    .ForRequest("/api/weather")
    .WithDelay(TimeSpan.FromMilliseconds(500))
    .RespondWith(HttpStatusCode.OK, new WeatherData("Seattle", 65));

// Request will take 500ms to complete
var result = await service.GetWeatherAsync("Seattle");
```

### Verifying Requests

```csharp
// Verify specific request was made
fakeHandler.VerifyRequest(req =>
    req.RequestUri.AbsolutePath == "/api/weather" &&
    req.Method == HttpMethod.Get,
    times: Times.Once()
);

// Verify header was sent
fakeHandler.VerifyHeader("X-API-Key", "secret-key");

// Verify request count
fakeHandler.VerifyRequestCount("/api/weather", Times.Exactly(3));
```

## Features

- **FakeHttpResponseHandler** - Mock HTTP responses without network calls
- **Fluent API** - Easy-to-read test setup
- **Sequential Responses** - Configure different responses for multiple calls
- **Request Verification** - Assert requests were made correctly
- **Latency Simulation** - Test timeout scenarios
- **Pattern Matching** - Match requests by URL, method, headers, or custom predicates

## Documentation

For complete documentation, visit: [https://markhazleton.github.io/WebSpark.HttpClientUtility/testing/](https://markhazleton.github.io/WebSpark.HttpClientUtility/testing/)

## Requirements

- .NET 8, .NET 9, or .NET 10
- WebSpark.HttpClientUtility 2.1.0 or higher

## License

MIT License - see [LICENSE](https://github.com/MarkHazleton/HttpClientUtility/blob/main/LICENSE) for details.
