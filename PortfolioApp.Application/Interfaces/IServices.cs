using PortfolioApp.Domain.Entities;

namespace PortfolioApp.Application.Interfaces;

public interface IUserProfileService
{
    Task<UserProfile?> GetProfileAsync();
    Task<UserProfile> UpsertProfileAsync(UserProfile profile);
}

public interface IQualificationService
{
    Task<List<Qualification>> GetAllAsync();
    Task<Qualification?> GetByIdAsync(int id);
    Task<Qualification> CreateAsync(Qualification qualification);
    Task<Qualification> UpdateAsync(Qualification qualification);
    Task DeleteAsync(int id);
}

public interface IExperienceService
{
    Task<List<Experience>> GetAllAsync();
    Task<Experience?> GetByIdAsync(int id);
    Task<Experience> CreateAsync(Experience experience);
    Task<Experience> UpdateAsync(Experience experience);
    Task DeleteAsync(int id);
}

public interface ISkillService
{
    Task<List<Skill>> GetAllAsync();
    Task<Dictionary<string, List<Skill>>> GetGroupedByCategory();
    Task<Skill?> GetByIdAsync(int id);
    Task<Skill> CreateAsync(Skill skill);
    Task<Skill> UpdateAsync(Skill skill);
    Task DeleteAsync(int id);
}

public interface IProjectService
{
    Task<List<Project>> GetAllAsync();
    Task<List<Project>> GetPagedAsync(int page, int pageSize, string? searchTerm = null);
    Task<int> GetTotalCountAsync(string? searchTerm = null);
    Task<Project?> GetByIdAsync(int id);
    Task<Project> CreateAsync(Project project);
    Task<Project> UpdateAsync(Project project);
    Task DeleteAsync(int id);
}

public interface IMessageService
{
    Task<List<Message>> GetAllAsync();
    Task<int> GetUnreadCountAsync();
    Task<Message?> GetByIdAsync(int id);

    /// <summary>
    /// Stores a contact-form message and notifies the site owner.
    /// Throws <see cref="RateLimitExceededException"/> when the sender or the site as a whole
    /// has sent too many messages recently.
    /// </summary>
    Task<Message> CreateAsync(Message message);
    Task MarkAsReadAsync(int id);
    Task DeleteAsync(int id);
}

/// <summary>Sends a notification to the site owner when a new contact message arrives.</summary>
public interface IMessageNotifier
{
    Task NotifyNewMessageAsync(Message message, CancellationToken cancellationToken = default);
}

public class RateLimitExceededException : Exception
{
    public RateLimitExceededException(string message) : base(message) { }
}
