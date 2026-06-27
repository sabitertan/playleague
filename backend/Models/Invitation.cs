namespace PlayLeague.Api.Models;

public enum InvitationStatus { PENDING, ACCEPTED, EXPIRED }

public class Invitation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Email { get; set; }
    public required string Token { get; set; } = Guid.NewGuid().ToString("N");
    public InvitationStatus Status { get; set; } = InvitationStatus.PENDING;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid TeamId { get; set; }
    public Team Team { get; set; } = null!;

    public Guid InvitedById { get; set; }
    public User InvitedBy { get; set; } = null!;
}
