using Microsoft.AspNetCore.Mvc;
using WebSpark.HttpClientUtility.Web.Services;

namespace WebSpark.HttpClientUtility.Web.Controllers;

/// <summary>
/// Demonstrates basic HTTP operations using IHttpClientService
/// </summary>
public class BasicHttpController : Controller
{
    private readonly JokeApiService _jokeApiService;
    private readonly ILogger<BasicHttpController> _logger;

    public BasicHttpController(JokeApiService jokeApiService, ILogger<BasicHttpController> logger)
    {
   _jokeApiService = jokeApiService;
        _logger = logger;
    }

    public IActionResult Index()
    {
      ViewData["Title"] = "Basic HTTP Operations";
        ViewData["Description"] = "Demonstrates IHttpClientService for simple, type-safe HTTP requests";
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Get(string category = "Programming")
    {
        _logger.LogInformation("Fetching joke for category: {Category}", category);
    var result = await _jokeApiService.GetJokeBasicAsync(category);
      return View("Result", result);
    }

    [HttpGet]
    public async Task<IActionResult> Post()
    {
  var result = await _jokeApiService.PostJokeSubmissionAsync("Why do programmers prefer dark mode? Because light attracts bugs!");
        return View("Result", result);
    }

    [HttpGet]
    public IActionResult GetDemo()
    {
        return View();
    }

    [HttpGet]
    public IActionResult PostDemo()
    {
        return View();
    }
}
