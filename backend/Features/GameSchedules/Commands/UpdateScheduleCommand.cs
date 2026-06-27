using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.GameSchedules.Commands;

public record UpdateScheduleCommand(
    Guid ScheduleId,
    string Name,
    string? SeasonName,
    DateTime? StartDate,
    DateTime? EndDate,
    bool RoundRobin,
    int? Rounds,
    string? Notes,
    Guid UserId) : IRequest;

public class UpdateScheduleCommandHandler(AppDbContext db) : IRequestHandler<UpdateScheduleCommand>
{
    public async Task Handle(UpdateScheduleCommand cmd, CancellationToken ct)
    {
        var schedule = await db.GameSchedules.FindAsync([cmd.ScheduleId], ct);
        if (schedule is null)
            throw new NotFoundException("Schedule not found.");

        if (schedule.LeagueId.HasValue)
        {
            var isAdmin = await db.LeagueUsers
                .AnyAsync(lu => lu.LeagueId == schedule.LeagueId && lu.UserId == cmd.UserId && lu.Role == LeagueRole.LEAGUE_ADMIN, ct);
            if (!isAdmin)
                throw new ForbiddenException("Only league admins can update league schedules.");
        }
        else if (schedule.TeamId.HasValue)
        {
            var isAdmin = await db.TeamMembers
                .AnyAsync(tm => tm.TeamId == schedule.TeamId && tm.UserId == cmd.UserId && tm.Role == TeamRole.ADMIN, ct);
            if (!isAdmin)
                throw new ForbiddenException("Only team admins can update team schedules.");
        }

        schedule.Name = cmd.Name;
        schedule.SeasonName = cmd.SeasonName;
        schedule.StartDate = cmd.StartDate;
        schedule.EndDate = cmd.EndDate;
        schedule.RoundRobin = cmd.RoundRobin;
        schedule.Rounds = cmd.Rounds;
        schedule.Notes = cmd.Notes;

        await db.SaveChangesAsync(ct);
    }
}
