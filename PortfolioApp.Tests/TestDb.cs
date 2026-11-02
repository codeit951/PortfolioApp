using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PortfolioApp.Infrastructure.Data;

namespace PortfolioApp.Tests;

/// <summary>An in-memory SQLite database that lives as long as this object.</summary>
public sealed class TestDb : IDbContextFactory<AppDbContext>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public TestDb()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = CreateDbContext();
        db.Database.Migrate();
    }

    public AppDbContext CreateDbContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}
