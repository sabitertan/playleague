using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Coaches.Queries;

public record TeamCoachDto(
    Guid Id,
    string Name,
    string? Title,
    string? Email,
    string? Phone);

public record GetTeamCoachesQuery(Guid TeamId, Guid UserId) : IRequest<List<TeamCoachDto>>;

public class GetTeamCoachesQueryHandler(AppDbContext db) : IRequestHandler<GetTeamCoachesQuery, List<TeamCoachDto>>
{
    public async Task<List<TeamCoachDto>> Handle(GetTeamCoachesQuery query, CancellationToken ct)
    {
        var isMember = await db.TeamMembers
            .AnyAsync(tm => tm.TeamId == query.TeamId && tm.UserId == query.UserId, ct);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this team.");

        return await db.CoachAssignments
            .Where(ca => ca.TeamId == query.TeamId)
            .OrderBy(ca => ca.Coach.Name)
            .Select(ca => new TeamCoachDto(
                ca.Coach.Id,
                ca.Coach.Name,
                ca.Coach.Title,
                ca.Coach.Email,
                ca.Coach.Phone))
            .ToListAsync(ct);
    }
}
