namespace PlayLeague.Api.Models;

public class Division
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? AgeGroup { get; set; }
    public string? SkillLevel { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid LeagueId { get; set; }
    public League League { get; set; } = null!;

    public ICollection<Team> Teams { get; set; } = [];
}
