namespace PortfolioApp.Domain.Entities;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Level { get; set; } // 1–100
    public string IconClass { get; set; } = string.Empty;
}
