using Microsoft.AspNetCore.Mvc;

namespace WebSpark.HttpClientUtility.Web.Controllers;

/// <summary>
/// Demonstrates authentication providers
/// </summary>
public class AuthenticationController : Controller
{
    public IActionResult Index()
    {
     ViewData["Title"] = "Authentication Providers";
     ViewData["Description"] = "Multiple authentication strategies: API Key, Bearer Token, Basic Auth";
  return View();
    }
}
