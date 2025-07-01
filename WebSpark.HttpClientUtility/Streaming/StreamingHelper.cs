using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace WebSpark.HttpClientUtility.Streaming;

/// <summary>
/// Provides streaming capabilities for HTTP responses to handle large payloads efficiently.
/// </summary>
public static class StreamingHelper
{
    /// <summary>
    /// The default threshold in bytes above which responses should be streamed rather than loaded into memory.
    /// </summary>
    public const long DefaultStreamingThreshold = 10 * 1024 * 1024; // 10 MB

    /// <summary>
    /// Buffer size for stream reading operations.
    /// </summary>
    private const int BufferSize = 8192; // 8 KB buffer

    /// <summary>
    /// Processes an HTTP response using streaming for large payloads and efficient memory usage.
    /// </summary>
    /// <typeparam name="T">The target type for deserialization.</typeparam>
    /// <param name="response">The HTTP response message to process.</param>
    /// <param name="streamingThreshold">The size threshold in bytes above which streaming should be used.</param>
    /// <param name="jsonOptions">JSON serializer options for deserialization.</param>
    /// <param name="logger">Logger for recording processing information.</param>
    /// <param name="correlationId">Correlation ID for tracking the operation.</param>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>The deserialized object of type T, or null if deserialization fails.</returns>
    public static async Task<T?> ProcessResponseAsync<T>(
        HttpResponseMessage response,
        long streamingThreshold,
        JsonSerializerOptions jsonOptions,
        ILogger logger,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        if (response?.Content == null)
        {
            logger.LogWarning("Response or content is null [CorrelationId: {CorrelationId}]", correlationId);
            return default;
        }

        try
        {
            var contentLength = response.Content.Headers.ContentLength;
            var shouldStream = contentLength.HasValue && contentLength.Value > streamingThreshold;

            logger.LogDebug(
                "Processing response - ContentLength: {ContentLength}, Threshold: {Threshold}, UseStreaming: {UseStreaming} [CorrelationId: {CorrelationId}]",
                contentLength ?? -1, streamingThreshold, shouldStream, correlationId);

            if (shouldStream)
            {
                return await ProcessResponseWithStreamingAsync<T>(response, jsonOptions, logger, correlationId, cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                return await ProcessResponseWithoutStreamingAsync<T>(response, jsonOptions, logger, correlationId, cancellationToken)
                    .ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error processing HTTP response [CorrelationId: {CorrelationId}]", correlationId);
            return default;
        }
    }

    /// <summary>
    /// Processes the response using streaming for large payloads.
    /// </summary>
    private static async Task<T?> ProcessResponseWithStreamingAsync<T>(
        HttpResponseMessage response,
        JsonSerializerOptions jsonOptions,
        ILogger logger,
        string correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Using streaming deserialization [CorrelationId: {CorrelationId}]", correlationId);

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var result = await JsonSerializer.DeserializeAsync<T>(stream, jsonOptions, cancellationToken)
                .ConfigureAwait(false);

            logger.LogDebug("Streaming deserialization completed successfully [CorrelationId: {CorrelationId}]", correlationId);
            return result;
        }
        catch (JsonException jsonEx)
        {
            logger.LogWarning(jsonEx,
                "JSON deserialization failed during streaming [CorrelationId: {CorrelationId}]", correlationId);
            return default;
        }
    }

    /// <summary>
    /// Processes the response without streaming for smaller payloads.
    /// </summary>
    private static async Task<T?> ProcessResponseWithoutStreamingAsync<T>(
        HttpResponseMessage response,
        JsonSerializerOptions jsonOptions,
        ILogger logger,
        string correlationId,
        CancellationToken cancellationToken)
    {
        logger.LogDebug("Using standard deserialization [CorrelationId: {CorrelationId}]", correlationId);

        var jsonContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            logger.LogWarning("Response content is empty or whitespace [CorrelationId: {CorrelationId}]", correlationId);
            return default;
        }

        try
        {
            var result = JsonSerializer.Deserialize<T>(jsonContent, jsonOptions);
            logger.LogDebug("Standard deserialization completed successfully [CorrelationId: {CorrelationId}]", correlationId);
            return result;
        }
        catch (JsonException jsonEx)
        {
            logger.LogWarning(jsonEx,
                "JSON deserialization failed [CorrelationId: {CorrelationId}]. Content preview: {ContentPreview}",
                correlationId, TruncateForLogging(jsonContent, 200));
            return default;
        }
    }

    /// <summary>
    /// Truncates content for logging purposes to avoid overwhelming log entries.
    /// </summary>
    /// <param name="content">The content to truncate.</param>
    /// <param name="maxLength">Maximum length to preserve.</param>
    /// <returns>Truncated content with indication if truncation occurred.</returns>
    public static string TruncateForLogging(string content, int maxLength = 500)
    {
        if (string.IsNullOrEmpty(content) || content.Length <= maxLength)
            return content;

        return content[..maxLength] + "... [truncated]";
    }
}
