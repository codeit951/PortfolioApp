using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PortfolioApp.Domain.Entities;

namespace PortfolioApp.Infrastructure.Data;

public class DataSeeder
{
    private readonly AppDbContext _db;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<DataSeeder> _logger;

    public DataSeeder(AppDbContext db, UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager, ILogger<DataSeeder> logger)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
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
        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
            _logger.LogInformation("Admin role created.");
        }
    }

    private async Task SeedAdminUserAsync()
    {
        const string adminEmail = "admin@codeit.com.ng";
        const string adminPassword = "Codeit@951";

        if (await _userManager.FindByEmailAsync(adminEmail) is null)
        {
            var user = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, adminPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Admin");
                _logger.LogInformation("Admin user seeded: {Email}", adminEmail);
            }
        }
    }

    private async Task SeedProfileAsync()
    {
        if (!await _db.UserProfiles.AnyAsync())
        {
            _db.UserProfiles.Add(new UserProfile
            {
                FullName = "Peter Anthony",
                Title = "Senior Full Stack Developer",
                Bio = "Passionate software engineer with 8+ years building scalable web applications. " +
                      "I love clean code, great UX, and solving hard problems.",
                ProfileImageUrl = "/images/profile.jpg",
                Email = "Codeit951@gmail.com",
                Phone = "+1 (555) 123-4567",
                Location = "San Francisco, CA",
                GitHubUrl = "https://github.com/codeit951",
                LinkedInUrl = "https://linkedin.com/in/codeit951",
                TwitterUrl = "https://twitter.com/codeit951",
                CvFileUrl = "/files/alex-morgan-cv.pdf",
                Summary = "Senior Full Stack Developer specializing in .NET, Blazor, and React. " +
                          "Strong background in cloud-native architecture, microservices, and DevOps practices. " +
                          "Proven track record of delivering high-quality software on time."
            });
            await _db.SaveChangesAsync();
        }
    }

    private async Task SeedQualificationsAsync()
    {
        if (!await _db.Qualifications.AnyAsync())
        {
            _db.Qualifications.AddRange(
                new Qualification { Title = "BSc Computer Science", Institution = "University of California, Berkeley", Year = 2016, Grade = "First Class Honours", Description = "Focused on software engineering, algorithms, and distributed systems." },
                new Qualification { Title = "MSc Software Engineering", Institution = "Stanford University", Year = 2018, Grade = "Distinction", Description = "Specialised in cloud computing, machine learning, and enterprise architecture." },
                new Qualification { Title = "Microsoft Certified: Azure Developer Associate", Institution = "Microsoft", Year = 2020, Description = "Expertise in developing, testing, and maintaining Azure cloud solutions." },
                new Qualification { Title = "AWS Certified Solutions Architect", Institution = "Amazon Web Services", Year = 2021, Grade = "Passed", Description = "Designing scalable, cost-efficient, and fault-tolerant systems on AWS." },
                new Qualification { Title = "Docker Certified Associate", Institution = "Docker Inc.", Year = 2022, Description = "Advanced container management, orchestration, and security." }
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
                    Company = "TechNova Inc.",
                    Role = "Senior Full Stack Developer",
                    StartDate = new DateTime(2022, 3, 1),
                    IsCurrent = true,
                    Location = "San Francisco, CA (Remote)",
                    Description = "Lead developer on a SaaS platform serving 50,000+ users. " +
                                  "Architected microservices migration, reducing latency by 40%. " +
                                  "Mentored a team of 5 junior developers. Tech: .NET 8, Blazor, React, Azure, PostgreSQL."
                },
                new Experience
                {
                    Company = "CloudSoft Solutions",
                    Role = "Full Stack Developer",
                    StartDate = new DateTime(2019, 6, 1),
                    EndDate = new DateTime(2022, 2, 28),
                    Location = "New York, NY",
                    Description = "Built and maintained enterprise ERP systems. Improved CI/CD pipeline, " +
                                  "cutting deployment time by 60%. Integrated third-party APIs for payments and logistics. " +
                                  "Tech: ASP.NET Core, Angular, SQL Server, Docker."
                },
                new Experience
                {
                    Company = "StartupXYZ",
                    Role = "Junior .NET Developer",
                    StartDate = new DateTime(2018, 8, 1),
                    EndDate = new DateTime(2019, 5, 31),
                    Location = "Remote",
                    Description = "Developed REST APIs and web interfaces for an e-commerce startup. " +
                                  "Implemented real-time features using SignalR. " +
                                  "Tech: ASP.NET Core, Vue.js, MySQL, Redis."
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
                // Languages
                new Skill { Name = "C#", Category = "Languages", Level = 90, IconClass = "bi bi-code-slash" },
                new Skill { Name = "JavaScript", Category = "Languages", Level = 60, IconClass = "bi bi-code" },
                new Skill { Name = "HTML", Category = "Languages", Level = 80, IconClass = "bi bi-code-square" },
                new Skill { Name = "CSS", Category = "Languages", Level = 70, IconClass = "bi bi-palette" },
                new Skill { Name = "JQuery", Category = "Languages", Level = 60, IconClass = "bi bi-code-square" },
                new Skill { Name = "SQL", Category = "Languages", Level = 79, IconClass = "bi bi-database" },
                // Frameworks
                new Skill { Name = "ASP.NET Core", Category = "Frameworks", Level = 90, IconClass = "bi bi-braces" },
                new Skill { Name = "Blazor", Category = "Frameworks", Level = 85, IconClass = "bi bi-lightning" },
                new Skill { Name = "React", Category = "Frameworks", Level = 10, IconClass = "bi bi-palette" },
                new Skill { Name = "Entity Framework Core", Category = "Frameworks", Level = 80, IconClass = "bi bi-table" },
                // Tools & Platforms
                new Skill { Name = "Azure", Category = "Tools & Platforms", Level = 82, IconClass = "bi bi-cloud" },
                new Skill { Name = "CPanel", Category = "Tools & Platforms", Level = 85, IconClass = "bi bi-cpu" },
                new Skill { Name = "Git / GitHub", Category = "Tools & Platforms", Level = 92, IconClass = "bi bi-git" }
            );
            await _db.SaveChangesAsync();
        }
    }

    private async Task SeedProjectsAsync()
    {
        if (!await _db.Projects.AnyAsync())
        {
            _db.Projects.AddRange(
                new Project
                {
                    Title = "CloudERP Platform",
                    Description = "Enterprise resource planning system built for mid-size companies. Features include inventory management, HR, finance, and real-time reporting dashboards.",
                    TechStack = ".NET 8, Blazor Server, SQL Server, Azure, SignalR, Docker",
                    GitHubUrl = "https://github.com/alexmorgan/cloud-erp",
                    DemoUrl = "https://clouderp-demo.azurewebsites.net",
                    IsFeatured = true,
                    CreatedAt = new DateTime(2023, 6, 1)
                },
                new Project
                {
                    Title = "DevTrack – Issue Tracker",
                    Description = "A GitHub-inspired issue and project tracking tool with boards, sprints, and team collaboration features. Supports real-time updates via WebSockets.",
                    TechStack = "ASP.NET Core API, React, TypeScript, PostgreSQL, Redis",
                    GitHubUrl = "https://github.com/alexmorgan/devtrack",
                    DemoUrl = "https://devtrack.io",
                    IsFeatured = true,
                    CreatedAt = new DateTime(2022, 11, 15)
                },
                new Project
                {
                    Title = "AI Code Reviewer",
                    Description = "Automated PR code reviewer powered by OpenAI GPT. Integrates with GitHub webhooks to provide inline review comments and quality scores.",
                    TechStack = "Python, FastAPI, OpenAI API, GitHub Actions, Docker",
                    GitHubUrl = "https://github.com/alexmorgan/ai-code-reviewer",
                    IsFeatured = true,
                    CreatedAt = new DateTime(2024, 1, 20)
                },
                new Project
                {
                    Title = "Personal Portfolio (This Site)",
                    Description = "A fully dynamic developer portfolio built with Blazor Server, EF Core, and SQLite. Features an admin panel, dark mode, and fully database-driven content.",
                    TechStack = ".NET 8, Blazor Server, EF Core, SQLite, Bootstrap 5",
                    GitHubUrl = "https://github.com/alexmorgan/blazor-portfolio",
                    IsFeatured = false,
                    CreatedAt = new DateTime(2024, 5, 1)
                }
            );
            await _db.SaveChangesAsync();
        }
    }
}
