using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text.Json;

namespace WebSpark.HttpClientUtility.Web.TagHelpers;

/// <summary>
/// Tag helper for referencing Vite-built assets using the manifest file
/// </summary>
[HtmlTargetElement("vite-script")]
[HtmlTargetElement("vite-style")]
public class ViteAssetTagHelper : TagHelper
{
    private readonly IWebHostEnvironment _env;
    private static readonly Dictionary<string, ViteManifest> _manifestCache = new();
    private static readonly object _manifestLock = new();

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext? ViewContext { get; set; }

    /// <summary>
    /// The entry point file name (e.g., "main.js" or "site.css")
    /// </summary>
    [HtmlAttributeName("entry")]
    public string? Entry { get; set; }

    public ViteAssetTagHelper(IWebHostEnvironment env)
    {
        _env = env;
    }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (string.IsNullOrEmpty(Entry))
        {
            output.SuppressOutput();
            return;
        }

        var manifest = GetManifest();
        if (manifest == null || !manifest.TryGetValue($"src/{Entry}", out var asset))
        {
            // Fallback to development mode or suppress output
            output.SuppressOutput();
            return;
        }

        var tagName = context.TagName;
        if (tagName == "vite-script")
        {
            output.TagName = "script";
            output.Attributes.SetAttribute("src", $"/dist/{asset.File}");
            output.Attributes.SetAttribute("type", "module");
        }
        else if (tagName == "vite-style")
        {
            output.TagName = "link";
            output.Attributes.SetAttribute("rel", "stylesheet");
            output.Attributes.SetAttribute("href", $"/dist/{asset.File}");
        }
    }

    private Dictionary<string, ViteManifestEntry>? GetManifest()
    {
        var manifestPath = Path.Combine(_env.WebRootPath, "dist", ".vite", "manifest.json");
        
        if (!File.Exists(manifestPath))
        {
            return null;
        }

        var lastWriteTime = File.GetLastWriteTimeUtc(manifestPath);
        var cacheKey = $"{manifestPath}_{lastWriteTime.Ticks}";

        lock (_manifestLock)
        {
            if (_manifestCache.TryGetValue(cacheKey, out var cachedManifest))
            {
                return cachedManifest.Entries;
            }

            // Clear old cache entries
            _manifestCache.Clear();

            var json = File.ReadAllText(manifestPath);
            var entries = JsonSerializer.Deserialize<Dictionary<string, ViteManifestEntry>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (entries != null)
            {
                var manifest = new ViteManifest { Entries = entries };
                _manifestCache[cacheKey] = manifest;
                return entries;
            }
        }

        return null;
    }

    private class ViteManifest
    {
        public Dictionary<string, ViteManifestEntry> Entries { get; set; } = new();
    }

    private class ViteManifestEntry
    {
        public string File { get; set; } = string.Empty;
        public string[]? Css { get; set; }
        public bool IsEntry { get; set; }
        public string? Src { get; set; }
    }
}
