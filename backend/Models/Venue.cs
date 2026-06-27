namespace PlayLeague.Api.Models;

public enum VenueVisibility { PUBLIC, LEAGUE, TEAM }
public enum VenueProfileStatus { DRAFT, PUBLISHED, ARCHIVED }

public class Venue
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public string? Slug { get; set; }
    public string? PublicDescription { get; set; }
    public string? LogoUrl { get; set; }
    public string? Timezone { get; set; }
    public VenueVisibility Visibility { get; set; } = VenueVisibility.TEAM;
    public VenueProfileStatus ProfileStatus { get; set; } = VenueProfileStatus.DRAFT;
    public bool IsActive { get; set; } = true;
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }

    public Guid? LeagueId { get; set; }
    public League? League { get; set; }

    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = null!;

    public ICollection<IceSurface> Surfaces { get; set; } = [];
    public ICollection<Event> Events { get; set; } = [];
}
