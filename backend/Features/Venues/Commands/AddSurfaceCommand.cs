using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Venues.Commands;

public record AddSurfaceCommand(
    Guid VenueId,
    string Name,
    string? SurfaceType,
    int? Capacity,
    bool IsDefault,
    Guid UserId
) : IRequest<Guid>;

public class AddSurfaceCommandHandler(AppDbContext db) : IRequestHandler<AddSurfaceCommand, Guid>
{
    public async Task<Guid> Handle(AddSurfaceCommand cmd, CancellationToken ct)
    {
        var venue = await db.Venues
            .FirstOrDefaultAsync(v => v.Id == cmd.VenueId, ct);

        if (venue is null)
            throw new NotFoundException("Venue not found.");

        if (venue.CreatedById != cmd.UserId)
            throw new ForbiddenException("Only the venue creator can add surfaces.");

        if (cmd.IsDefault)
        {
            // Clear existing default surfaces for this venue
            var existingSurfaces = await db.IceSurfaces
                .Where(s => s.VenueId == cmd.VenueId && s.IsDefault)
                .ToListAsync(ct);

            foreach (var surface in existingSurfaces)
                surface.IsDefault = false;
        }

        var newSurface = new IceSurface
        {
            VenueId = cmd.VenueId,
            Name = cmd.Name,
            SurfaceType = cmd.SurfaceType,
            Capacity = cmd.Capacity,
            IsDefault = cmd.IsDefault,
        };

        db.IceSurfaces.Add(newSurface);
        await db.SaveChangesAsync(ct);

        return newSurface.Id;
    }
}
