using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.GameSchedules.Commands;

public record CreateScheduleCommand(
    string Name,
    string? SeasonName,
    DateTime? StartDate,
    DateTime? EndDate,
    bool RoundRobin,
    int? Rounds,
    string? Notes,
    Guid? LeagueId,
    Guid? TeamId,
    Guid UserId) : IRequest<Guid>;

public class CreateScheduleCommandHandler(AppDbContext db) : IRequestHandler<CreateScheduleCommand, Guid>
{
    public async Task<Guid> Handle(CreateScheduleCommand cmd, CancellationToken ct)
    {
        if (cmd.LeagueId.HasValue)
        {
            var isLeagueAdmin = await db.LeagueUsers
                .AnyAsync(lu => lu.LeagueId == cmd.LeagueId.Value
                             && lu.UserId == cmd.UserId
                             && lu.Role == LeagueRole.LEAGUE_ADMIN, ct);
            if (!isLeagueAdmin)
                throw new ForbiddenException("Only league admins can create league schedules.");
        }
        else if (cmd.TeamId.HasValue)
        {
            var isTeamAdmin = await db.TeamMembers
                .AnyAsync(tm => tm.TeamId == cmd.TeamId.Value
                             && tm.UserId == cmd.UserId
                             && tm.Role == TeamRole.ADMIN, ct);
            if (!isTeamAdmin)
                throw new ForbiddenException("Only team admins can create team schedules.");
        }
        else
        {
            throw new ForbiddenException("Either LeagueId or TeamId must be provided.");
        }

        var schedule = new GameSchedule
        {
            Name = cmd.Name,
            SeasonName = cmd.SeasonName,
            StartDate = cmd.StartDate,
            EndDate = cmd.EndDate,
            Status = ScheduleStatus.DRAFT,
            RoundRobin = cmd.RoundRobin,
            Rounds = cmd.Rounds,
            Notes = cmd.Notes,
            LeagueId = cmd.LeagueId,
            TeamId = cmd.TeamId,
            CreatedById = cmd.UserId,
        };

        db.GameSchedules.Add(schedule);
        await db.SaveChangesAsync(ct);

        return schedule.Id;
    }
}
