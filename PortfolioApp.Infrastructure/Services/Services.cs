using Microsoft.EntityFrameworkCore;
using PortfolioApp.Application.Interfaces;
using PortfolioApp.Domain.Entities;
using PortfolioApp.Infrastructure.Data;

namespace PortfolioApp.Infrastructure.Services;

public class UserProfileService : IUserProfileService
{
    private readonly AppDbContext _db;
    public UserProfileService(AppDbContext db) => _db = db;

    public async Task<UserProfile?> GetProfileAsync() =>
        await _db.UserProfiles.AsNoTracking().FirstOrDefaultAsync();

    public async Task<UserProfile> UpsertProfileAsync(UserProfile profile)
    {
        var existing = await _db.UserProfiles.FirstOrDefaultAsync();
        if (existing is null)
        {
            _db.UserProfiles.Add(profile);
        }
        else
        {
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
            profile = existing;
        }
        await _db.SaveChangesAsync();
        return profile;
    }
}

public class QualificationService : IQualificationService
{
    private readonly AppDbContext _db;
    public QualificationService(AppDbContext db) => _db = db;

    public async Task<List<Qualification>> GetAllAsync() =>
        await _db.Qualifications.AsNoTracking().OrderByDescending(q => q.Year).ToListAsync();

    public async Task<Qualification?> GetByIdAsync(int id) =>
        await _db.Qualifications.AsNoTracking().FirstOrDefaultAsync(q => q.Id == id);

    public async Task<Qualification> CreateAsync(Qualification q)
    {
        _db.Qualifications.Add(q);
        await _db.SaveChangesAsync();
        return q;
    }

    public async Task<Qualification> UpdateAsync(Qualification q)
    {
        _db.Qualifications.Update(q);
        await _db.SaveChangesAsync();
        return q;
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _db.Qualifications.FindAsync(id);
        if (item is not null) { _db.Qualifications.Remove(item); await _db.SaveChangesAsync(); }
    }
}

public class ExperienceService : IExperienceService
{
    private readonly AppDbContext _db;
    public ExperienceService(AppDbContext db) => _db = db;

    public async Task<List<Experience>> GetAllAsync() =>
        await _db.Experiences.AsNoTracking().OrderByDescending(e => e.StartDate).ToListAsync();

    public async Task<Experience?> GetByIdAsync(int id) =>
        await _db.Experiences.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);

    public async Task<Experience> CreateAsync(Experience e)
    {
        _db.Experiences.Add(e);
        await _db.SaveChangesAsync();
        return e;
    }

    public async Task<Experience> UpdateAsync(Experience e)
    {
        _db.Experiences.Update(e);
        await _db.SaveChangesAsync();
        return e;
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _db.Experiences.FindAsync(id);
        if (item is not null) { _db.Experiences.Remove(item); await _db.SaveChangesAsync(); }
    }
}

public class SkillService : ISkillService
{
    private readonly AppDbContext _db;
    public SkillService(AppDbContext db) => _db = db;

    public async Task<List<Skill>> GetAllAsync() =>
        await _db.Skills.AsNoTracking().OrderBy(s => s.Category).ThenByDescending(s => s.Level).ToListAsync();

    public async Task<Dictionary<string, List<Skill>>> GetGroupedByCategory()
    {
        var skills = await GetAllAsync();
        return skills.GroupBy(s => s.Category).ToDictionary(g => g.Key, g => g.ToList());
    }

    public async Task<Skill?> GetByIdAsync(int id) => await _db.Skills.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

    public async Task<Skill> CreateAsync(Skill s)
    {
        _db.Skills.Add(s);
        await _db.SaveChangesAsync();
        return s;
    }

    public async Task<Skill> UpdateAsync(Skill s)
    {
        _db.Skills.Update(s);
        await _db.SaveChangesAsync();
        return s;
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _db.Skills.FindAsync(id);
        if (item is not null) { _db.Skills.Remove(item); await _db.SaveChangesAsync(); }
    }
}

public class ProjectService : IProjectService
{
    private readonly AppDbContext _db;
    public ProjectService(AppDbContext db) => _db = db;

    public async Task<List<Project>> GetAllAsync() =>
        await _db.Projects.AsNoTracking().OrderByDescending(p => p.CreatedAt).ToListAsync();

    public async Task<List<Project>> GetPagedAsync(int page, int pageSize, string? searchTerm = null)
    {
        var query = _db.Projects.AsQueryable();
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(p => p.Title.Contains(searchTerm) || p.Description.Contains(searchTerm) || p.TechStack.Contains(searchTerm));
        return await query.AsNoTracking().OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(string? searchTerm = null)
    {
        var query = _db.Projects.AsQueryable();
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(p => p.Title.Contains(searchTerm) || p.Description.Contains(searchTerm) || p.TechStack.Contains(searchTerm));
        return await query.CountAsync();
    }

    public async Task<Project?> GetByIdAsync(int id) => await _db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Project> CreateAsync(Project p)
    {
        p.CreatedAt = DateTime.UtcNow;
        _db.Projects.Add(p);
        await _db.SaveChangesAsync();
        return p;
    }

    public async Task<Project> UpdateAsync(Project p)
    {
        _db.Projects.Update(p);
        await _db.SaveChangesAsync();
        return p;
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _db.Projects.FindAsync(id);
        if (item is not null) { _db.Projects.Remove(item); await _db.SaveChangesAsync(); }
    }
}

public class MessageService : IMessageService
{
    private readonly AppDbContext _db;
    public MessageService(AppDbContext db) => _db = db;

    public async Task<List<Message>> GetAllAsync() =>
        await _db.Messages.AsNoTracking().OrderByDescending(m => m.CreatedAt).ToListAsync();

    public async Task<int> GetUnreadCountAsync() =>
        await _db.Messages.CountAsync(m => !m.IsRead);

    public async Task<Message?> GetByIdAsync(int id) => await _db.Messages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);

    public async Task<Message> CreateAsync(Message m)
    {
        m.CreatedAt = DateTime.UtcNow;
        _db.Messages.Add(m);
        await _db.SaveChangesAsync();
        return m;
    }

    public async Task MarkAsReadAsync(int id)
    {
        var item = await _db.Messages.FindAsync(id);
        if (item is not null) { item.IsRead = true; await _db.SaveChangesAsync(); }
    }

    public async Task DeleteAsync(int id)
    {
        var item = await _db.Messages.FindAsync(id);
        if (item is not null) { _db.Messages.Remove(item); await _db.SaveChangesAsync(); }
    }
}
