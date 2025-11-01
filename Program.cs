using Microsoft.Extensions.Caching.Memory;
using WebSpark.HttpClientUtility.ClientService;
using WebSpark.HttpClientUtility.RequestResult;
using WebSpark.HttpClientUtility.StringConverter;
using WebSpark.HttpClientUtility.FireAndForget;
using WebSpark.HttpClientUtility.Concurrent;
using WebSpark.HttpClientUtility.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ============================================================================
// WebSpark.HttpClientUtility Configuration - Demo Showcase
// ============================================================================

// 1. Core HTTP Client Factory (Essential for all HTTP operations)
builder.Services.AddHttpClient();

// 2. Memory Cache (Required for caching decorator)
builder.Services.AddMemoryCache();

// 3. String Converter (Choose JSON serializer)
// Using System.Text.Json (default, high performance)
builder.Services.AddSingleton<IStringConverter, SystemJsonStringConverter>();
// Alternative: Newtonsoft.Json
// builder.Services.AddSingleton<IStringConverter, NewtonsoftJsonStringConverter>();

// 4. Basic HTTP Client Service
builder.Services.AddScoped<IHttpClientService, HttpClientService>();

// 5. Request Result Service with Decorator Pattern
// This demonstrates the powerful decorator chain pattern
builder.Services.AddScoped<HttpRequestResultService>();

builder.Services.AddScoped<IHttpRequestResultService>(provider =>
{
    // Start with base service
    IHttpRequestResultService service = provider.GetRequiredService<HttpRequestResultService>();

    // Layer 1: Add Caching (reduces API calls, improves performance)
    service = new HttpRequestResultServiceCache(
        provider.GetRequiredService<ILogger<HttpRequestResultServiceCache>>(),
        service,
    provider.GetRequiredService<IMemoryCache>()
    );

    // Layer 2: Add Polly Resilience (retries and circuit breaker)
    var pollyOptions = new HttpRequestResultPollyOptions
    {
        MaxRetryAttempts = 3,
        RetryDelay = TimeSpan.FromSeconds(1),
   EnableCircuitBreaker = true,
  CircuitBreakerThreshold = 5,
        CircuitBreakerDuration = TimeSpan.FromSeconds(30)
    };
    service = new HttpRequestResultServicePolly(
        provider.GetRequiredService<ILogger<HttpRequestResultServicePolly>>(),
  service,
    pollyOptions
    );

    // Layer 3: Add Telemetry (captures timing and metrics)
    service = new HttpRequestResultServiceTelemetry(
  provider.GetRequiredService<ILogger<HttpRequestResultServiceTelemetry>>(),
        service
    );

    return service;
});

// 6. Utility Services
builder.Services.AddSingleton<FireAndForgetUtility>();
builder.Services.AddScoped<HttpClientConcurrentProcessor>();

// 7. Demo Application Services
builder.Services.AddScoped<JokeApiService>();

// 8. OpenTelemetry (Optional - for advanced monitoring)
// Uncomment to enable OpenTelemetry tracing
/*
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerBuilder =>
    {
tracerBuilder
 .AddSource("WebSpark.HttpClientUtility.Web")
          .AddHttpClientInstrumentation()
     .AddAspNetCoreInstrumentation()
    .AddConsoleExporter();
    });
*/

// ============================================================================
// End of WebSpark.HttpClientUtility Configuration
// ============================================================================

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
