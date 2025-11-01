using System.Diagnostics;
using WebSpark.HttpClientUtility.ClientService;
using WebSpark.HttpClientUtility.RequestResult;
using WebSpark.HttpClientUtility.Web.Models;

namespace WebSpark.HttpClientUtility.Web.Services;

/// <summary>
/// Service demonstrating all WebSpark.HttpClientUtility features using the Joke API
/// </summary>
public class JokeApiService
{
  private readonly IHttpClientService _httpClientService;
    private readonly IHttpRequestResultService _requestResultService;
    private readonly ILogger<JokeApiService> _logger;
    private const string BaseUrl = "https://v2.jokeapi.dev";

    public JokeApiService(
        IHttpClientService httpClientService,
        IHttpRequestResultService requestResultService,
        ILogger<JokeApiService> logger)
 {
        _httpClientService = httpClientService;
        _requestResultService = requestResultService;
        _logger = logger;
    }

    #region Basic IHttpClientService Demos

    /// <summary>
/// Demo: Basic GET request using IHttpClientService
    /// </summary>
    public async Task<DemoResultViewModel> GetJokeBasicAsync(string category = "Any")
    {
        var stopwatch = Stopwatch.StartNew();
try
        {
            var requestUri = new Uri($"{BaseUrl}/joke/{category}?safe-mode");
   var result = await _httpClientService.GetAsync<JokeResponse>(requestUri, CancellationToken.None);

   stopwatch.Stop();

     return new DemoResultViewModel
     {
   Success = result.IsSuccess,
       Title = "Basic GET Request",
                Description = "Simple GET request using IHttpClientService.GetAsync<T>",
      Data = result.Content,
        DurationMs = stopwatch.ElapsedMilliseconds,
      StatusCode = result.StatusCode,
      RequestUrl = requestUri.ToString(),
                CodeSample = GetCodeSample("BasicGet")
            };
     }
  catch (Exception ex)
        {
      stopwatch.Stop();
        _logger.LogError(ex, "Error in GetJokeBasicAsync");
            return new DemoResultViewModel
            {
                Success = false,
  Title = "Basic GET Request",
      ErrorMessage = ex.Message,
                DurationMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    /// <summary>
    /// Demo: POST request using IHttpClientService
    /// </summary>
    public async Task<DemoResultViewModel> PostJokeSubmissionAsync(string jokeText)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
        // Note: Joke API doesn't have a POST endpoint, this is for demonstration
            var requestUri = new Uri($"{BaseUrl}/submit");
        var payload = new { joke = jokeText, category = "Programming" };

            // This will fail (404) but demonstrates the POST pattern
     var result = await _httpClientService.PostAsync<object, JokeResponse>(
            requestUri,
      payload,
     CancellationToken.None);

        stopwatch.Stop();

        return new DemoResultViewModel
 {
    Success = result.IsSuccess,
         Title = "POST Request Demo",
        Description = "Demonstrates POST with payload using IHttpClientService.PostAsync<T, TResult>",
       Data = result.Content,
             DurationMs = stopwatch.ElapsedMilliseconds,
   StatusCode = result.StatusCode,
        RequestUrl = requestUri.ToString(),
    CodeSample = GetCodeSample("BasicPost")
            };
        }
catch (Exception ex)
        {
  stopwatch.Stop();
            return new DemoResultViewModel
            {
                Success = false,
      Title = "POST Request Demo",
        ErrorMessage = ex.Message,
     DurationMs = stopwatch.ElapsedMilliseconds,
     CodeSample = GetCodeSample("BasicPost")
       };
  }
    }

    #endregion

    #region HttpRequestResult Demos

    /// <summary>
    /// Demo: Using HttpRequestResult pattern with full context
    /// </summary>
    public async Task<DemoResultViewModel> GetJokeWithContextAsync(string category = "Programming")
    {
        var request = new HttpRequestResult<JokeResponse>
        {
            RequestPath = $"{BaseUrl}/joke/{category}?safe-mode",
 RequestMethod = HttpMethod.Get,
   IsDebugEnabled = true
        };

        var result = await _requestResultService.HttpSendRequestResultAsync(request);

    return new DemoResultViewModel
        {
            Success = result.IsSuccessStatusCode,
            Title = "Request with Full Context",
            Description = "HttpRequestResult provides correlation ID, timing, and detailed error information",
       Data = result.ResponseResults,
        DurationMs = result.RequestDurationMilliseconds,
            StatusCode = result.StatusCode,
            CorrelationId = result.CorrelationId,
       ErrorMessage = result.ErrorList.Any() ? string.Join(", ", result.ErrorList) : null,
            RequestUrl = request.RequestPath,
Metrics = new Dictionary<string, object>
         {
              ["CorrelationId"] = result.CorrelationId,
            ["RequestStartTime"] = result.RequestStartTimestamp,
          ["Duration"] = result.RequestDurationMilliseconds
},
            CodeSample = GetCodeSample("RequestResult")
        };
    }

    /// <summary>
    /// Demo: Request with custom headers
    /// </summary>
    public async Task<DemoResultViewModel> GetJokeWithHeadersAsync()
    {
        var request = new HttpRequestResult<JokeResponse>
        {
            RequestPath = $"{BaseUrl}/joke/Any?safe-mode",
            RequestMethod = HttpMethod.Get,
  RequestHeaders = new Dictionary<string, string>
   {
          ["X-Demo-Header"] = "WebSpark-Demo",
                ["X-Request-Time"] = DateTime.UtcNow.ToString("O")
     }
        };

        var result = await _requestResultService.HttpSendRequestResultAsync(request);

     return new DemoResultViewModel
 {
            Success = result.IsSuccessStatusCode,
            Title = "Request with Custom Headers",
            Description = "Demonstrates adding custom headers to requests",
 Data = result.ResponseResults,
            DurationMs = result.RequestDurationMilliseconds,
          StatusCode = result.StatusCode,
         CorrelationId = result.CorrelationId,
       RequestUrl = request.RequestPath,
            CodeSample = GetCodeSample("CustomHeaders")
        };
    }

    #endregion

    #region Caching Demos

    /// <summary>
    /// Demo: Cached request (if caching decorator is enabled)
    /// </summary>
    public async Task<DemoResultViewModel> GetJokeCachedAsync(string category = "Programming")
    {
        var request = new HttpRequestResult<JokeResponse>
    {
            RequestPath = $"{BaseUrl}/joke/{category}?safe-mode&id=1",
      RequestMethod = HttpMethod.Get,
          CacheDurationMinutes = 5 // Cache for 5 minutes
        };

    var result = await _requestResultService.HttpSendRequestResultAsync(request);

  var isCacheHit = result.RequestContext.TryGetValue("CacheHit", out var cacheHit) && 
               cacheHit is bool hitBool && hitBool;

 return new DemoResultViewModel
        {
    Success = result.IsSuccessStatusCode,
            Title = "Cached Request",
            Description = $"Response {(isCacheHit ? "retrieved from cache" : "fetched from API and cached")}",
       Data = result.ResponseResults,
  DurationMs = result.RequestDurationMilliseconds,
   StatusCode = result.StatusCode,
CorrelationId = result.CorrelationId,
     IsCached = isCacheHit,
         RequestUrl = request.RequestPath,
            Metrics = new Dictionary<string, object>
     {
 ["CacheHit"] = isCacheHit,
    ["CacheDuration"] = "5 minutes",
     ["CacheAge"] = result.RequestContext.TryGetValue("CacheAge", out var age) ? age : "N/A"
    },
      CodeSample = GetCodeSample("Caching")
        };
    }

    #endregion

    #region Resilience Demos

    /// <summary>
    /// Demo: Request with retry logic (simulated failure)
    /// </summary>
    public async Task<DemoResultViewModel> GetJokeWithRetryAsync()
    {
      // Using an invalid endpoint to trigger retries
        var request = new HttpRequestResult<JokeResponse>
        {
            RequestPath = $"{BaseUrl}/joke/InvalidCategory",
      RequestMethod = HttpMethod.Get,
        IsDebugEnabled = true
        };

        var result = await _requestResultService.HttpSendRequestResultAsync(request);

        var retryCount = result.RequestContext.TryGetValue("RetryCount", out var retry) ? retry : 0;

        return new DemoResultViewModel
    {
            Success = result.IsSuccessStatusCode,
 Title = "Request with Retry Logic",
   Description = "Demonstrates automatic retry with Polly (if enabled)",
            Data = result.ResponseResults,
            ErrorMessage = result.ErrorList.Any() ? string.Join(", ", result.ErrorList) : null,
     DurationMs = result.RequestDurationMilliseconds,
          StatusCode = result.StatusCode,
   CorrelationId = result.CorrelationId,
 RequestUrl = request.RequestPath,
         Metrics = new Dictionary<string, object>
   {
      ["RetryAttempts"] = retryCount,
       ["PollyEnabled"] = "Check Program.cs configuration"
 },
            CodeSample = GetCodeSample("Retry")
  };
    }

    #endregion

    #region Concurrent Processing Demos

    /// <summary>
    /// Demo: Fetch multiple jokes concurrently
    /// </summary>
    public async Task<DemoResultViewModel> GetMultipleJokesConcurrentAsync(int count = 5)
    {
        var stopwatch = Stopwatch.StartNew();
   var categories = new[] { "Programming", "Misc", "Dark", "Pun", "Spooky" };
        var tasks = new List<Task<HttpRequestResult<JokeResponse>>>();

 for (int i = 0; i < Math.Min(count, categories.Length); i++)
{
         var request = new HttpRequestResult<JokeResponse>
        {
          RequestPath = $"{BaseUrl}/joke/{categories[i]}?safe-mode",
          RequestMethod = HttpMethod.Get
            };
            tasks.Add(_requestResultService.HttpSendRequestResultAsync(request));
        }

var results = await Task.WhenAll(tasks);
        stopwatch.Stop();

        var jokes = results
      .Where(r => r.IsSuccessStatusCode && r.ResponseResults != null)
    .Select(r => r.ResponseResults!)
 .ToList();

 return new DemoResultViewModel
        {
     Success = jokes.Any(),
            Title = "Concurrent Requests",
            Description = $"Fetched {jokes.Count} jokes concurrently using Task.WhenAll",
            Data = jokes,
            DurationMs = stopwatch.ElapsedMilliseconds,
            StatusCode = System.Net.HttpStatusCode.OK,
            RequestUrl = $"{BaseUrl}/joke/[Multiple]",
            Metrics = new Dictionary<string, object>
      {
        ["TotalRequests"] = count,
["SuccessfulRequests"] = jokes.Count,
           ["FailedRequests"] = count - jokes.Count,
   ["AverageDuration"] = results.Average(r => r.RequestDurationMilliseconds)
      },
      CodeSample = GetCodeSample("Concurrent")
        };
}

 #endregion

    #region Helper Methods

private string GetCodeSample(string type)
    {
        return type switch
        {
            "BasicGet" => @"
// Basic GET request
var requestUri = new Uri(""https://v2.jokeapi.dev/joke/Programming?safe-mode"");
var result = await _httpClientService.GetAsync<JokeResponse>(requestUri, CancellationToken.None);

if (result.IsSuccess)
{
    var joke = result.Content;
    Console.WriteLine(joke.FullJoke);
}",

            "BasicPost" => @"
// Basic POST request
var requestUri = new Uri(""https://api.example.com/submit"");
var payload = new { joke = ""Why do programmers prefer dark mode?"", category = ""Programming"" };
var result = await _httpClientService.PostAsync<object, Response>(requestUri, payload, CancellationToken.None);",

  "RequestResult" => @"
// Request with full context and correlation tracking
var request = new HttpRequestResult<JokeResponse>
{
    RequestPath = ""https://v2.jokeapi.dev/joke/Programming?safe-mode"",
    RequestMethod = HttpMethod.Get,
    IsDebugEnabled = true
};

var result = await _requestResultService.HttpSendRequestResultAsync(request);

// Access rich context
Console.WriteLine($""CorrelationId: {result.CorrelationId}"");
Console.WriteLine($""Duration: {result.RequestDurationMilliseconds}ms"");
Console.WriteLine($""Called from: {result.CallerMemberName}"");",

            "CustomHeaders" => @"
// Request with custom headers
var request = new HttpRequestResult<JokeResponse>
{
    RequestPath = ""https://v2.jokeapi.dev/joke/Any?safe-mode"",
    RequestMethod = HttpMethod.Get,
    RequestHeaders = new Dictionary<string, string>
    {
        [""X-API-Key""] = ""your-api-key"",
        [""X-Request-Time""] = DateTime.UtcNow.ToString(""O"")
    }
};

var result = await _requestResultService.HttpSendRequestResultAsync(request);",

    "Caching" => @"
// Cached request (requires HttpRequestResultServiceCache decorator)
var request = new HttpRequestResult<JokeResponse>
{
    RequestPath = ""https://v2.jokeapi.dev/joke/Programming?safe-mode"",
    RequestMethod = HttpMethod.Get,
    CacheDurationMinutes = 5 // Cache for 5 minutes
};

var result = await _requestResultService.HttpSendRequestResultAsync(request);

// Check if response came from cache
var isCacheHit = result.RequestContext.TryGetValue(""CacheHit"", out var hit) && (bool)hit;
Console.WriteLine($""From cache: {isCacheHit}"");",

          "Retry" => @"
// Request with automatic retry (requires HttpRequestResultServicePolly decorator)
// Configure in Program.cs:
// var pollyOptions = new HttpRequestResultPollyOptions
// {
//     MaxRetryAttempts = 3,
//     RetryDelay = TimeSpan.FromSeconds(1)
// };

var request = new HttpRequestResult<JokeResponse>
{
    RequestPath = ""https://v2.jokeapi.dev/joke/Any?safe-mode"",
    RequestMethod = HttpMethod.Get
};

var result = await _requestResultService.HttpSendRequestResultAsync(request);
// Automatically retries on transient failures!",

     "Concurrent" => @"
// Concurrent requests
var categories = new[] { ""Programming"", ""Misc"", ""Dark"", ""Pun"" };
var tasks = categories.Select(category =>
{
    var request = new HttpRequestResult<JokeResponse>
    {
        RequestPath = $""https://v2.jokeapi.dev/joke/{category}?safe-mode"",
      RequestMethod = HttpMethod.Get
    };
    return _requestResultService.HttpSendRequestResultAsync(request);
});

var results = await Task.WhenAll(tasks);
var jokes = results.Where(r => r.IsSuccessStatusCode).Select(r => r.ResponseResults).ToList();",

            _ => "// Code sample not available"
        };
    }

    #endregion
}
