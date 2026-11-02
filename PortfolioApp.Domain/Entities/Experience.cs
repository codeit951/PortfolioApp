using System.ComponentModel.DataAnnotations;

namespace PortfolioApp.Domain.Entities;

public class Experience : IValidatableObject
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Company is required")]
    [StringLength(300)]
    public string Company { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required")]
    [StringLength(300)]
    public string Role { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [StringLength(3000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(300)]
    public string Location { get; set; } = string.Empty;

    public bool IsCurrent { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!IsCurrent && EndDate is null)
            yield return new ValidationResult("End date is required unless this is your current role.", new[] { nameof(EndDate) });
        if (!IsCurrent && EndDate is not null && EndDate < StartDate)
            yield return new ValidationResult("End date cannot be before the start date.", new[] { nameof(EndDate) });
    }
}
