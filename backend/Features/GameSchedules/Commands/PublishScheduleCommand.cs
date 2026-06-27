using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.GameSchedules.Commands;

public record PublishScheduleCommand(Guid ScheduleId, Guid UserId) : IRequest;

public class PublishScheduleCommandHandler(AppDbContext db) : IRequestHandler<PublishScheduleCommand>
{
    public async Task Handle(PublishScheduleCommand cmd, CancellationToken ct)
    {
        var schedule = await db.GameSchedules.FindAsync([cmd.ScheduleId], ct);
        if (schedule is null)
            throw new NotFoundException("Schedule not found.");

        // Verify the requesting user is an admin in the relevant context
        if (schedule.LeagueId.HasValue)
        {
            var isLeagueAdmin = await db.LeagueUsers
                .AnyAsync(lu => lu.LeagueId == schedule.LeagueId.Value
                             && lu.UserId == cmd.UserId
                             && lu.Role == LeagueRole.LEAGUE_ADMIN, ct);
            if (!isLeagueAdmin)
                throw new ForbiddenException("Only league admins can publish league schedules.");
        }
        else if (schedule.TeamId.HasValue)
        {
            var isTeamAdmin = await db.TeamMembers
                .AnyAsync(tm => tm.TeamId == schedule.TeamId.Value
                             && tm.UserId == cmd.UserId
                             && tm.Role == TeamRole.ADMIN, ct);
            if (!isTeamAdmin)
                throw new ForbiddenException("Only team admins can publish team schedules.");
        }
        else
        {
            throw new ForbiddenException("Schedule has no associated league or team.");
        }

        if (schedule.Status != ScheduleStatus.DRAFT)
            throw new ConflictException("Only schedules in DRAFT status can be published.");

        schedule.Status = ScheduleStatus.PUBLISHED;
        await db.SaveChangesAsync(ct);
    }
}
