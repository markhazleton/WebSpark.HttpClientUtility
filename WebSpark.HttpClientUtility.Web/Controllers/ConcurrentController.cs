using Microsoft.AspNetCore.Mvc;
using WebSpark.HttpClientUtility.Web.Services;

namespace WebSpark.HttpClientUtility.Web.Controllers;

/// <summary>
/// Demonstrates concurrent HTTP request processing
/// </summary>
public class ConcurrentController : Controller
{
    private readonly JokeApiService _jokeApiService;
    private readonly ILogger<ConcurrentController> _logger;

    public ConcurrentController(JokeApiService jokeApiService, ILogger<ConcurrentController> logger)
    {
   _jokeApiService = jokeApiService;
 _logger = logger;
    }

    public IActionResult Index()
    {
   ViewData["Title"] = "Concurrent Processing";
     ViewData["Description"] = "Execute multiple HTTP requests in parallel with controlled concurrency";
   return View();
    }

    [HttpGet]
    public async Task<IActionResult> ParallelRequests(int count = 5)
    {
        _logger.LogInformation("Fetching {Count} jokes concurrently", count);
        var result = await _jokeApiService.GetMultipleJokesConcurrentAsync(count);
        return View("Result", result);
    }

    [HttpGet]
    public IActionResult Demo()
    {
   return View();
    }
}
