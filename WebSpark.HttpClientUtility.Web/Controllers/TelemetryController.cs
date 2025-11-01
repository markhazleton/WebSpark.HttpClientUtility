using Microsoft.AspNetCore.Mvc;
using WebSpark.HttpClientUtility.Web.Services;

namespace WebSpark.HttpClientUtility.Web.Controllers;

/// <summary>
/// Demonstrates telemetry and observability features
/// </summary>
public class TelemetryController : Controller
{
    private readonly JokeApiService _jokeApiService;
    private readonly ILogger<TelemetryController> _logger;

    public TelemetryController(JokeApiService jokeApiService, ILogger<TelemetryController> logger)
    {
        _jokeApiService = jokeApiService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Telemetry & Observability";
        ViewData["Description"] = "OpenTelemetry integration for comprehensive monitoring and tracing";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> WithTelemetry(string category = "Programming")
    {
        _logger.LogInformation("Executing request with telemetry tracking for category: {Category}", category);
        var result = await _jokeApiService.GetJokeWithContextAsync(category);
        return View("Result", result);
    }

    [HttpGet]
    public async Task<IActionResult> CorrelationTracking()
    {
        _logger.LogInformation("Demonstrating correlation ID tracking across requests");
        var result = await _jokeApiService.GetJokeWithContextAsync("Any");
        return View("Result", result);
    }
}
