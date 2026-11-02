using System.ComponentModel.DataAnnotations;

namespace PortfolioApp.Domain.Entities;

public class Project
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Project title is required")]
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Project description is required")]
    [StringLength(3000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(1000)]
    public string TechStack { get; set; } = string.Empty;

    [RegularExpression(@"^https?://\S+$", ErrorMessage = "Enter a valid URL starting with http:// or https://")]
    public string GitHubUrl { get; set; } = string.Empty;

    [RegularExpression(@"^https?://\S+$", ErrorMessage = "Enter a valid URL starting with http:// or https://")]
    public string DemoUrl { get; set; } = string.Empty;

    /// <summary>Optional preview image. A placeholder is shown when empty.</summary>
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
