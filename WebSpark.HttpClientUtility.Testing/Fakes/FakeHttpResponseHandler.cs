using System.Net;
using System.Text;
using System.Text.Json;

namespace WebSpark.HttpClientUtility.Testing.Fakes;

/// <summary>
/// Fake HTTP message handler for testing HTTP requests without making actual network calls.
/// Allows configuring responses based on request patterns.
/// </summary>
public class FakeHttpResponseHandler : HttpMessageHandler
{
    private readonly List<ResponseConfiguration> _configurations = new();
    private readonly List<HttpRequestMessage> _requests = new();
    private int _currentConfigIndex = 0;

    /// <summary>
    /// Gets all requests that have been sent through this handler.
    /// </summary>
    public IReadOnlyList<HttpRequestMessage> Requests => _requests.AsReadOnly();

    /// <summary>
    /// Configure a response for requests matching a specific pattern.
    /// </summary>
    /// <param name="requestPath">The request path to match (can be partial)</param>
    /// <returns>A fluent configuration builder</returns>
    public ResponseConfigurationBuilder ForRequest(string requestPath)
    {
        return new ResponseConfigurationBuilder(this, req => req.RequestUri?.AbsolutePath.Contains(requestPath) == true);
    }

    /// <summary>
    /// Configure a response for requests matching a predicate.
    /// </summary>
    /// <param name="predicate">The predicate to match requests</param>
    /// <returns>A fluent configuration builder</returns>
    public ResponseConfigurationBuilder ForRequest(Func<HttpRequestMessage, bool> predicate)
    {
        return new ResponseConfigurationBuilder(this, predicate);
    }

    /// <summary>
    /// Verify that a request was made matching the predicate.
    /// </summary>
    /// <param name="predicate">The predicate to match</param>
    /// <param name="times">Expected number of times (null for at least once)</param>
    public void VerifyRequest(Func<HttpRequestMessage, bool> predicate, Times? times = null)
    {
        var matchingRequests = _requests.Where(predicate).ToList();
        var count = matchingRequests.Count;

        if (times == null && count == 0)
        {
            throw new InvalidOperationException("Expected at least one matching request, but found none.");
        }

        if (times != null && !times.Matches(count))
        {
            throw new InvalidOperationException($"Expected {times}, but found {count} matching request(s).");
        }
    }

    /// <summary>
    /// Verify that a specific header was sent.
    /// </summary>
    public void VerifyHeader(string headerName, string expectedValue)
    {
        var matchingRequest = _requests.FirstOrDefault(r =>
            r.Headers.TryGetValues(headerName, out var values) && values.Contains(expectedValue));

        if (matchingRequest == null)
        {
            throw new InvalidOperationException($"Expected header '{headerName}: {expectedValue}' was not found in any request.");
        }
    }

    /// <summary>
    /// Verify the number of requests made to a specific path.
    /// </summary>
    public void VerifyRequestCount(string path, Times times)
    {
        var count = _requests.Count(r => r.RequestUri?.AbsolutePath.Contains(path) == true);

        if (!times.Matches(count))
        {
            throw new InvalidOperationException($"Expected {times} requests to '{path}', but found {count}.");
        }
    }

    /// <summary>
    /// Clear all recorded requests and configurations.
    /// </summary>
    public void Reset()
    {
        _requests.Clear();
        _configurations.Clear();
        _currentConfigIndex = 0;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        _requests.Add(request);

        // Find matching configuration
        var config = _configurations.FirstOrDefault(c => c.Predicate(request));

        if (config == null)
        {
            // No configuration found - return 404
            return new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                RequestMessage = request,
                Content = new StringContent("No mock configuration found for this request")
            };
        }

        // Apply delay if configured
        if (config.Delay.HasValue)
        {
            await Task.Delay(config.Delay.Value, cancellationToken);
        }

        // Create response
        var response = new HttpResponseMessage(config.StatusCode)
        {
            RequestMessage = request,
            Content = config.Content
        };

        // Add headers
        foreach (var header in config.Headers)
        {
            response.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // If there's a next configuration for sequential responses, move to it
        if (config.NextConfig != null)
        {
            var index = _configurations.IndexOf(config);
            if (index >= 0 && index + 1 < _configurations.Count)
            {
                _configurations[index] = config.NextConfig;
            }
        }

        return response;
    }

    internal void AddConfiguration(ResponseConfiguration config)
    {
        _configurations.Add(config);
    }

    /// <summary>
    /// Response configuration for a specific request pattern.
    /// </summary>
    public class ResponseConfiguration
    {
        public Func<HttpRequestMessage, bool> Predicate { get; set; } = _ => true;
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;
        public HttpContent? Content { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new();
        public TimeSpan? Delay { get; set; }
        public ResponseConfiguration? NextConfig { get; set; }
    }

    /// <summary>
    /// Fluent builder for configuring responses.
    /// </summary>
    public class ResponseConfigurationBuilder
    {
        private readonly FakeHttpResponseHandler _handler;
        private readonly ResponseConfiguration _config;

        internal ResponseConfigurationBuilder(FakeHttpResponseHandler handler, Func<HttpRequestMessage, bool> predicate)
        {
            _handler = handler;
            _config = new ResponseConfiguration { Predicate = predicate };
        }

        /// <summary>
        /// Configure the response to return.
        /// </summary>
        public ResponseConfigurationBuilder RespondWith(HttpStatusCode statusCode, object? data = null)
        {
            _config.StatusCode = statusCode;

            if (data != null)
            {
                var json = JsonSerializer.Serialize(data);
                _config.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            _handler.AddConfiguration(_config);
            return this;
        }

        /// <summary>
        /// Configure the response with custom content.
        /// </summary>
        public ResponseConfigurationBuilder RespondWith(HttpStatusCode statusCode, HttpContent content)
        {
            _config.StatusCode = statusCode;
            _config.Content = content;
            _handler.AddConfiguration(_config);
            return this;
        }

        /// <summary>
        /// Add a delay before returning the response.
        /// </summary>
        public ResponseConfigurationBuilder WithDelay(TimeSpan delay)
        {
            _config.Delay = delay;
            return this;
        }

        /// <summary>
        /// Add a header to the response.
        /// </summary>
        public ResponseConfigurationBuilder WithHeader(string name, string value)
        {
            _config.Headers[name] = value;
            return this;
        }

        /// <summary>
        /// Configure the next response for sequential calls.
        /// </summary>
        public ResponseConfigurationBuilder ThenOnNextRequest()
        {
            var nextConfig = new ResponseConfiguration { Predicate = _config.Predicate };
            _config.NextConfig = nextConfig;
            return new ResponseConfigurationBuilder(_handler, _config.Predicate);
        }
    }
}

/// <summary>
/// Verification helper for specifying expected call counts.
/// </summary>
public class Times
{
    private readonly int? _exactCount;
    private readonly int? _minCount;
    private readonly int? _maxCount;

    private Times(int? exactCount, int? minCount, int? maxCount)
    {
        _exactCount = exactCount;
        _minCount = minCount;
        _maxCount = maxCount;
    }

    public static Times Once() => new(1, null, null);
    public static Times Exactly(int count) => new(count, null, null);
    public static Times AtLeastOnce() => new(null, 1, null);
    public static Times AtLeast(int count) => new(null, count, null);
    public static Times AtMost(int count) => new(null, null, count);
    public static Times Between(int min, int max) => new(null, min, max);
    public static Times Never() => new(0, null, null);

    public bool Matches(int actualCount)
    {
        if (_exactCount.HasValue)
            return actualCount == _exactCount.Value;

        if (_minCount.HasValue && actualCount < _minCount.Value)
            return false;

        if (_maxCount.HasValue && actualCount > _maxCount.Value)
            return false;

        return true;
    }

    public override string ToString()
    {
        if (_exactCount.HasValue)
            return $"exactly {_exactCount} time(s)";

        if (_minCount.HasValue && _maxCount.HasValue)
            return $"between {_minCount} and {_maxCount} time(s)";

        if (_minCount.HasValue)
            return $"at least {_minCount} time(s)";

        if (_maxCount.HasValue)
            return $"at most {_maxCount} time(s)";

        return "any number of times";
    }
}
