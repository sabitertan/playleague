namespace PlayLeague.Api.Models;

public enum EventType { GAME, PRACTICE, OTHER }

public class Event
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Title { get; set; }
    public EventType Type { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public string? Location { get; set; }
    public string? Opponent { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public Guid? VenueId { get; set; }
    public Venue? Venue { get; set; }

    public Guid? LeagueId { get; set; }
    public League? League { get; set; }

    public Guid? HomeTeamId { get; set; }
    public Team? HomeTeam { get; set; }

    public Guid? AwayTeamId { get; set; }
    public Team? AwayTeam { get; set; }

    public ICollection<Rsvp> Rsvps { get; set; } = [];
}
