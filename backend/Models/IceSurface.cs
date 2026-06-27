namespace PlayLeague.Api.Models;

public class IceSurface
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string? SurfaceType { get; set; }
    public int? Capacity { get; set; }
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid VenueId { get; set; }
    public Venue Venue { get; set; } = null!;
}
