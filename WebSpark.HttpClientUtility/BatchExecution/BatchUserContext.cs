namespace WebSpark.HttpClientUtility.BatchExecution;

/// <summary>
/// User context for placeholder substitution.
/// </summary>
public sealed class BatchUserContext
{
    /// <summary>
    /// Logical user identifier.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Template property bag used for substitution.
    /// </summary>
    public IReadOnlyDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
}
