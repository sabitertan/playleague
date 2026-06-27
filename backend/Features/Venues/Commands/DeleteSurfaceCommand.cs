using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Venues.Commands;

public record DeleteSurfaceCommand(Guid SurfaceId, Guid UserId) : IRequest;

public class DeleteSurfaceCommandHandler(AppDbContext db) : IRequestHandler<DeleteSurfaceCommand>
{
    public async Task Handle(DeleteSurfaceCommand cmd, CancellationToken ct)
    {
        var surface = await db.IceSurfaces
            .Include(s => s.Venue)
            .FirstOrDefaultAsync(s => s.Id == cmd.SurfaceId, ct);

        if (surface is null)
            throw new NotFoundException("Surface not found.");

        if (surface.Venue.CreatedById != cmd.UserId)
            throw new ForbiddenException("Only the venue creator can delete surfaces.");

        db.IceSurfaces.Remove(surface);
        await db.SaveChangesAsync(ct);
    }
}
