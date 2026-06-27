using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Venues.Commands;

public record UpdateSurfaceCommand(
    Guid SurfaceId,
    string Name,
    string? SurfaceType,
    int? Capacity,
    bool IsDefault,
    string? Notes,
    Guid UserId) : IRequest;

public class UpdateSurfaceCommandHandler(AppDbContext db) : IRequestHandler<UpdateSurfaceCommand>
{
    public async Task Handle(UpdateSurfaceCommand cmd, CancellationToken ct)
    {
        var surface = await db.IceSurfaces
            .Include(s => s.Venue)
            .FirstOrDefaultAsync(s => s.Id == cmd.SurfaceId, ct);

        if (surface is null)
            throw new NotFoundException("Surface not found.");

        if (surface.Venue.CreatedById != cmd.UserId)
            throw new ForbiddenException("Only the venue creator can update surfaces.");

        if (cmd.IsDefault && !surface.IsDefault)
        {
            var others = await db.IceSurfaces
                .Where(s => s.VenueId == surface.VenueId && s.IsDefault)
                .ToListAsync(ct);
            foreach (var s in others)
                s.IsDefault = false;
        }

        surface.Name = cmd.Name;
        surface.SurfaceType = cmd.SurfaceType;
        surface.Capacity = cmd.Capacity;
        surface.IsDefault = cmd.IsDefault;
        surface.Notes = cmd.Notes;

        await db.SaveChangesAsync(ct);
    }
}
