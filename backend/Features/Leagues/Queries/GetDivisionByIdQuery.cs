using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Leagues.Queries;

public record DivisionDetailDto(
    Guid Id,
    string Name,
    string? AgeGroup,
    string? SkillLevel,
    bool IsActive,
    List<LeagueTeamDto> Teams);

public record GetDivisionByIdQuery(Guid DivisionId, Guid UserId) : IRequest<DivisionDetailDto>;

public class GetDivisionByIdQueryHandler(AppDbContext db) : IRequestHandler<GetDivisionByIdQuery, DivisionDetailDto>
{
    public async Task<DivisionDetailDto> Handle(GetDivisionByIdQuery query, CancellationToken ct)
    {
        var division = await db.Divisions
            .Include(d => d.Teams)
                .ThenInclude(t => t.Players)
            .FirstOrDefaultAsync(d => d.Id == query.DivisionId, ct);

        if (division is null)
            throw new NotFoundException("Division not found.");

        var isMember = await db.LeagueUsers
            .AnyAsync(lu => lu.LeagueId == division.LeagueId && lu.UserId == query.UserId, ct);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this league.");

        var teams = division.Teams
            .Select(t => new LeagueTeamDto(t.Id, t.Name, t.Sport, t.Season, division.Name, t.Players.Count, t.IsActive))
            .ToList();

        return new DivisionDetailDto(division.Id, division.Name, division.AgeGroup, division.SkillLevel, division.IsActive, teams);
    }
}
