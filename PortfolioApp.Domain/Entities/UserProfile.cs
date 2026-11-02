using System.ComponentModel.DataAnnotations;

namespace PortfolioApp.Domain.Entities;

public class UserProfile
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Bio { get; set; } = string.Empty;

    public string ProfileImageUrl { get; set; } = string.Empty;

    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Enter a valid email")]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [StringLength(50)]
    public string Phone { get; set; } = string.Empty;

    [StringLength(200)]
    public string Location { get; set; } = string.Empty;

    [RegularExpression(@"^https?://\S+$", ErrorMessage = "Enter a valid URL starting with http:// or https://")]
    public string GitHubUrl { get; set; } = string.Empty;

    [RegularExpression(@"^https?://\S+$", ErrorMessage = "Enter a valid URL starting with http:// or https://")]
    public string LinkedInUrl { get; set; } = string.Empty;

    [RegularExpression(@"^https?://\S+$", ErrorMessage = "Enter a valid URL starting with http:// or https://")]
    public string TwitterUrl { get; set; } = string.Empty;

    public string CvFileUrl { get; set; } = string.Empty;

    [StringLength(2000)]
    public string Summary { get; set; } = string.Empty;

    /// <summary>First name, used for the site brand.</summary>
    public string FirstName => FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
}
