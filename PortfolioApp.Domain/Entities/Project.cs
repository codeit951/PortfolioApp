using System.ComponentModel.DataAnnotations;

namespace PortfolioApp.Domain.Entities;

public class Project
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Project title is required")]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Project description is required")]
    public string Description { get; set; } = string.Empty;

    public string TechStack { get; set; } = string.Empty;
    public string GitHubUrl { get; set; } = string.Empty;
    public string DemoUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Project image is required")]
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
