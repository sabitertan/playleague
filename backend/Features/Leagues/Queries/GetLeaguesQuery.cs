using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Leagues.Queries;

public record LeagueSummaryDto(Guid Id, string Name, string Sport, string? ContactEmail, string Role);

public record GetLeaguesQuery(Guid UserId) : IRequest<List<LeagueSummaryDto>>;

public class GetLeaguesQueryHandler(AppDbContext db) : IRequestHandler<GetLeaguesQuery, List<LeagueSummaryDto>>
{
    public async Task<List<LeagueSummaryDto>> Handle(GetLeaguesQuery query, CancellationToken ct)
    {
        return await db.LeagueUsers
            .Where(lu => lu.UserId == query.UserId)
            .Include(lu => lu.League)
            .OrderBy(lu => lu.League.Name)
            .Select(lu => new LeagueSummaryDto(
                lu.League.Id,
                lu.League.Name,
                lu.League.Sport,
                lu.League.ContactEmail,
                lu.Role.ToString()))
            .ToListAsync(ct);
    }
}
