using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortfolioApp.Infrastructure.Data;

namespace PortfolioApp.Tests;

public class SeederTests : IDisposable
{
    private readonly TestDb _db = new();
    public void Dispose() => _db.Dispose();

    private ServiceProvider BuildServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped(_ => _db.CreateDbContext());
        services.AddIdentityCore<IdentityUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>();
        return services.BuildServiceProvider();
    }

    private static async Task<(DataSeeder seeder, UserManager<IdentityUser> users)> Create(IServiceProvider sp, AdminUserOptions options)
    {
        var scope = sp.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var seeder = new DataSeeder(
            scope.ServiceProvider.GetRequiredService<AppDbContext>(),
            users,
            scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>(),
            Options.Create(options),
            scope.ServiceProvider.GetRequiredService<ILogger<DataSeeder>>());
        await Task.CompletedTask;
        return (seeder, users);
    }

    [Fact]
    public async Task Creates_admin_from_configured_credentials()
    {
        using var sp = BuildServices();
        var (seeder, users) = await Create(sp, new AdminUserOptions { Email = "owner@example.com", Password = "Str0ng!Pass" });
        await seeder.SeedAsync();

        var user = await users.FindByEmailAsync("owner@example.com");
        Assert.NotNull(user);
        Assert.True(await users.IsInRoleAsync(user!, DataSeeder.AdminRole));
        Assert.True(await users.CheckPasswordAsync(user!, "Str0ng!Pass"));
    }

    [Fact]
    public async Task Generates_password_when_none_configured()
    {
        using var sp = BuildServices();
        var (seeder, users) = await Create(sp, new AdminUserOptions { Email = "owner@example.com" });
        await seeder.SeedAsync();

        var user = await users.FindByEmailAsync("owner@example.com");
        Assert.NotNull(user);
        Assert.True(await users.HasPasswordAsync(user!));
        Assert.True(await users.IsInRoleAsync(user!, DataSeeder.AdminRole));
    }

    [Fact]
    public async Task Does_not_create_admin_without_email()
    {
        using var sp = BuildServices();
        var (seeder, users) = await Create(sp, new AdminUserOptions());
        await seeder.SeedAsync();

        Assert.Empty(users.Users);
    }

    [Fact]
    public async Task Never_replaces_an_existing_admin()
    {
        using var sp = BuildServices();
        var (first, users) = await Create(sp, new AdminUserOptions { Email = "owner@example.com", Password = "Str0ng!Pass" });
        await first.SeedAsync();

        var (second, _) = await Create(sp, new AdminUserOptions { Email = "other@example.com", Password = "0ther!Pass" });
        await second.SeedAsync();

        Assert.Null(await users.FindByEmailAsync("other@example.com"));
    }
}
