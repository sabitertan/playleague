namespace PlayLeague.Api.Models;

public enum TeamRole { ADMIN, MEMBER }

public class TeamMember
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public TeamRole Role { get; set; } = TeamRole.MEMBER;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
