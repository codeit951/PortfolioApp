using Microsoft.Extensions.Logging.Abstractions;
using PortfolioApp.Application.Interfaces;
using PortfolioApp.Domain.Entities;
using PortfolioApp.Infrastructure.Services;

namespace PortfolioApp.Tests;

public class ServiceTests : IDisposable
{
    private readonly TestDb _db = new();

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task Project_can_be_updated_repeatedly_with_fresh_instances()
    {
        // Regression: with a circuit-scoped DbContext the second Update() of the same key threw
        // "another instance with the same key value is already being tracked".
        var service = new ProjectService(_db);
        var created = await service.CreateAsync(new Project { Title = "A", Description = "d" });

        await service.UpdateAsync(new Project { Id = created.Id, Title = "B", Description = "d" });
        await service.UpdateAsync(new Project { Id = created.Id, Title = "C", Description = "d" });

        Assert.Equal("C", (await service.GetByIdAsync(created.Id))!.Title);
    }

    [Fact]
    public async Task Project_search_is_case_insensitive_and_paged()
    {
        var service = new ProjectService(_db);
        await service.CreateAsync(new Project { Title = "Blazor Portfolio", Description = "x", TechStack = ".NET" });
        await service.CreateAsync(new Project { Title = "Other", Description = "uses BLAZOR too" });
        await service.CreateAsync(new Project { Title = "Unrelated", Description = "nothing" });

        Assert.Equal(2, await service.GetTotalCountAsync("blazor"));
        Assert.Single(await service.GetPagedAsync(2, 1, "BlAzOr"));
        Assert.Equal(3, await service.GetTotalCountAsync(null));
    }

    [Fact]
    public async Task Current_experience_has_no_end_date()
    {
        var service = new ExperienceService(_db);
        var exp = await service.CreateAsync(new Experience
        {
            Company = "Co", Role = "Dev", StartDate = new DateTime(2020, 1, 1),
            EndDate = new DateTime(2021, 1, 1), IsCurrent = true
        });

        Assert.Null((await service.GetByIdAsync(exp.Id))!.EndDate);
    }

    [Fact]
    public async Task Skill_level_is_clamped()
    {
        var service = new SkillService(_db);
        var skill = await service.CreateAsync(new Skill { Name = "C#", Category = "Languages", Level = 150 });
        Assert.Equal(100, skill.Level);
    }

    [Fact]
    public async Task Messages_are_rate_limited_per_sender_and_notify_owner()
    {
        var notifier = new RecordingNotifier();
        var service = new MessageService(_db, notifier, NullLogger<MessageService>.Instance);

        for (var i = 0; i < MessageService.PerSenderHourlyLimit; i++)
            await service.CreateAsync(NewMessage("spam@example.com"));

        await Assert.ThrowsAsync<RateLimitExceededException>(() => service.CreateAsync(NewMessage("SPAM@example.com")));

        // A different sender is still accepted.
        await service.CreateAsync(NewMessage("someone@example.com"));
        Assert.Equal(MessageService.PerSenderHourlyLimit + 1, notifier.Count);
        Assert.Equal(MessageService.PerSenderHourlyLimit + 1, await service.GetUnreadCountAsync());
    }

    [Fact]
    public async Task Message_is_saved_even_if_notification_fails()
    {
        var service = new MessageService(_db, new ThrowingNotifier(), NullLogger<MessageService>.Instance);
        await service.CreateAsync(NewMessage("a@example.com"));
        Assert.Single(await service.GetAllAsync());
    }

    [Fact]
    public async Task Mark_as_read_and_delete_message()
    {
        var service = new MessageService(_db, new RecordingNotifier(), NullLogger<MessageService>.Instance);
        var m = await service.CreateAsync(NewMessage("a@example.com"));

        await service.MarkAsReadAsync(m.Id);
        Assert.Equal(0, await service.GetUnreadCountAsync());

        await service.DeleteAsync(m.Id);
        Assert.Empty(await service.GetAllAsync());
    }

    [Fact]
    public async Task Profile_upsert_creates_then_updates_single_row()
    {
        var service = new UserProfileService(_db);
        await service.UpsertProfileAsync(new UserProfile { FullName = "First" });
        await service.UpsertProfileAsync(new UserProfile { FullName = "Second" });

        Assert.Equal("Second", (await service.GetProfileAsync())!.FullName);
        using var ctx = _db.CreateDbContext();
        Assert.Equal(1, ctx.UserProfiles.Count());
    }

    private static Message NewMessage(string email) =>
        new() { Name = "Test", Email = email, Content = "Hello there, this is a test." };

    private sealed class RecordingNotifier : IMessageNotifier
    {
        public int Count { get; private set; }
        public Task NotifyNewMessageAsync(Message message, CancellationToken ct = default) { Count++; return Task.CompletedTask; }
    }

    private sealed class ThrowingNotifier : IMessageNotifier
    {
        public Task NotifyNewMessageAsync(Message message, CancellationToken ct = default) => throw new InvalidOperationException("SMTP down");
    }
}
