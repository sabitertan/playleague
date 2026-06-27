namespace PlayLeague.Api.Models;

public class Player
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int? JerseyNumber { get; set; }
    public string? Position { get; set; }
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public Guid? UserId { get; set; }
    public User? User { get; set; }
}
