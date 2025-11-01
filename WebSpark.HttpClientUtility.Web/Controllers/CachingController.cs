using Microsoft.AspNetCore.Mvc;
using WebSpark.HttpClientUtility.Web.Services;

namespace WebSpark.HttpClientUtility.Web.Controllers;

/// <summary>
/// Demonstrates caching capabilities
/// </summary>
public class CachingController : Controller
{
    private readonly JokeApiService _jokeApiService;
    private readonly ILogger<CachingController> _logger;

    public CachingController(JokeApiService jokeApiService, ILogger<CachingController> logger)
    {
   _jokeApiService = jokeApiService;
    _logger = logger;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Response Caching";
        ViewData["Description"] = "Automatic HTTP response caching with configurable TTL";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> CachedRequest(string category = "Programming")
{
  _logger.LogInformation("Cached request demo for category: {Category}", category);
        var result = await _jokeApiService.GetJokeCachedAsync(category);
return View("Result", result);
    }

    [HttpGet]
    public IActionResult Demo()
    {
        return View();
 }
}
