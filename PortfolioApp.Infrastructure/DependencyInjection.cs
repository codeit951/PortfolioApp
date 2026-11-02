using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PortfolioApp.Application.Interfaces;
using PortfolioApp.Infrastructure.Data;
using PortfolioApp.Infrastructure.Services;

namespace PortfolioApp.Infrastructure;

public static class DependencyInjection
{
    /// <param name="contentRootPath">
    /// A relative SQLite "Data Source" is resolved against this folder, so the database location
    /// does not depend on the directory the app was started from.
    /// </param>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config, string contentRootPath)
    {
        var connectionString = ResolveSqlitePath(
            config.GetConnectionString("DefaultConnection") ?? "Data Source=portfolio.db",
            contentRootPath);

        // Services use the factory (one short-lived context per operation, safe for Blazor Server).
        // Identity's stores still need a scoped AppDbContext, which the factory also provides.
        services.AddDbContextFactory<AppDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped(sp => sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

        services.Configure<SmtpOptions>(config.GetSection(SmtpOptions.SectionName));
        services.Configure<AdminUserOptions>(config.GetSection(AdminUserOptions.SectionName));

        services.AddScoped<IUserProfileService, UserProfileService>();
        services.AddScoped<IQualificationService, QualificationService>();
        services.AddScoped<IExperienceService, ExperienceService>();
        services.AddScoped<ISkillService, SkillService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IMessageNotifier, SmtpMessageNotifier>();
        services.AddScoped<DataSeeder>();

        return services;
    }

    public static string ResolveSqlitePath(string connectionString, string contentRootPath)
    {
        var builder = new SqliteConnectionStringBuilder(connectionString);
        if (!string.IsNullOrEmpty(builder.DataSource)
            && builder.DataSource != ":memory:"
            && !Path.IsPathRooted(builder.DataSource))
        {
            builder.DataSource = Path.GetFullPath(Path.Combine(contentRootPath, builder.DataSource));
        }
        var dir = Path.GetDirectoryName(builder.DataSource);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        return builder.ToString();
    }
}
