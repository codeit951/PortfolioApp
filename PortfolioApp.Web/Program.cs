using System.Security.Claims;
using Blazored.Toast;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.FileProviders;
using PortfolioApp.Infrastructure;
using PortfolioApp.Infrastructure.Data;
using PortfolioApp.Web.Components;
using PortfolioApp.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        // Only show exception details to the browser while developing.
        options.DetailedErrors = builder.Environment.IsDevelopment();
    });

builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);

builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.SignIn.RequireConfirmedAccount = false;

        // Brute-force protection: lock the account for 15 minutes after 5 failed attempts.
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Optional social login. A provider is only enabled when its ClientId and ClientSecret are configured.
var authBuilder = builder.Services.AddAuthentication();
var google = builder.Configuration.GetSection("Authentication:Google");
if (!string.IsNullOrWhiteSpace(google["ClientId"]) && !string.IsNullOrWhiteSpace(google["ClientSecret"]))
{
    authBuilder.AddGoogle(o =>
    {
        o.ClientId = google["ClientId"]!;
        o.ClientSecret = google["ClientSecret"]!;
        o.CallbackPath = "/signin-google";
    });
}
var github = builder.Configuration.GetSection("Authentication:GitHub");
if (!string.IsNullOrWhiteSpace(github["ClientId"]) && !string.IsNullOrWhiteSpace(github["ClientSecret"]))
{
    authBuilder.AddGitHub(o =>
    {
        o.ClientId = github["ClientId"]!;
        o.ClientSecret = github["ClientSecret"]!;
        o.CallbackPath = "/signin-github";
        o.Scope.Add("user:email");
    });
}

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/admin/login";
    options.AccessDeniedPath = "/admin/login";
    options.Cookie.Name = "PortfolioAuth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
});

// Persist data-protection keys (auth cookies, antiforgery) when a path is configured, e.g. in Docker,
// so users aren't signed out every time the container restarts.
var keysPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(keysPath))
{
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(Path.GetFullPath(keysPath, builder.Environment.ContentRootPath)))
        .SetApplicationName("PortfolioApp");
}

builder.Services.AddBlazoredToast();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<FileStorageService>();

// ── Build ──────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Seed DB ────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}

// ── Middleware ─────────────────────────────────────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found");
app.UseHttpsRedirection();
app.UseStaticFiles();

// Uploads stored outside wwwroot (Storage:UploadsPath) are served at /uploads.
var uploadsRoot = FileStorageService.ResolveRoot(app.Configuration, app.Environment);
var defaultUploadsRoot = Path.Combine(app.Environment.WebRootPath, "uploads");
if (!string.Equals(Path.GetFullPath(uploadsRoot), Path.GetFullPath(defaultUploadsRoot), StringComparison.OrdinalIgnoreCase))
{
    Directory.CreateDirectory(uploadsRoot);
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(uploadsRoot),
        RequestPath = FileStorageService.RequestPath
    });
}

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// ── Account endpoints ──────────────────────────────────────────────────────
app.MapPost("/admin/logout", async (SignInManager<IdentityUser> signInManager) =>
{
    await signInManager.SignOutAsync();
    return Results.Redirect("/");
});

app.MapGet("/admin/external-login/{provider}", async (string provider, SignInManager<IdentityUser> signInManager) =>
{
    var schemes = await signInManager.GetExternalAuthenticationSchemesAsync();
    if (!schemes.Any(s => s.Name == provider))
        return Results.Redirect("/admin/login?error=provider");

    var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, "/admin/external-callback");
    return Results.Challenge(properties, new[] { provider });
});

app.MapGet("/admin/external-callback", async (HttpContext http, SignInManager<IdentityUser> signInManager,
    UserManager<IdentityUser> userManager, ILogger<Program> logger) =>
{
    var info = await signInManager.GetExternalLoginInfoAsync();
    if (info is null)
        return Results.Redirect("/admin/login?error=external");

    var result = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey,
        isPersistent: false, bypassTwoFactor: true);
    if (result.Succeeded)
        return Results.Redirect("/admin");
    if (result.IsLockedOut)
        return Results.Redirect("/admin/login?error=locked");

    // First external sign-in: link it only to an existing admin account with the same email.
    // External logins never create new accounts.
    var email = info.Principal.FindFirstValue(ClaimTypes.Email);
    if (!string.IsNullOrWhiteSpace(email))
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is not null && await userManager.IsInRoleAsync(user, DataSeeder.AdminRole))
        {
            var link = await userManager.AddLoginAsync(user, info);
            if (link.Succeeded)
            {
                await signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                return Results.Redirect("/admin");
            }
        }
    }

    logger.LogWarning("Rejected {Provider} sign-in for {Email}: no matching admin account.", info.LoginProvider, email ?? "(no email)");
    await http.SignOutAsync(IdentityConstants.ExternalScheme);
    return Results.Redirect("/admin/login?error=denied");
});

app.Run();

public partial class Program { }
