using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortfolioApp.Domain.Entities;

namespace PortfolioApp.Infrastructure.Data;

public class AdminUserOptions
{
    public const string SectionName = "AdminUser";

    /// <summary>Email of the admin account created on first run.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Initial password for the admin account. Only used when the account is first created.
    /// If empty, a random password is generated and written to the log once.
    /// </summary>
    public string Password { get; set; } = string.Empty;
}

public class DataSeeder
{
    public const string AdminRole = "Admin";

    private readonly AppDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AdminUserOptions _adminOptions;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(AppDbContext db, UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager, IOptions<AdminUserOptions> adminOptions, ILogger<DataSeeder> logger)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
        _adminOptions = adminOptions.Value;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await _db.Database.MigrateAsync();

        await SeedRolesAsync();
        await SeedAdminUserAsync();
        await SeedProfileAsync();
        await SeedQualificationsAsync();
        await SeedExperiencesAsync();
        await SeedSkillsAsync();
        await SeedProjectsAsync();
    }

    private async Task SeedRolesAsync()
    {
        if (!await _roleManager.RoleExistsAsync(AdminRole))
        {
            await _roleManager.CreateAsync(new IdentityRole(AdminRole));
            _logger.LogInformation("Admin role created.");
        }
    }

    private async Task SeedAdminUserAsync()
    {
        // Only create an admin if none exists yet; never overwrite an existing password.
        if ((await _userManager.GetUsersInRoleAsync(AdminRole)).Count > 0) return;

        var email = _adminOptions.Email;
        if (string.IsNullOrWhiteSpace(email))
        {
            _logger.LogWarning("No admin user exists and AdminUser:Email is not configured. " +
                               "Set AdminUser:Email (and optionally AdminUser:Password) and restart to create one.");
            return;
        }

        var existing = await _userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            await _userManager.AddToRoleAsync(existing, AdminRole);
            return;
        }

        var password = _adminOptions.Password;
        var generated = string.IsNullOrWhiteSpace(password);
        if (generated) password = GeneratePassword();

        var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            _logger.LogError("Could not create admin user {Email}: {Errors}", email,
                string.Join("; ", result.Errors.Select(e => e.Description)));
            return;
        }

        await _userManager.AddToRoleAsync(user, AdminRole);
        if (generated)
            _logger.LogWarning("Admin user {Email} created with generated password: {Password}  " +
                               "- sign in and change it at /admin/account. This is only shown once.", email, password);
        else
            _logger.LogInformation("Admin user {Email} created from configuration.", email);
    }

    private static string GeneratePassword()
    {
        // 18 random URL-safe chars plus a fixed suffix that guarantees Identity's complexity rules.
        var random = Convert.ToBase64String(RandomNumberGenerator.GetBytes(14))
            .Replace('+', 'x').Replace('/', 'y').TrimEnd('=');
        return random + "a1!A";
    }

    // ── Sample content ─────────────────────────────────────────────────────────
    // Seeded only into an empty database so the site isn't blank on first run.
    // Replace it from the admin panel.

    private async Task SeedProfileAsync()
    {
        if (!await _db.UserProfiles.AnyAsync())
        {
            _db.UserProfiles.Add(new UserProfile
            {
                FullName = "Peter Anthony",
                Title = "Senior Full Stack Developer",
                Bio = "Passionate software engineer building scalable web applications. " +
                      "I love clean code, great UX, and solving hard problems.",
                ProfileImageUrl = "/images/profile.jpg",
                Email = "Codeit951@gmail.com",
                GitHubUrl = "https://github.com/codeit951",
                Summary = "Full Stack Developer specializing in .NET and Blazor. " +
                          "Edit this summary from the admin panel (/admin/profile)."
            });
            await _db.SaveChangesAsync();
        }
    }

    private async Task SeedQualificationsAsync()
    {
        if (!await _db.Qualifications.AnyAsync())
        {
            _db.Qualifications.AddRange(
                new Qualification { Title = "BSc Computer Science", Institution = "Sample University", Year = 2016, Grade = "First Class Honours", Description = "Sample entry — edit or delete it in the admin panel." },
                new Qualification { Title = "Microsoft Certified: Azure Developer Associate", Institution = "Microsoft", Year = 2020, Description = "Sample entry — edit or delete it in the admin panel." }
            );
            await _db.SaveChangesAsync();
        }
    }

    private async Task SeedExperiencesAsync()
    {
        if (!await _db.Experiences.AnyAsync())
        {
            _db.Experiences.AddRange(
                new Experience
                {
                    Company = "Sample Company",
                    Role = "Senior Full Stack Developer",
                    StartDate = new DateTime(2022, 3, 1),
                    IsCurrent = true,
                    Location = "Remote",
                    Description = "Sample entry — replace it with your own experience in the admin panel."
                },
                new Experience
                {
                    Company = "Another Sample Company",
                    Role = "Full Stack Developer",
                    StartDate = new DateTime(2019, 6, 1),
                    EndDate = new DateTime(2022, 2, 28),
                    Location = "Remote",
                    Description = "Sample entry — replace it with your own experience in the admin panel."
                }
            );
            await _db.SaveChangesAsync();
        }
    }

    private async Task SeedSkillsAsync()
    {
        if (!await _db.Skills.AnyAsync())
        {
            _db.Skills.AddRange(
                new Skill { Name = "C#", Category = "Languages", Level = 90, IconClass = "bi bi-code-slash" },
                new Skill { Name = "JavaScript", Category = "Languages", Level = 60, IconClass = "bi bi-code" },
                new Skill { Name = "HTML", Category = "Languages", Level = 80, IconClass = "bi bi-code-square" },
                new Skill { Name = "CSS", Category = "Languages", Level = 70, IconClass = "bi bi-palette" },
                new Skill { Name = "SQL", Category = "Languages", Level = 79, IconClass = "bi bi-database" },
                new Skill { Name = "ASP.NET Core", Category = "Frameworks", Level = 90, IconClass = "bi bi-braces" },
                new Skill { Name = "Blazor", Category = "Frameworks", Level = 85, IconClass = "bi bi-lightning" },
                new Skill { Name = "Entity Framework Core", Category = "Frameworks", Level = 80, IconClass = "bi bi-table" },
                new Skill { Name = "Azure", Category = "Tools & Platforms", Level = 82, IconClass = "bi bi-cloud" },
                new Skill { Name = "Git / GitHub", Category = "Tools & Platforms", Level = 92, IconClass = "bi bi-git" }
            );
            await _db.SaveChangesAsync();
        }
    }

    private async Task SeedProjectsAsync()
    {
        if (!await _db.Projects.AnyAsync())
        {
            _db.Projects.Add(new Project
            {
                Title = "Personal Portfolio (This Site)",
                Description = "A fully dynamic developer portfolio built with Blazor Server, EF Core, and SQLite. Features an admin panel, dark mode, and fully database-driven content.",
                TechStack = ".NET 8, Blazor Server, EF Core, SQLite, Bootstrap 5",
                GitHubUrl = "https://github.com/codeit951/PortfolioApp",
                IsFeatured = true,
                CreatedAt = new DateTime(2024, 5, 1)
            });
            await _db.SaveChangesAsync();
        }
    }
}
