namespace PlayLeague.Api.Models;

public enum AuditSeverity { LOW, MEDIUM, HIGH, CRITICAL }

public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Action { get; set; }
    public string? ResourceType { get; set; }
    public string? ResourceId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public AuditSeverity Severity { get; set; } = AuditSeverity.LOW;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public Guid? LeagueId { get; set; }
    public League? League { get; set; }

    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }
}
