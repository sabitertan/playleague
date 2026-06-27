namespace PlayLeague.Api.Models;

public enum LeagueRole { LEAGUE_ADMIN, TEAM_ADMIN, MEMBER }

public class LeagueUser
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid LeagueId { get; set; }
    public League League { get; set; } = null!;

    public LeagueRole Role { get; set; } = LeagueRole.MEMBER;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
