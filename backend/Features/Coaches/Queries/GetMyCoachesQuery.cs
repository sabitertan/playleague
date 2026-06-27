using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;

namespace PlayLeague.Api.Features.Coaches.Queries;

public record AssignedTeamDto(Guid TeamId, string TeamName);

public record CoachDto(
    Guid Id,
    string Name,
    string? Title,
    string? Email,
    string? Phone,
    List<AssignedTeamDto> Teams);

public record GetMyCoachesQuery(Guid UserId) : IRequest<List<CoachDto>>;

public class GetMyCoachesQueryHandler(AppDbContext db) : IRequestHandler<GetMyCoachesQuery, List<CoachDto>>
{
    public async Task<List<CoachDto>> Handle(GetMyCoachesQuery query, CancellationToken ct)
    {
        return await db.Coaches
            .Where(c => c.CreatedById == query.UserId)
            .OrderBy(c => c.Name)
            .Select(c => new CoachDto(
                c.Id,
                c.Name,
                c.Title,
                c.Email,
                c.Phone,
                c.Assignments
                    .Select(a => new AssignedTeamDto(a.TeamId, a.Team.Name))
                    .ToList()))
            .ToListAsync(ct);
    }
}
