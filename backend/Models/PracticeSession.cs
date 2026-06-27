namespace PlayLeague.Api.Models;

public class PracticeSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Title { get; set; }
    public DateTime Date { get; set; }
    public int DurationMinutes { get; set; }
    public bool IsShared { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public ICollection<PracticeSessionPlay> SessionPlays { get; set; } = [];
}

public class Play
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Thumbnail { get; set; }
    public string? PlayData { get; set; }
    public bool IsTemplate { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }

    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public ICollection<PracticeSessionPlay> SessionPlays { get; set; } = [];
}

public class PracticeSessionPlay
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Sequence { get; set; }
    public int DurationMinutes { get; set; }
    public string? Instructions { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid SessionId { get; set; }
    public PracticeSession Session { get; set; } = null!;

    public Guid PlayId { get; set; }
    public Play Play { get; set; } = null!;
}
