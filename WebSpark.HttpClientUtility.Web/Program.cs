using WebSpark.HttpClientUtility;
using WebSpark.HttpClientUtility.Crawler;
using WebSpark.HttpClientUtility.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ========================================
// Configure HttpClientUtility services (v2.0.0 Two-Package Pattern)
// ========================================
// Base package: Core HTTP utilities
builder.Services.AddHttpClientUtility();

// Crawler package: Web crawling features (requires base package)
builder.Services.AddHttpClientCrawler();
// ========================================

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

#if NET9_0_OR_GREATER
app.MapStaticAssets();
#else
app.UseStaticFiles();
#endif

app.UseRouting();

app.UseAuthorization();

// Map SignalR hub for crawler progress updates
app.MapHub<CrawlHub>("/crawlHub");

#if NET9_0_OR_GREATER
app.MapControllerRoute(
 name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
#else
app.MapControllerRoute(
    name: "default",
  pattern: "{controller=Home}/{action=Index}/{id?}");
#endif

app.Run();
