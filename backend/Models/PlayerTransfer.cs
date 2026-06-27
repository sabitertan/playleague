namespace PlayLeague.Api.Models;

public class PlayerTransfer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid PlayerId { get; set; }
    public Player Player { get; set; } = null!;

    public Guid FromTeamId { get; set; }
    public Team FromTeam { get; set; } = null!;

    public Guid ToTeamId { get; set; }
    public Team ToTeam { get; set; } = null!;

    public Guid LeagueId { get; set; }
    public League League { get; set; } = null!;

    public Guid TransferredById { get; set; }
    public User TransferredBy { get; set; } = null!;
}
