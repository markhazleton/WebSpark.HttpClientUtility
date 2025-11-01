using System.Net;

namespace WebSpark.HttpClientUtility.Web.Models;

/// <summary>
/// View model for displaying demo results
/// </summary>
public class DemoResultViewModel
{
    public bool Success { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public object? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public long DurationMs { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public string? CorrelationId { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
    public string? CodeSample { get; set; }
    public string? CurlCommand { get; set; }
    public bool IsCached { get; set; }
  public string RequestUrl { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Feature showcase card for the home page
/// </summary>
public class FeatureCardViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string DemoUrl { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<string> KeyFeatures { get; set; } = new();
    public string BadgeText { get; set; } = string.Empty;
    public string BadgeColor { get; set; } = "primary";
}

/// <summary>
/// Comparison result for side-by-side demos
/// </summary>
public class ComparisonResultViewModel
{
public string MethodName { get; set; } = string.Empty;
public DemoResultViewModel WithLibrary { get; set; } = new();
    public DemoResultViewModel WithoutLibrary { get; set; } = new();
    public string Improvement { get; set; } = string.Empty;
    public int CodeLinesReduced { get; set; }
}
