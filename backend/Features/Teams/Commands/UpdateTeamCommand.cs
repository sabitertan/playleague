using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Teams.Commands;

public record UpdateTeamCommand(
    Guid TeamId,
    string Name,
    string? Season,
    Guid UserId) : IRequest;

public class UpdateTeamCommandHandler(AppDbContext db) : IRequestHandler<UpdateTeamCommand>
{
    public async Task Handle(UpdateTeamCommand cmd, CancellationToken ct)
    {
        var member = await db.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == cmd.TeamId && tm.UserId == cmd.UserId, ct);

        if (member is null || member.Role != TeamRole.ADMIN)
            throw new ForbiddenException("You must be a team admin to update this team.");

        var team = await db.Teams.FindAsync([cmd.TeamId], ct);

        if (team is null)
            throw new NotFoundException($"Team {cmd.TeamId} not found.");

        team.Name = cmd.Name;
        team.Season = cmd.Season;
        team.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
