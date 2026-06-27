namespace PlayLeague.Api.Models;

public class NotificationPreference
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public bool LeagueMessages { get; set; } = true;
    public bool LeagueAnnouncements { get; set; } = true;
    public bool EventNotifications { get; set; } = true;
    public bool RsvpReminders { get; set; } = true;
    public bool TeamInvitations { get; set; } = true;
    public bool PracticePlanNotifications { get; set; } = true;
    public bool EmailEnabled { get; set; } = true;
    public bool UrgentOnly { get; set; } = false;
    public bool BatchDelivery { get; set; } = false;
    public required string UnsubscribeToken { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid? LeagueId { get; set; }
    public League? League { get; set; }
}
