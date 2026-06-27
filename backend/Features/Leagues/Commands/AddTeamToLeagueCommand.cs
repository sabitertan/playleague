using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Leagues.Commands;

public record AddTeamToLeagueCommand(
    Guid LeagueId,
    Guid TeamId,
    Guid? DivisionId,
    Guid UserId) : IRequest;

public class AddTeamToLeagueCommandHandler(AppDbContext db) : IRequestHandler<AddTeamToLeagueCommand>
{
    public async Task Handle(AddTeamToLeagueCommand cmd, CancellationToken ct)
    {
        var isLeagueAdmin = await db.LeagueUsers
            .AnyAsync(lu => lu.LeagueId == cmd.LeagueId
                         && lu.UserId == cmd.UserId
                         && lu.Role == LeagueRole.LEAGUE_ADMIN, ct);

        if (!isLeagueAdmin)
            throw new ForbiddenException("Only league admins can add teams to a league.");

        var team = await db.Teams.FindAsync([cmd.TeamId], ct);
        if (team is null)
            throw new NotFoundException("Team not found.");

        var leagueExists = await db.Leagues.AnyAsync(l => l.Id == cmd.LeagueId, ct);
        if (!leagueExists)
            throw new NotFoundException("League not found.");

        if (cmd.DivisionId.HasValue)
        {
            var divisionExists = await db.Divisions
                .AnyAsync(d => d.Id == cmd.DivisionId.Value && d.LeagueId == cmd.LeagueId, ct);
            if (!divisionExists)
                throw new NotFoundException("Division not found in this league.");
        }

        team.LeagueId = cmd.LeagueId;
        team.DivisionId = cmd.DivisionId;
        team.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
