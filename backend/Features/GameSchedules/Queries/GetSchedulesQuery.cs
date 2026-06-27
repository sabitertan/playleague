using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.GameSchedules.Queries;

public record ScheduleDto(
    Guid Id,
    string Name,
    string? SeasonName,
    ScheduleStatus Status,
    int GameCount,
    DateTime? StartDate,
    DateTime? EndDate);

public record GetSchedulesQuery(Guid? TeamId, Guid? LeagueId, Guid UserId) : IRequest<List<ScheduleDto>>;

public class GetSchedulesQueryHandler(AppDbContext db) : IRequestHandler<GetSchedulesQuery, List<ScheduleDto>>
{
    public async Task<List<ScheduleDto>> Handle(GetSchedulesQuery query, CancellationToken ct)
    {
        // Verify user membership in the requested context
        if (query.TeamId.HasValue)
        {
            var isMember = await db.TeamMembers
                .AnyAsync(tm => tm.TeamId == query.TeamId.Value && tm.UserId == query.UserId, ct);
            if (!isMember)
                throw new ForbiddenException("You are not a member of this team.");
        }
        else if (query.LeagueId.HasValue)
        {
            var isMember = await db.LeagueUsers
                .AnyAsync(lu => lu.LeagueId == query.LeagueId.Value && lu.UserId == query.UserId, ct);
            if (!isMember)
                throw new ForbiddenException("You are not a member of this league.");
        }
        else
        {
            throw new ForbiddenException("Either TeamId or LeagueId must be provided.");
        }

        var schedulesQuery = db.GameSchedules
            .Include(gs => gs.Games)
            .AsQueryable();

        if (query.TeamId.HasValue)
            schedulesQuery = schedulesQuery.Where(gs => gs.TeamId == query.TeamId.Value);

        if (query.LeagueId.HasValue)
            schedulesQuery = schedulesQuery.Where(gs => gs.LeagueId == query.LeagueId.Value);

        var schedules = await schedulesQuery
            .OrderByDescending(gs => gs.CreatedAt)
            .ToListAsync(ct);

        return schedules.Select(gs => new ScheduleDto(
            gs.Id,
            gs.Name,
            gs.SeasonName,
            gs.Status,
            gs.Games.Count,
            gs.StartDate,
            gs.EndDate))
            .ToList();
    }
}
