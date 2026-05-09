namespace PortfolioApp.Domain.Entities;

public class Qualification
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Institution { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;
}
