namespace WebSpark.HttpClientUtility.RequestResult;

/// <summary>
/// Represents resilience options for retry and circuit breaker behavior.
/// </summary>
/// <remarks>
/// Configuration can be provided through the <c>HttpRequestResultPollyOptions</c> section.
/// For backward compatibility with legacy configuration, the following alias keys are also supported:
/// <list type="bullet">
/// <item><description><c>RetryDelaySeconds</c> as an alternative to <c>RetryDelay</c></description></item>
/// <item><description><c>CircuitBreakerDurationSeconds</c> as an alternative to <c>CircuitBreakerDuration</c></description></item>
/// </list>
/// </remarks>
public class HttpRequestResultPollyOptions
{
    /// <summary>
    /// Gets or sets the maximum number of retry attempts.
    /// </summary>
    public int MaxRetryAttempts { get; set; }

    /// <summary>
    /// Gets or sets the delay between retry attempts.
    /// </summary>
    public TimeSpan RetryDelay { get; set; }

    /// <summary>
    /// Gets or sets the threshold for circuit breaker.
    /// </summary>
    public int CircuitBreakerThreshold { get; set; }

    /// <summary>
    /// Gets or sets the duration of circuit breaker.
    /// </summary>
    public TimeSpan CircuitBreakerDuration { get; set; }
}
