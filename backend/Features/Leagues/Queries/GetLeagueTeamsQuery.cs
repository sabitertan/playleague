using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Leagues.Queries;

public record LeagueTeamDto(
    Guid Id,
    string Name,
    string Sport,
    string? Season,
    string? DivisionName,
    int PlayerCount,
    bool IsActive);

public record GetLeagueTeamsQuery(Guid LeagueId, Guid UserId) : IRequest<List<LeagueTeamDto>>;

public class GetLeagueTeamsQueryHandler(AppDbContext db) : IRequestHandler<GetLeagueTeamsQuery, List<LeagueTeamDto>>
{
    public async Task<List<LeagueTeamDto>> Handle(GetLeagueTeamsQuery query, CancellationToken ct)
    {
        var isMember = await db.LeagueUsers
            .AnyAsync(lu => lu.LeagueId == query.LeagueId && lu.UserId == query.UserId, ct);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this league.");

        var leagueExists = await db.Leagues.AnyAsync(l => l.Id == query.LeagueId, ct);
        if (!leagueExists)
            throw new NotFoundException("League not found.");

        var teams = await db.Teams
            .Where(t => t.LeagueId == query.LeagueId)
            .Include(t => t.Division)
            .Include(t => t.Players)
            .OrderBy(t => t.Name)
            .ToListAsync(ct);

        return teams.Select(t => new LeagueTeamDto(
            t.Id,
            t.Name,
            t.Sport,
            t.Season,
            t.Division?.Name,
            t.Players.Count,
            t.IsActive))
            .ToList();
    }
}
