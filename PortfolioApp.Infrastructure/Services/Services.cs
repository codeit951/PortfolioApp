using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PortfolioApp.Application.Interfaces;
using PortfolioApp.Domain.Entities;
using PortfolioApp.Infrastructure.Data;

namespace PortfolioApp.Infrastructure.Services;

// All services create a short-lived DbContext per operation. Blazor Server keeps a DI scope
// alive for the whole circuit, so a scoped DbContext would accumulate tracked entities and
// fail on the second edit of the same record or on overlapping async calls.

public class UserProfileService : IUserProfileService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    public UserProfileService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<UserProfile?> GetProfileAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.UserProfiles.AsNoTracking().OrderBy(p => p.Id).FirstOrDefaultAsync();
    }

    public async Task<UserProfile> UpsertProfileAsync(UserProfile profile)
    {
        await using var db = await _factory.CreateDbContextAsync();
        var existing = await db.UserProfiles.OrderBy(p => p.Id).FirstOrDefaultAsync();
        if (existing is null)
        {
            profile.Id = 0;
            db.UserProfiles.Add(profile);
            await db.SaveChangesAsync();
            return profile;
        }

        existing.FullName = profile.FullName;
        existing.Title = profile.Title;
        existing.Bio = profile.Bio;
        existing.ProfileImageUrl = profile.ProfileImageUrl;
        existing.Email = profile.Email;
        existing.Phone = profile.Phone;
        existing.Location = profile.Location;
        existing.GitHubUrl = profile.GitHubUrl;
        existing.LinkedInUrl = profile.LinkedInUrl;
        existing.TwitterUrl = profile.TwitterUrl;
        existing.CvFileUrl = profile.CvFileUrl;
        existing.Summary = profile.Summary;
        await db.SaveChangesAsync();
        return existing;
    }
}

public class QualificationService : IQualificationService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    public QualificationService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<List<Qualification>> GetAllAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Qualifications.AsNoTracking().OrderByDescending(q => q.Year).ToListAsync();
    }

    public async Task<Qualification?> GetByIdAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Qualifications.AsNoTracking().FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<Qualification> CreateAsync(Qualification q)
    {
        await using var db = await _factory.CreateDbContextAsync();
        db.Qualifications.Add(q);
        await db.SaveChangesAsync();
        return q;
    }

    public async Task<Qualification> UpdateAsync(Qualification q)
    {
        await using var db = await _factory.CreateDbContextAsync();
        db.Qualifications.Update(q);
        await db.SaveChangesAsync();
        return q;
    }

    public async Task DeleteAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        await db.Qualifications.Where(q => q.Id == id).ExecuteDeleteAsync();
    }
}

public class ExperienceService : IExperienceService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    public ExperienceService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<List<Experience>> GetAllAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Experiences.AsNoTracking().OrderByDescending(e => e.StartDate).ToListAsync();
    }

    public async Task<Experience?> GetByIdAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Experiences.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Experience> CreateAsync(Experience e)
    {
        Normalize(e);
        await using var db = await _factory.CreateDbContextAsync();
        db.Experiences.Add(e);
        await db.SaveChangesAsync();
        return e;
    }

    public async Task<Experience> UpdateAsync(Experience e)
    {
        Normalize(e);
        await using var db = await _factory.CreateDbContextAsync();
        db.Experiences.Update(e);
        await db.SaveChangesAsync();
        return e;
    }

    public async Task DeleteAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        await db.Experiences.Where(e => e.Id == id).ExecuteDeleteAsync();
    }

    // A current role has no end date.
    private static void Normalize(Experience e)
    {
        if (e.IsCurrent) e.EndDate = null;
    }
}

public class SkillService : ISkillService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    public SkillService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<List<Skill>> GetAllAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Skills.AsNoTracking().OrderBy(s => s.Category).ThenByDescending(s => s.Level).ToListAsync();
    }

    public async Task<Dictionary<string, List<Skill>>> GetGroupedByCategory()
    {
        var skills = await GetAllAsync();
        return skills.GroupBy(s => s.Category).ToDictionary(g => g.Key, g => g.ToList());
    }

    public async Task<Skill?> GetByIdAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Skills.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Skill> CreateAsync(Skill s)
    {
        s.Level = Math.Clamp(s.Level, 0, 100);
        await using var db = await _factory.CreateDbContextAsync();
        db.Skills.Add(s);
        await db.SaveChangesAsync();
        return s;
    }

    public async Task<Skill> UpdateAsync(Skill s)
    {
        s.Level = Math.Clamp(s.Level, 0, 100);
        await using var db = await _factory.CreateDbContextAsync();
        db.Skills.Update(s);
        await db.SaveChangesAsync();
        return s;
    }

    public async Task DeleteAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        await db.Skills.Where(s => s.Id == id).ExecuteDeleteAsync();
    }
}

public class ProjectService : IProjectService
{
    private readonly IDbContextFactory<AppDbContext> _factory;
    public ProjectService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<List<Project>> GetAllAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Projects.AsNoTracking().OrderByDescending(p => p.CreatedAt).ToListAsync();
    }

    public async Task<List<Project>> GetPagedAsync(int page, int pageSize, string? searchTerm = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        await using var db = await _factory.CreateDbContextAsync();
        return await ApplySearch(db.Projects.AsNoTracking(), searchTerm)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(string? searchTerm = null)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await ApplySearch(db.Projects, searchTerm).CountAsync();
    }

    // Case-insensitive: SQLite's instr()/Contains is case-sensitive, so compare lower-cased values.
    private static IQueryable<Project> ApplySearch(IQueryable<Project> query, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm)) return query;
        var term = searchTerm.Trim().ToLower();
        return query.Where(p => p.Title.ToLower().Contains(term)
                             || p.Description.ToLower().Contains(term)
                             || p.TechStack.ToLower().Contains(term));
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Project> CreateAsync(Project p)
    {
        p.CreatedAt = DateTime.UtcNow;
        await using var db = await _factory.CreateDbContextAsync();
        db.Projects.Add(p);
        await db.SaveChangesAsync();
        return p;
    }

    public async Task<Project> UpdateAsync(Project p)
    {
        await using var db = await _factory.CreateDbContextAsync();
        db.Projects.Update(p);
        await db.SaveChangesAsync();
        return p;
    }

    public async Task DeleteAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        await db.Projects.Where(p => p.Id == id).ExecuteDeleteAsync();
    }
}

public class MessageService : IMessageService
{
    /// <summary>Max messages accepted from one email address per hour.</summary>
    public const int PerSenderHourlyLimit = 3;
    /// <summary>Max messages accepted from everyone combined per hour.</summary>
    public const int GlobalHourlyLimit = 30;

    private readonly IDbContextFactory<AppDbContext> _factory;
    private readonly IMessageNotifier _notifier;
    private readonly ILogger<MessageService> _logger;

    public MessageService(IDbContextFactory<AppDbContext> factory, IMessageNotifier notifier, ILogger<MessageService> logger)
    {
        _factory = factory;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task<List<Message>> GetAllAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Messages.AsNoTracking().OrderByDescending(m => m.CreatedAt).ToListAsync();
    }

    public async Task<int> GetUnreadCountAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Messages.CountAsync(m => !m.IsRead);
    }

    public async Task<Message?> GetByIdAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        return await db.Messages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<Message> CreateAsync(Message m)
    {
        m.Email = m.Email.Trim();
        var since = DateTime.UtcNow.AddHours(-1);
        var email = m.Email.ToLower();

        await using (var db = await _factory.CreateDbContextAsync())
        {
            if (await db.Messages.CountAsync(x => x.CreatedAt >= since && x.Email.ToLower() == email) >= PerSenderHourlyLimit)
                throw new RateLimitExceededException("You've sent several messages recently. Please try again later.");
            if (await db.Messages.CountAsync(x => x.CreatedAt >= since) >= GlobalHourlyLimit)
                throw new RateLimitExceededException("The inbox is receiving a lot of messages right now. Please try again later.");

            m.Id = 0;
            m.IsRead = false;
            m.CreatedAt = DateTime.UtcNow;
            db.Messages.Add(m);
            await db.SaveChangesAsync();
        }

        // The message is already stored; a failed notification must not fail the request.
        try
        {
            await _notifier.NotifyNewMessageAsync(m);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification for message {MessageId}", m.Id);
        }
        return m;
    }

    public async Task MarkAsReadAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        await db.Messages.Where(m => m.Id == id).ExecuteUpdateAsync(s => s.SetProperty(m => m.IsRead, true));
    }

    public async Task DeleteAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();
        await db.Messages.Where(m => m.Id == id).ExecuteDeleteAsync();
    }
}
