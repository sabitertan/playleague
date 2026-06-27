using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Teams.Queries;

public record TeamDto(
    Guid Id,
    string Name,
    string Sport,
    string? Season,
    string Role,
    string? LeagueName,
    string? DivisionName,
    int PlayerCount);

public record GetTeamsQuery(Guid UserId) : IRequest<List<TeamDto>>;

public class GetTeamsQueryHandler(AppDbContext db) : IRequestHandler<GetTeamsQuery, List<TeamDto>>
{
    public async Task<List<TeamDto>> Handle(GetTeamsQuery query, CancellationToken ct)
    {
        return await db.TeamMembers
            .Where(tm => tm.UserId == query.UserId)
            .Include(tm => tm.Team)
                .ThenInclude(t => t.League)
            .Include(tm => tm.Team)
                .ThenInclude(t => t.Division)
            .Select(tm => new TeamDto(
                tm.Team.Id,
                tm.Team.Name,
                tm.Team.Sport,
                tm.Team.Season,
                tm.Role.ToString(),
                tm.Team.League != null ? tm.Team.League.Name : null,
                tm.Team.Division != null ? tm.Team.Division.Name : null,
                tm.Team.Players.Count))
            .ToListAsync(ct);
    }
}
