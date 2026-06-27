using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.GameSchedules.Commands;

public record DeleteScheduleCommand(Guid ScheduleId, Guid UserId) : IRequest;

public class DeleteScheduleCommandHandler(AppDbContext db) : IRequestHandler<DeleteScheduleCommand>
{
    public async Task Handle(DeleteScheduleCommand cmd, CancellationToken ct)
    {
        var schedule = await db.GameSchedules.FindAsync([cmd.ScheduleId], ct);
        if (schedule is null)
            throw new NotFoundException("Schedule not found.");

        if (schedule.LeagueId.HasValue)
        {
            var isAdmin = await db.LeagueUsers
                .AnyAsync(lu => lu.LeagueId == schedule.LeagueId && lu.UserId == cmd.UserId && lu.Role == LeagueRole.LEAGUE_ADMIN, ct);
            if (!isAdmin)
                throw new ForbiddenException("Only league admins can delete league schedules.");
        }
        else if (schedule.TeamId.HasValue)
        {
            var isAdmin = await db.TeamMembers
                .AnyAsync(tm => tm.TeamId == schedule.TeamId && tm.UserId == cmd.UserId && tm.Role == TeamRole.ADMIN, ct);
            if (!isAdmin)
                throw new ForbiddenException("Only team admins can delete team schedules.");
        }

        db.GameSchedules.Remove(schedule);
        await db.SaveChangesAsync(ct);
    }
}
