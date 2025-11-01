using Microsoft.AspNetCore.Mvc;
using WebSpark.HttpClientUtility.Web.Models;

namespace WebSpark.HttpClientUtility.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        var features = new List<FeatureCardViewModel>
        {
            new()
            {
                Title = "Basic HTTP Operations",
                Description = "Simple, type-safe HTTP requests with automatic serialization",
                Icon = "🌐",
                DemoUrl = "/BasicHttp",
                Category = "Core Features",
                KeyFeatures = new() { "GET, POST, PUT, DELETE", "Type-safe responses", "Automatic JSON handling" },
                BadgeText = "Essential",
                BadgeColor = "primary"
            },
            new()
            {
                Title = "Request Result Pattern",
                Description = "Rich context with correlation IDs, timing metrics, and detailed errors",
                Icon = "📊",
                DemoUrl = "/RequestResult",
                Category = "Core Features",
                KeyFeatures = new() { "Correlation tracking", "Caller information", "Comprehensive context" },
                BadgeText = "Core",
                BadgeColor = "success"
            },
            new()
            {
                Title = "Configuration & Logging",
                Description = "Complete guide to configuring the library with advanced logging options",
                Icon = "⚙️",
                DemoUrl = "/Configuration",
                Category = "Core Features",
                KeyFeatures = new() { "appsettings.json setup", "Log levels", "cURL logging", "Environment configs" },
                BadgeText = "Essential",
                BadgeColor = "primary"
            },
            new()
            {
                Title = "Resilience & Polly",
                Description = "Built-in retry policies and circuit breakers for fault tolerance",
                Icon = "🛡️",
                DemoUrl = "/Resilience",
                Category = "Advanced",
                KeyFeatures = new() { "Automatic retries", "Circuit breaker", "Exponential backoff" },
                BadgeText = "Advanced",
                BadgeColor = "warning"
            },
            new()
            {
                Title = "Response Caching",
                Description = "Automatic response caching with configurable TTL",
                Icon = "⚡",
                DemoUrl = "/Caching",
                Category = "Performance",
                KeyFeatures = new() { "Memory caching", "Cache hit/miss tracking", "TTL configuration" },
                BadgeText = "Performance",
                BadgeColor = "info"
            },
            new()
            {
                Title = "Concurrent Processing",
                Description = "Execute multiple HTTP requests in parallel efficiently",
                Icon = "🚀",
                DemoUrl = "/Concurrent",
                Category = "Performance",
                KeyFeatures = new() { "Parallel execution", "Controlled concurrency", "Result aggregation" },
                BadgeText = "Performance",
                BadgeColor = "info"
            },
            new()
            {
                Title = "Authentication Providers",
                Description = "Multiple authentication strategies out of the box",
                Icon = "🔐",
                DemoUrl = "/Authentication",
                Category = "Security",
                KeyFeatures = new() { "API Key", "Bearer Token", "Basic Auth", "Custom providers" },
                BadgeText = "Security",
                BadgeColor = "danger"
            },
            new()
            {
                Title = "Telemetry & Observability",
                Description = "OpenTelemetry integration for comprehensive monitoring",
                Icon = "📈",
                DemoUrl = "/Telemetry",
                Category = "Observability",
                KeyFeatures = new() { "OpenTelemetry", "Request tracking", "Performance metrics" },
                BadgeText = "Observability",
                BadgeColor = "secondary"
            },
            new()
            {
                Title = "Utilities & Tools",
                Description = "Fire-and-forget tasks, cURL generation, and more",
                Icon = "🔧",
                DemoUrl = "/Utilities",
                Category = "Tools",
                KeyFeatures = new() { "FireAndForget", "cURL generator", "Query string builders" },
                BadgeText = "Tools",
                BadgeColor = "dark"
            }
        };

        return View(features);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
