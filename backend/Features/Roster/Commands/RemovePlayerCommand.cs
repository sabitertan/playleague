using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Roster.Commands;

public record RemovePlayerCommand(Guid PlayerId, Guid TeamId, Guid UserId) : IRequest;

public class RemovePlayerCommandHandler(AppDbContext db) : IRequestHandler<RemovePlayerCommand>
{
    public async Task Handle(RemovePlayerCommand cmd, CancellationToken ct)
    {
        var member = await db.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == cmd.TeamId && tm.UserId == cmd.UserId, ct);

        if (member is null || member.Role != TeamRole.ADMIN)
            throw new ForbiddenException("You must be a team admin to remove players.");

        var player = await db.Players
            .FirstOrDefaultAsync(p => p.Id == cmd.PlayerId && p.TeamId == cmd.TeamId, ct);

        if (player is null)
            throw new NotFoundException($"Player {cmd.PlayerId} not found in this team.");

        db.Players.Remove(player);
        await db.SaveChangesAsync(ct);
    }
}
