using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PortfolioApp.Application.Interfaces;
using PortfolioApp.Domain.Entities;

namespace PortfolioApp.Infrastructure.Services;

public class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    /// <summary>Address the notification is sent from.</summary>
    public string From { get; set; } = string.Empty;
    /// <summary>Address that receives notifications. Defaults to the profile email when empty.</summary>
    public string To { get; set; } = string.Empty;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Host) && !string.IsNullOrWhiteSpace(From);
}

/// <summary>
/// Emails the site owner when a contact message arrives. Does nothing (beyond logging)
/// when the "Smtp" configuration section is not filled in.
/// </summary>
public class SmtpMessageNotifier : IMessageNotifier
{
    private readonly SmtpOptions _options;
    private readonly IUserProfileService _profiles;
    private readonly ILogger<SmtpMessageNotifier> _logger;

    public SmtpMessageNotifier(IOptions<SmtpOptions> options, IUserProfileService profiles, ILogger<SmtpMessageNotifier> logger)
    {
        _options = options.Value;
        _profiles = profiles;
        _logger = logger;
    }

    public async Task NotifyNewMessageAsync(Message message, CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured)
        {
            _logger.LogInformation("New contact message {MessageId} from {Email}; SMTP not configured, no email sent.", message.Id, message.Email);
            return;
        }

        var to = _options.To;
        if (string.IsNullOrWhiteSpace(to))
            to = (await _profiles.GetProfileAsync())?.Email ?? string.Empty;
        if (string.IsNullOrWhiteSpace(to))
        {
            _logger.LogWarning("SMTP is configured but no recipient is set (Smtp:To or profile email).");
            return;
        }

        using var mail = new MailMessage(_options.From, to)
        {
            Subject = $"New portfolio message: {(string.IsNullOrWhiteSpace(message.Subject) ? "(no subject)" : message.Subject)}",
            Body = $"From: {message.Name} <{message.Email}>\nReceived: {message.CreatedAt:u}\n\n{message.Content}",
            IsBodyHtml = false
        };
        mail.ReplyToList.Add(new MailAddress(message.Email, message.Name));

        using var client = new SmtpClient(_options.Host, _options.Port) { EnableSsl = _options.EnableSsl };
        if (!string.IsNullOrWhiteSpace(_options.UserName))
            client.Credentials = new NetworkCredential(_options.UserName, _options.Password);

        await client.SendMailAsync(mail, cancellationToken);
    }
}
