using WebSpark.HttpClientUtility.ClientService;
using WebSpark.HttpClientUtility.RequestResult;
using WebSpark.HttpClientUtility.StringConverter;
using WebSpark.HttpClientUtility.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure HttpClientUtility services (From NuGet package)
builder.Services.AddHttpClient();

// Register JSON converter
builder.Services.AddSingleton<IStringConverter, SystemJsonStringConverter>();

// Register base HTTP client service
builder.Services.AddScoped<IHttpClientService, HttpClientService>();

// Register base HttpRequestResultService
builder.Services.AddScoped<HttpRequestResultService>();

// Register IHttpRequestResultService with decorator chain
builder.Services.AddScoped<IHttpRequestResultService>(provider =>
{
    // Start with the base service
    IHttpRequestResultService service = provider.GetRequiredService<HttpRequestResultService>();

    // Add Telemetry decorator (outermost layer)
  service = new HttpRequestResultServiceTelemetry(
        provider.GetRequiredService<ILogger<HttpRequestResultServiceTelemetry>>(),
        service
    );

    return service;
});

// Register demo services
builder.Services.AddScoped<JokeApiService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
