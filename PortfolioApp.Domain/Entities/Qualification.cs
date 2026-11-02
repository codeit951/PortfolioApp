using System.ComponentModel.DataAnnotations;

namespace PortfolioApp.Domain.Entities;

public class Qualification
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Institution is required")]
    [StringLength(300)]
    public string Institution { get; set; } = string.Empty;

    [Range(1950, 2100, ErrorMessage = "Enter a valid year")]
    public int Year { get; set; }

    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(200)]
    public string Grade { get; set; } = string.Empty;
}
