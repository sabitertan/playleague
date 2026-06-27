namespace PlayLeague.Api.Models;

public class Coach
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? Title { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Owner of this coach record (the pool the coach belongs to)
    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public ICollection<CoachAssignment> Assignments { get; set; } = [];
}
