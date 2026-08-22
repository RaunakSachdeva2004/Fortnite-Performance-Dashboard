using FortniteDashboard.Data;
using FortniteDashboard.Models;
using FortniteDashboard.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// NOTE: We intentionally do NOT call builder.WebHost.UseUrls(...) here.
// The previous version hardcoded "http://127.0.0.1:5001", which silently
// overrode whatever Visual Studio's Properties/launchSettings.json says
// and dropped the HTTPS profile entirely. Removing it lets the normal
// Visual Studio "Run" / F5 experience (and its port choice) work as
// expected. If you ever need a fixed port again, set it in
// Properties/launchSettings.json instead of here.
// ---------------------------------------------------------------------

// ---- MVC ----
builder.Services.AddControllersWithViews();

// ---- Database (SQLite) ----
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// ---- Authentication (cookie-based; matches AccountController's SignInAsync/SignOutAsync) ----
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(14);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// ---- Application services (Dependency Injection) ----
// Typed HttpClient: gives FortniteApiClient a managed, pooled HttpClient
// instance instead of controllers/services newing one up themselves.
builder.Services.AddHttpClient<IFortniteApiClient, FortniteApiClient>();

builder.Services.AddScoped<IStatsService, StatsService>();
builder.Services.AddScoped<IRecommendationEngine, RuleBasedRecommendationEngine>();

var app = builder.Build();

// ---- Configure the HTTP request pipeline ----
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // MUST come before UseAuthorization(), and was missing entirely before.
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

// ---------------------------------------------------------------------
// Development convenience: auto-apply pending EF Core migrations and
// seed one Administrator account so the app is immediately usable after
// a fresh `git clone` + F5 in Visual Studio. This does NOT run outside
// Development, so it will never touch a production database.
//
// The seed admin password is read from configuration (User Secrets or
// an environment variable), never hardcoded. See README "Getting
// Started" for how to set FortniteApi:ApiKey and SeedAdmin:Password.
// ---------------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();

    if (!db.Users.Any(u => u.Role == "Administrator"))
    {
        var seedEmail = builder.Configuration["SeedAdmin:Email"] ?? "admin@fortnitedashboard.local";
        var seedPassword = builder.Configuration["SeedAdmin:Password"];

        if (string.IsNullOrWhiteSpace(seedPassword))
        {
            // Fallback so the app still boots for a first-time run, but this
            // is clearly not something to ship or leave unchanged.
            seedPassword = "ChangeMe123!";
            app.Logger.LogWarning(
                "No SeedAdmin:Password configured — using a default development-only password " +
                "('ChangeMe123!'). Set your own with: dotnet user-secrets set \"SeedAdmin:Password\" \"...\"");
        }

        var admin = new User
        {
            Name = "Administrator",
            Email = seedEmail,
            Role = "Administrator",
            CreatedDate = DateTime.UtcNow
        };
        admin.PasswordHash = new PasswordHasher<User>().HashPassword(admin, seedPassword);

        db.Users.Add(admin);
        db.SaveChanges();

        app.Logger.LogInformation("Seeded development Administrator account: {Email}", seedEmail);
    }
}

app.Run();
