namespace PlayLeague.Api.Models;

public enum ScheduleStatus { DRAFT, PUBLISHED, ARCHIVED }

public class GameSchedule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? SeasonName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ScheduleStatus Status { get; set; } = ScheduleStatus.DRAFT;
    public bool RoundRobin { get; set; } = false;
    public int? Rounds { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? LeagueId { get; set; }
    public League? League { get; set; }

    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }

    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public ICollection<ScheduleGame> Games { get; set; } = [];
}

public class ScheduleGame
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int RoundNumber { get; set; }
    public int GameNumber { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid GameScheduleId { get; set; }
    public GameSchedule GameSchedule { get; set; } = null!;

    public Guid? EventId { get; set; }
    public Event? Event { get; set; }
}
