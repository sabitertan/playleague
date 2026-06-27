using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Venues.Queries;

public record SurfaceDto(
    Guid Id,
    string Name,
    string? SurfaceType,
    int? Capacity,
    bool IsDefault
);

public record VenueDetailDto(
    Guid Id,
    string Name,
    string? City,
    string? State,
    VenueVisibility Visibility,
    VenueProfileStatus ProfileStatus,
    int SurfaceCount,
    string? Address,
    string? ZipCode,
    string? Phone,
    string? Website,
    string? Notes,
    List<SurfaceDto> Surfaces
);

public record GetVenueByIdQuery(Guid VenueId, Guid UserId) : IRequest<VenueDetailDto>;

public class GetVenueByIdQueryHandler(AppDbContext db) : IRequestHandler<GetVenueByIdQuery, VenueDetailDto>
{
    public async Task<VenueDetailDto> Handle(GetVenueByIdQuery query, CancellationToken ct)
    {
        var venue = await db.Venues
            .Include(v => v.Surfaces)
            .FirstOrDefaultAsync(v => v.Id == query.VenueId, ct);

        if (venue is null)
            throw new NotFoundException("Venue not found.");

        // Verify the user has access via team or league membership
        bool hasAccess = false;

        if (venue.TeamId.HasValue)
        {
            hasAccess = await db.TeamMembers
                .AnyAsync(tm => tm.TeamId == venue.TeamId && tm.UserId == query.UserId, ct);
        }

        if (!hasAccess && venue.LeagueId.HasValue)
        {
            hasAccess = await db.LeagueUsers
                .AnyAsync(lu => lu.LeagueId == venue.LeagueId && lu.UserId == query.UserId, ct);
        }

        // Venue creator always has access
        if (!hasAccess && venue.CreatedById == query.UserId)
            hasAccess = true;

        if (!hasAccess)
            throw new ForbiddenException("You do not have access to this venue.");

        var surfaces = venue.Surfaces
            .Where(s => s.IsActive)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new SurfaceDto(s.Id, s.Name, s.SurfaceType, s.Capacity, s.IsDefault))
            .ToList();

        return new VenueDetailDto(
            venue.Id,
            venue.Name,
            venue.City,
            venue.State,
            venue.Visibility,
            venue.ProfileStatus,
            surfaces.Count,
            venue.Address,
            venue.ZipCode,
            venue.Phone,
            venue.Website,
            venue.Notes,
            surfaces
        );
    }
}
