using Microsoft.AspNetCore.Mvc;
using WebSpark.HttpClientUtility.Web.Services;

namespace WebSpark.HttpClientUtility.Web.Controllers;

/// <summary>
/// Demonstrates HttpRequestResult pattern with rich context
/// </summary>
public class RequestResultController : Controller
{
    private readonly JokeApiService _jokeApiService;
    private readonly ILogger<RequestResultController> _logger;

    public RequestResultController(JokeApiService jokeApiService, ILogger<RequestResultController> logger)
    {
        _jokeApiService = jokeApiService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Request Result Pattern";
      ViewData["Description"] = "Rich HTTP responses with correlation IDs, timing, and comprehensive error tracking";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> BasicRequest(string category = "Programming")
    {
        _logger.LogInformation("Request Result demo for category: {Category}", category);
        var result = await _jokeApiService.GetJokeWithContextAsync(category);
  return View("Result", result);
    }

    [HttpGet]
    public async Task<IActionResult> WithHeaders()
    {
        var result = await _jokeApiService.GetJokeWithHeadersAsync();
   return View("Result", result);
    }

    [HttpGet]
    public IActionResult BasicDemo()
    {
        return View();
    }

    [HttpGet]
    public IActionResult HeadersDemo()
    {
        return View();
    }
}
