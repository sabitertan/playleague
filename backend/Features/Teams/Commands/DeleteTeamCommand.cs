using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Teams.Commands;

public record DeleteTeamCommand(Guid TeamId, Guid UserId) : IRequest;

public class DeleteTeamCommandHandler(AppDbContext db) : IRequestHandler<DeleteTeamCommand>
{
    public async Task Handle(DeleteTeamCommand cmd, CancellationToken ct)
    {
        var member = await db.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == cmd.TeamId && tm.UserId == cmd.UserId, ct);

        if (member is null || member.Role != TeamRole.ADMIN)
            throw new ForbiddenException("You must be a team admin to delete this team.");

        var team = await db.Teams.FindAsync([cmd.TeamId], ct);

        if (team is null)
            throw new NotFoundException($"Team {cmd.TeamId} not found.");

        db.Teams.Remove(team);
        await db.SaveChangesAsync(ct);
    }
}
