using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Venues.Queries;

public record GetSurfaceByIdQuery(Guid SurfaceId, Guid UserId) : IRequest<SurfaceDto>;

public class GetSurfaceByIdQueryHandler(AppDbContext db) : IRequestHandler<GetSurfaceByIdQuery, SurfaceDto>
{
    public async Task<SurfaceDto> Handle(GetSurfaceByIdQuery query, CancellationToken ct)
    {
        var surface = await db.IceSurfaces
            .Include(s => s.Venue)
            .FirstOrDefaultAsync(s => s.Id == query.SurfaceId, ct);

        if (surface is null)
            throw new NotFoundException("Surface not found.");

        var venue = surface.Venue;
        bool hasAccess = venue.CreatedById == query.UserId;

        if (!hasAccess && venue.TeamId.HasValue)
            hasAccess = await db.TeamMembers.AnyAsync(tm => tm.TeamId == venue.TeamId && tm.UserId == query.UserId, ct);

        if (!hasAccess && venue.LeagueId.HasValue)
            hasAccess = await db.LeagueUsers.AnyAsync(lu => lu.LeagueId == venue.LeagueId && lu.UserId == query.UserId, ct);

        if (!hasAccess)
            throw new ForbiddenException("You do not have access to this surface.");

        return new SurfaceDto(surface.Id, surface.Name, surface.SurfaceType, surface.Capacity, surface.IsDefault);
    }
}
