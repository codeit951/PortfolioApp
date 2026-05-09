using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PortfolioApp.Infrastructure.Data;

/// <summary>
/// Allows EF Core CLI tools to create the DbContext at design time without a running host.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlite("Data Source=portfolio.db");
        return new AppDbContext(optionsBuilder.Options);
    }
}
