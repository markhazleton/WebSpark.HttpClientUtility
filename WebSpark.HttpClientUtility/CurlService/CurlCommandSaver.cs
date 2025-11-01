using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using CsvHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace WebSpark.HttpClientUtility.CurlService;

/// <summary>
/// Options for configuring the CurlCommandSaver behavior.
/// </summary>
public class CurlCommandSaverOptions
{
    /// <summary>
    /// Gets or sets the folder path where CSV files will be stored.
    /// </summary>
    public string OutputFolder { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the base name of the CSV file (without extension).
    /// </summary>
    public string CsvFileName { get; set; } = "curl_commands";

    /// <summary>
    /// Gets or sets the maximum size of each log file in bytes before rotation (default: 10MB).
    /// </summary>
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024;

    /// <summary>
    /// Gets or sets the maximum number of retries for file operations.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Gets or sets the delay between retries in milliseconds.
    /// </summary>
    public int RetryDelayMs { get; set; } = 200;

    /// <summary>
    /// Gets or sets a value indicating whether to sanitize sensitive information from requests.
    /// </summary>
    public bool SanitizeSensitiveInfo { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to use batch processing for better performance.
    /// </summary>
    public bool UseBatchProcessing { get; set; } = true;

    /// <summary>
    /// Gets or sets the batch size when batch processing is enabled.
    /// </summary>
    public int BatchSize { get; set; } = 100;

    /// <summary>
    /// Gets or sets the flush interval in milliseconds when batch processing is enabled.
    /// </summary>
    public int BatchFlushIntervalMs { get; set; } = 5000;

    /// <summary>
    /// Gets or sets a list of header names that should be redacted from the curl command.
    /// </summary>
    public List<string> SensitiveHeaders { get; set; } = new List<string>
    {
        "Authorization",
        "Api-Key",
        "X-Api-Key",
        "Password",
        "Token"
    };
}

/// <summary>
/// Provides functionality to generate and save cURL commands from HTTP requests for debugging and logging purposes.
/// </summary>
public class CurlCommandSaver : IDisposable
{
    // SemaphoreSlim is used to enforce exclusive access during file writes.
    private static readonly SemaphoreSlim _fileLock = new SemaphoreSlim(1, 1);
    private readonly string _csvFilePath;
    private readonly ILogger _logger;
    private readonly CurlCommandSaverOptions _options;
    private readonly ConcurrentQueue<CurlCommandRecord> _pendingRecords = new ConcurrentQueue<CurlCommandRecord>();
    private readonly Timer? _batchProcessingTimer;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurlCommandSaver"/> class.
    /// </summary>
    /// <param name="logger">The logger instance to use for logging operations</param>
    /// <param name="configuration">The configuration containing settings for the command saver</param>
    /// <exception cref="ArgumentException">Thrown when the CsvOutputFolder configuration setting is not properly configured</exception>
    public CurlCommandSaver(ILogger logger, IConfiguration configuration)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (configuration == null)
        {
            throw new ArgumentNullException(nameof(configuration));
        }

        // Initialize options from configuration
        _options = new CurlCommandSaverOptions
        {
            OutputFolder = configuration["CsvOutputFolder"] ?? string.Empty,
            CsvFileName = configuration["CsvFileName"] ?? "curl_commands"
        };

        if (string.IsNullOrWhiteSpace(_options.OutputFolder))
        {
            throw new ArgumentException("CsvOutputFolder is not configured properly.");
        }

        // Manually check UseBatchProcessing configuration
        var useBatchProcessingSection = configuration.GetSection("CurlCommandSaver:UseBatchProcessing");
        if (useBatchProcessingSection.Value != null &&
            bool.TryParse(useBatchProcessingSection.Value, out bool useBatchProcessing))
        {
            _options.UseBatchProcessing = useBatchProcessing;
        }

        // Ensure the output directory exists.
        Directory.CreateDirectory(_options.OutputFolder);

        // Build the full path for the CSV file.
        _csvFilePath = Path.Combine(_options.OutputFolder, $"{_options.CsvFileName}.csv");

        // Log configuration settings
        _logger.LogInformation(
            "CurlCommandSaver initialized with OutputFolder: {OutputFolder}, BatchProcessing: {UseBatchProcessing}",
            _options.OutputFolder,
            _options.UseBatchProcessing);

        // Initialize batch processing timer if enabled
        if (_options.UseBatchProcessing)
        {
            _batchProcessingTimer = new Timer(
                ProcessBatch,
                null,
                _options.BatchFlushIntervalMs,
                _options.BatchFlushIntervalMs
            );
        }
    }

    /// <summary>
    /// Saves an HTTP request as a cURL command to a CSV file.
    /// </summary>
    /// <param name="request">The HTTP request message to convert and save</param>
    /// <param name="memberName">Name of the calling member (automatically populated)</param>
    /// <param name="filePath">File path of the calling code (automatically populated)</param>
    /// <param name="lineNumber">Line number of the calling code (automatically populated)</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task SaveCurlCommandAsync(HttpRequestMessage? request,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0)
    {
        try
        {
            // Build the curl command string.
            var curlCommand = new StringBuilder();
            curlCommand.Append("curl");

            // Handle null requests gracefully
            if (request == null)
            {
                _logger.LogInformation("Created curl command: {Command} (null request)", curlCommand.ToString());

                // Create a record with the current timestamp and a basic curl command.
                var record = new CurlCommandRecord
                {
                    CurlCommand = curlCommand.ToString(),
                    RequestPath = string.Empty,
                    RequestMethod = string.Empty,
                    CallingMethod = memberName,
                    CallingFile = filePath,
                    CallingLineNumber = lineNumber
                };

                if (_options.UseBatchProcessing)
                {
                    _pendingRecords.Enqueue(record);

                    // Process immediately if we've reached batch size
                    if (_pendingRecords.Count >= _options.BatchSize)
                    {
                        await ProcessBatchAsync();
                    }
                }
                else
                {
                    await SaveRecordToCsvWithRetryAsync(record);
                }
                return;
            }

            // Include the HTTP method if it's not GET (curl defaults to GET).
            if (request.Method != HttpMethod.Get)
            {
                curlCommand.Append(" -X ").Append(request.Method.Method);
            }

            // Add headers (with sanitization if configured)
            if (request.Headers != null && request.Headers.Any())
            {
                foreach (var header in request.Headers)
                {
                    string headerValue = header.Value.First();

                    // Apply sanitization if enabled and header is in sensitive list
                    if (_options.SanitizeSensitiveInfo &&
                        _options.SensitiveHeaders.Contains(header.Key, StringComparer.OrdinalIgnoreCase))
                    {
                        headerValue = "REDACTED";
                    }

                    curlCommand.Append(" -H \"")
                        .Append(header.Key)
                        .Append(": ")
                        .Append(headerValue.Replace("\"", "\\\""))
                        .Append("\"");
                }
            }

            // Include request content as a data parameter, if available.
            if (request.Content != null)
            {
                string content = await request.Content.ReadAsStringAsync().ConfigureAwait(true);

                // Sanitize content if enabled
                if (_options.SanitizeSensitiveInfo)
                {
                    content = SanitizeJson(content);
                }

                curlCommand.Append(" -d '").Append(content.Replace("'", "\\'")).Append('\'');

                // Add content type header if present
                if (request.Content.Headers.ContentType != null)
                {
                    curlCommand.Append(" -H \"Content-Type: ")
                        .Append(request.Content.Headers.ContentType)
                        .Append("\"");
                }
            }

            // Append the request URL.
            curlCommand.Append(" \"").Append(request.RequestUri).Append("\"");

            _logger.LogInformation("Created curl command: {Command}", curlCommand.ToString());

            // Create a record with the current timestamp and the generated curl command.
            var commandRecord = new CurlCommandRecord
            {
                CurlCommand = curlCommand.ToString(),
                RequestPath = request.RequestUri?.ToString() ?? string.Empty,
                RequestMethod = request.Method.Method,
                CallingMethod = memberName,
                CallingFile = filePath,
                CallingLineNumber = lineNumber
            };

            if (_options.UseBatchProcessing)
            {
                _pendingRecords.Enqueue(commandRecord);

                // Process immediately if we've reached batch size
                if (_pendingRecords.Count >= _options.BatchSize)
                {
                    await ProcessBatchAsync();
                }
            }
            else
            {
                await SaveRecordToCsvWithRetryAsync(commandRecord);
            }
        }
        catch (Exception ex)
        {
            // Log but don't throw to avoid impacting the main application flow
            _logger.LogError(ex, "Error generating or saving curl command");
        }
    }

    private string SanitizeJson(string json)
    {
        if (string.IsNullOrEmpty(json) || !json.Contains("\""))
        {
            return json;
        }

        try
        {
            // Very basic JSON sanitization - replace values of sensitive fields
            foreach (var sensitiveField in _options.SensitiveHeaders)
            {
                // This is a simple approach - for production code, use a proper JSON parser
                var pattern = $"\"{sensitiveField}\"\\s*:\\s*\"[^\"]*\"";
                json = System.Text.RegularExpressions.Regex.Replace(
                    json,
                    pattern,
                    $"\"{sensitiveField}\":\"REDACTED\"",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase
                );
            }

            return json;
        }
        catch
        {
            // If we fail to sanitize, return the original string
            return json;
        }
    }

    private void ProcessBatch(object? state)
    {
        // Fire and forget - errors are logged inside the method
        _ = ProcessBatchAsync();
    }

    private async Task ProcessBatchAsync()
    {
        if (_pendingRecords.IsEmpty)
        {
            return;
        }

        var recordsToProcess = new List<CurlCommandRecord>();

        // Dequeue all pending records
        while (_pendingRecords.TryDequeue(out var record))
        {
            recordsToProcess.Add(record);
        }

        if (recordsToProcess.Count == 0)
        {
            return;
        }

        try
        {
            await SaveRecordsBatchToCsvWithRetryAsync(recordsToProcess);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing batch of curl commands");
        }
    }

    private async Task SaveRecordToCsvWithRetryAsync(CurlCommandRecord record)
    {
        await SaveRecordsBatchToCsvWithRetryAsync(new List<CurlCommandRecord> { record });
    }

    private async Task SaveRecordsBatchToCsvWithRetryAsync(List<CurlCommandRecord> records)
    {
        if (records == null || records.Count == 0)
        {
            return;
        }

        int retryCount = 0;
        bool succeeded = false;

        while (!succeeded && retryCount < _options.MaxRetries)
        {
            try
            {
                // First, check if file rotation is needed
                await CheckAndRotateFileIfNeededAsync();

                // Write the records to the CSV file using a file lock for thread safety.
                await _fileLock.WaitAsync().ConfigureAwait(true);
                try
                {
                    bool fileExists = File.Exists(_csvFilePath);

                    using (var stream = File.Open(_csvFilePath, fileExists ? FileMode.Append : FileMode.Create, FileAccess.Write, FileShare.Read))
                    using (var writer = new StreamWriter(stream))
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        // If the file is new, write the header first.
                        if (!fileExists)
                        {
                            csv.WriteHeader<CurlCommandRecord>();
                            await csv.NextRecordAsync().ConfigureAwait(true);
                        }

                        foreach (var record in records)
                        {
                            csv.WriteRecord(record);
                            await csv.NextRecordAsync().ConfigureAwait(true);
                        }
                    }

                    _logger.LogInformation("Saved {Count} curl command records to CSV file at {Path}",
                        records.Count, _csvFilePath);

                    succeeded = true;
                }
                finally
                {
                    _fileLock.Release();
                }
            }
            catch (IOException ex)
            {
                retryCount++;

                if (retryCount >= _options.MaxRetries)
                {
                    _logger.LogError(ex, "Failed to save curl commands to CSV after {RetryCount} retries", retryCount);
                    throw;
                }

                _logger.LogWarning(ex, "Error saving curl commands to CSV (attempt {RetryCount}/{MaxRetries}). Retrying...",
                    retryCount, _options.MaxRetries);

                // Add exponential backoff
                await Task.Delay(_options.RetryDelayMs * (int)Math.Pow(2, retryCount - 1));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error saving curl commands to CSV");
                throw;
            }
        }
    }

    private async Task CheckAndRotateFileIfNeededAsync()
    {
        await _fileLock.WaitAsync().ConfigureAwait(true);
        try
        {
            if (!File.Exists(_csvFilePath))
            {
                return;
            }

            var fileInfo = new FileInfo(_csvFilePath);
            if (fileInfo.Length >= _options.MaxFileSize)
            {
                string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                string rotatedFilePath = Path.Combine(
                    _options.OutputFolder,
                    $"{_options.CsvFileName}_{timestamp}.csv"
                );

                File.Move(_csvFilePath, rotatedFilePath);
                _logger.LogInformation("Rotated CSV file to {RotatedFilePath}", rotatedFilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking/rotating curl command CSV file");
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// Forces the processing of any pending batch records.
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task FlushAsync()
    {
        if (_options.UseBatchProcessing && !_pendingRecords.IsEmpty)
        {
            await ProcessBatchAsync();
        }
    }

    /// <summary>
    /// Disposes resources used by the CurlCommandSaver.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes resources used by the CurlCommandSaver.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _batchProcessingTimer?.Dispose();

            // Process any pending records
            if (_options.UseBatchProcessing && !_pendingRecords.IsEmpty)
            {
                // Use Wait() here as this is Dispose
                ProcessBatchAsync().Wait();
            }

            _fileLock.Dispose();
        }

        _disposed = true;
    }

    /// <summary>
    /// Represents a record of a cURL command with metadata about where it was generated.
    /// </summary>
    private class CurlCommandRecord
    {
        /// <summary>
        /// Gets or sets the file path where the request was initiated.
        /// </summary>
        public string CallingFile { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the line number in the file where the request was initiated.
        /// </summary>
        public int CallingLineNumber { get; set; }

        /// <summary>
        /// Gets or sets the method name where the request was initiated.
        /// </summary>
        public string CallingMethod { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the generated cURL command string.
        /// </summary>
        public string CurlCommand { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the HTTP method used in the request.
        /// </summary>
        public string RequestMethod { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the URL or path of the request.
        /// </summary>
        public string RequestPath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the timestamp when the record was created.
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
