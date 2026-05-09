using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PortfolioApp.Domain.Entities;

namespace PortfolioApp.Infrastructure.Data;

public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<Qualification> Qualifications { get; set; }
    public DbSet<Experience> Experiences { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserProfile>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FullName).IsRequired().HasMaxLength(200);
            e.Property(x => x.Email).HasMaxLength(200);
            e.Property(x => x.Bio).HasMaxLength(2000);
            e.Property(x => x.Summary).HasMaxLength(2000);
        });

        builder.Entity<Qualification>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(300);
            e.Property(x => x.Institution).IsRequired().HasMaxLength(300);
        });

        builder.Entity<Experience>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Company).IsRequired().HasMaxLength(300);
            e.Property(x => x.Role).IsRequired().HasMaxLength(300);
            e.Property(x => x.Description).HasMaxLength(3000);
        });

        builder.Entity<Skill>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Category).IsRequired().HasMaxLength(100);
        });

        builder.Entity<Project>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(300);
            e.Property(x => x.Description).HasMaxLength(3000);
            e.Property(x => x.TechStack).HasMaxLength(1000);
        });

        builder.Entity<Message>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.Email).IsRequired().HasMaxLength(200);
            e.Property(x => x.Content).IsRequired().HasMaxLength(3000);
        });
    }
}
