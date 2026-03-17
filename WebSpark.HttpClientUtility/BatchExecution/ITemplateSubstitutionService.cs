namespace WebSpark.HttpClientUtility.BatchExecution;

/// <summary>
/// Renders template placeholders from a batch user context.
/// </summary>
public interface ITemplateSubstitutionService
{
    /// <summary>
    /// Renders a template string using values from the supplied user context.
    /// </summary>
    /// <param name="template">Template text to render.</param>
    /// <param name="userContext">Optional user context for placeholder resolution.</param>
    /// <returns>Rendered text with unresolved placeholders preserved.</returns>
    string Render(string template, BatchUserContext? userContext);
}
