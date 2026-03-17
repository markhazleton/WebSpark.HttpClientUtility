using System.Text.RegularExpressions;

namespace WebSpark.HttpClientUtility.BatchExecution;

/// <summary>
/// Applies single-pass placeholder substitution for batch templates.
/// </summary>
public sealed class TemplateSubstitutionService : ITemplateSubstitutionService
{
    private static readonly Regex SingleBraceTokenRegex = new("\\{([^{}]+)\\}", RegexOptions.Compiled);

    /// <inheritdoc />
    public string Render(string template, BatchUserContext? userContext)
    {
        if (string.IsNullOrEmpty(template))
        {
            return string.Empty;
        }

        var rendered = template;
        var userId = userContext?.UserId ?? string.Empty;
        rendered = rendered.Replace("{{encoded_user_name}}", Uri.EscapeDataString(userId), StringComparison.Ordinal);

        var properties = userContext?.Properties;
        if (properties is null || properties.Count == 0)
        {
            return rendered;
        }

        return SingleBraceTokenRegex.Replace(rendered, match =>
        {
            var key = match.Groups[1].Value;
            return properties.TryGetValue(key, out var value) ? value : match.Value;
        });
    }
}
