using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Leagues.Queries;

public record DivisionDto(
    Guid Id,
    string Name,
    string? AgeGroup,
    string? SkillLevel,
    int TeamCount);

public record LeagueDetailDto(
    Guid Id,
    string Name,
    string Sport,
    string? ContactEmail,
    int TeamCount,
    int MemberCount,
    List<DivisionDto> Divisions);

public record GetLeagueQuery(Guid LeagueId, Guid UserId) : IRequest<LeagueDetailDto>;

public class GetLeagueQueryHandler(AppDbContext db) : IRequestHandler<GetLeagueQuery, LeagueDetailDto>
{
    public async Task<LeagueDetailDto> Handle(GetLeagueQuery query, CancellationToken ct)
    {
        var isMember = await db.LeagueUsers
            .AnyAsync(lu => lu.LeagueId == query.LeagueId && lu.UserId == query.UserId, ct);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this league.");

        var league = await db.Leagues
            .Include(l => l.Teams)
            .Include(l => l.LeagueUsers)
            .Include(l => l.Divisions)
                .ThenInclude(d => d.Teams)
            .FirstOrDefaultAsync(l => l.Id == query.LeagueId, ct);

        if (league is null)
            throw new NotFoundException("League not found.");

        var divisions = league.Divisions
            .Select(d => new DivisionDto(
                d.Id,
                d.Name,
                d.AgeGroup,
                d.SkillLevel,
                d.Teams.Count))
            .ToList();

        return new LeagueDetailDto(
            league.Id,
            league.Name,
            league.Sport,
            league.ContactEmail,
            league.Teams.Count,
            league.LeagueUsers.Count,
            divisions);
    }
}
