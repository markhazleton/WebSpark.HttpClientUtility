using Microsoft.AspNetCore.Mvc;

namespace WebSpark.HttpClientUtility.Web.Controllers;

/// <summary>
/// Demonstrates utility features
/// </summary>
public class UtilitiesController : Controller
{
    public IActionResult Index()
  {
  ViewData["Title"] = "Utilities & Tools";
        ViewData["Description"] = "FireAndForget tasks, cURL command generation, and more";
        return View();
    }
}
