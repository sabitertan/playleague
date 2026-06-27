namespace PlayLeague.Api.Models;

public enum RsvpStatus { GOING, NOT_GOING, MAYBE, NO_RESPONSE }

public class Rsvp
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;

    public RsvpStatus Status { get; set; } = RsvpStatus.NO_RESPONSE;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
