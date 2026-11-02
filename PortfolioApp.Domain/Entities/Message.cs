using System.ComponentModel.DataAnnotations;

namespace PortfolioApp.Domain.Entities;

public class Message
{
    public int Id { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(300)]
    public string Subject { get; set; } = string.Empty;

    [Required, StringLength(3000)]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
}
