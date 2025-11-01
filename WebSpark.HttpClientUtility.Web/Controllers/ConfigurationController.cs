using Microsoft.AspNetCore.Mvc;

namespace WebSpark.HttpClientUtility.Web.Controllers;

/// <summary>
/// Demonstrates configuration and logging options
/// </summary>
public class ConfigurationController : Controller
{
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(ILogger<ConfigurationController> logger)
    {
  _logger = logger;
    }

    public IActionResult Index()
    {
        ViewData["Title"] = "Configuration & Logging";
        ViewData["Description"] = "Comprehensive guide to configuring HttpClientUtility and logging options";
     return View();
    }
}
