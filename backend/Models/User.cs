namespace PlayLeague.Api.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public string? Name { get; set; }
    public bool Approved { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<TeamMember> TeamMembers { get; set; } = [];
    public ICollection<LeagueUser> LeagueUsers { get; set; } = [];
    public ICollection<Rsvp> Rsvps { get; set; } = [];
    public ICollection<NotificationPreference> NotificationPreferences { get; set; } = [];
}
