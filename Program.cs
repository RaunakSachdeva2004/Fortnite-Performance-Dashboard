using FortniteDashboard.Data;
using FortniteDashboard.Models;
using FortniteDashboard.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Disable reloadOnChange to prevent Linux inotify user instance limit exceptions on containerized cloud hosts (e.g. Render.com)
builder.Configuration.Sources.Clear();
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);
builder.Configuration.AddEnvironmentVariables();

// ---- MVC ----
builder.Services.AddControllersWithViews();

// ---- Database (SQLite / SQL Server switch) ----
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

var provider = builder.Configuration["DatabaseProvider"] ?? "SQLite";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseSqlite(connectionString);
    }
});

// ---- Authentication (Cookie-based Identity) ----
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Error/403";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// ---- Application Services (Dependency Injection) ----
builder.Services.AddHttpClient<IFortniteApiClient, FortniteApiClient>();
builder.Services.AddScoped<IStatsService, StatsService>();
builder.Services.AddScoped<IRecommendationEngine, RuleBasedRecommendationEngine>();
builder.Services.AddScoped<IPerformanceService, PerformanceService>();
builder.Services.AddScoped<IMatchService, MatchService>();
builder.Services.AddScoped<IGameModeService, GameModeService>();

var app = builder.Build();

// ---- HTTP Request Pipeline ----
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ---- Health Check Endpoint ----
app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Timestamp = DateTime.UtcNow,
    Environment = app.Environment.EnvironmentName,
    DatabaseProvider = provider
}));

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ---- Automatic Database Migration & Seeding ----
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        db.Database.Migrate();
        logger.LogInformation("Database migration applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while applying database migrations.");
    }
}

app.Run();
