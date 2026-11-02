using System.ComponentModel.DataAnnotations;

namespace PortfolioApp.Domain.Entities;

public class Skill
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Skill name is required")]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [Range(0, 100)]
    public int Level { get; set; } // 0–100

    [StringLength(100)]
    public string IconClass { get; set; } = string.Empty;
}
