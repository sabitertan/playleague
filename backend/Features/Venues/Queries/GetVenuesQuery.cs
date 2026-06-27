using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Venues.Queries;

public record VenueDto(
    Guid Id,
    string Name,
    string? City,
    string? State,
    VenueVisibility Visibility,
    VenueProfileStatus ProfileStatus,
    int SurfaceCount
);

public record GetVenuesQuery(Guid? TeamId, Guid? LeagueId, Guid UserId) : IRequest<List<VenueDto>>;

public class GetVenuesQueryHandler(AppDbContext db) : IRequestHandler<GetVenuesQuery, List<VenueDto>>
{
    public async Task<List<VenueDto>> Handle(GetVenuesQuery query, CancellationToken ct)
    {
        var q = db.Venues
            .Include(v => v.Surfaces)
            .AsQueryable();

        if (query.TeamId.HasValue)
        {
            var isMember = await db.TeamMembers
                .AnyAsync(tm => tm.TeamId == query.TeamId && tm.UserId == query.UserId, ct);

            if (!isMember)
                throw new Exceptions.ForbiddenException("You are not a member of this team.");

            q = q.Where(v => v.TeamId == query.TeamId);
        }
        else if (query.LeagueId.HasValue)
        {
            var isLeagueMember = await db.LeagueUsers
                .AnyAsync(lu => lu.LeagueId == query.LeagueId && lu.UserId == query.UserId, ct);

            if (!isLeagueMember)
                throw new Exceptions.ForbiddenException("You are not a member of this league.");

            q = q.Where(v => v.LeagueId == query.LeagueId);
        }
        else
        {
            // No filter: return venues the user can see (via team or league membership)
            var userTeamIds = await db.TeamMembers
                .Where(tm => tm.UserId == query.UserId)
                .Select(tm => tm.TeamId)
                .ToListAsync(ct);

            var userLeagueIds = await db.LeagueUsers
                .Where(lu => lu.UserId == query.UserId)
                .Select(lu => lu.LeagueId)
                .ToListAsync(ct);

            q = q.Where(v =>
                (v.TeamId != null && userTeamIds.Contains(v.TeamId.Value)) ||
                (v.LeagueId != null && userLeagueIds.Contains(v.LeagueId.Value)));
        }

        var venues = await q
            .OrderBy(v => v.Name)
            .ToListAsync(ct);

        return venues.Select(v => new VenueDto(
            v.Id,
            v.Name,
            v.City,
            v.State,
            v.Visibility,
            v.ProfileStatus,
            v.Surfaces.Count(s => s.IsActive)
        )).ToList();
    }
}
