using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Leagues.Queries;

public record GetDivisionsQuery(Guid LeagueId, Guid UserId) : IRequest<List<DivisionDto>>;

public class GetDivisionsQueryHandler(AppDbContext db) : IRequestHandler<GetDivisionsQuery, List<DivisionDto>>
{
    public async Task<List<DivisionDto>> Handle(GetDivisionsQuery query, CancellationToken ct)
    {
        var isMember = await db.LeagueUsers
            .AnyAsync(lu => lu.LeagueId == query.LeagueId && lu.UserId == query.UserId, ct);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this league.");

        return await db.Divisions
            .Where(d => d.LeagueId == query.LeagueId && d.IsActive)
            .Include(d => d.Teams)
            .OrderBy(d => d.Name)
            .Select(d => new DivisionDto(d.Id, d.Name, d.AgeGroup, d.SkillLevel, d.Teams.Count))
            .ToListAsync(ct);
    }
}
