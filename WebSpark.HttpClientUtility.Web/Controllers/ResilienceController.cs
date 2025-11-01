using Microsoft.AspNetCore.Mvc;
using WebSpark.HttpClientUtility.Web.Services;

namespace WebSpark.HttpClientUtility.Web.Controllers;

/// <summary>
/// Demonstrates resilience patterns with Polly
/// </summary>
public class ResilienceController : Controller
{
    private readonly JokeApiService _jokeApiService;
    private readonly ILogger<ResilienceController> _logger;

    public ResilienceController(JokeApiService jokeApiService, ILogger<ResilienceController> logger)
    {
        _jokeApiService = jokeApiService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Resilience & Polly Integration";
        ViewData["Description"] = "Automatic retry policies and circuit breakers for fault-tolerant applications";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> RetryDemo()
    {
        _logger.LogInformation("Demonstrating retry logic with intentional failure");
        var result = await _jokeApiService.GetJokeWithRetryAsync();
        return View("Result", result);
    }

    [HttpGet]
    public async Task<IActionResult> SuccessfulRetry(string category = "Programming")
    {
        _logger.LogInformation("Demonstrating successful request with retry capability for category: {Category}", category);
        var result = await _jokeApiService.GetJokeWithContextAsync(category);
        return View("Result", result);
    }

    [HttpGet]
    public IActionResult Demo()
    {
        return View();
    }
}
