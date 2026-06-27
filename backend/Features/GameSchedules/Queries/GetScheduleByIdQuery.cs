using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.GameSchedules.Queries;

public record ScheduleDetailDto(
    Guid Id,
    string Name,
    string? SeasonName,
    ScheduleStatus Status,
    DateTime? StartDate,
    DateTime? EndDate,
    bool RoundRobin,
    int? Rounds,
    string? Notes,
    int GameCount,
    Guid? LeagueId,
    Guid? TeamId);

public record GetScheduleByIdQuery(Guid ScheduleId, Guid UserId) : IRequest<ScheduleDetailDto>;

public class GetScheduleByIdQueryHandler(AppDbContext db) : IRequestHandler<GetScheduleByIdQuery, ScheduleDetailDto>
{
    public async Task<ScheduleDetailDto> Handle(GetScheduleByIdQuery query, CancellationToken ct)
    {
        var schedule = await db.GameSchedules
            .Include(gs => gs.Games)
            .FirstOrDefaultAsync(gs => gs.Id == query.ScheduleId, ct);

        if (schedule is null)
            throw new NotFoundException("Schedule not found.");

        bool hasAccess = false;

        if (schedule.TeamId.HasValue)
            hasAccess = await db.TeamMembers.AnyAsync(tm => tm.TeamId == schedule.TeamId && tm.UserId == query.UserId, ct);

        if (!hasAccess && schedule.LeagueId.HasValue)
            hasAccess = await db.LeagueUsers.AnyAsync(lu => lu.LeagueId == schedule.LeagueId && lu.UserId == query.UserId, ct);

        if (!hasAccess)
            throw new ForbiddenException("You do not have access to this schedule.");

        return new ScheduleDetailDto(
            schedule.Id,
            schedule.Name,
            schedule.SeasonName,
            schedule.Status,
            schedule.StartDate,
            schedule.EndDate,
            schedule.RoundRobin,
            schedule.Rounds,
            schedule.Notes,
            schedule.Games.Count,
            schedule.LeagueId,
            schedule.TeamId);
    }
}
