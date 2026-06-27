namespace PlayLeague.Api.Models;

public class Team
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string Sport { get; set; }
    public string? Season { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid? LeagueId { get; set; }
    public League? League { get; set; }

    public Guid? DivisionId { get; set; }
    public Division? Division { get; set; }

    public ICollection<TeamMember> TeamMembers { get; set; } = [];
    public ICollection<Player> Players { get; set; } = [];
    public ICollection<Event> Events { get; set; } = [];
    public ICollection<Invitation> Invitations { get; set; } = [];
    public ICollection<Venue> Venues { get; set; } = [];
    public ICollection<PracticeSession> PracticeSessions { get; set; } = [];
}
