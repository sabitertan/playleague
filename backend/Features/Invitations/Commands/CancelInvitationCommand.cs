using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Invitations.Commands;

public record CancelInvitationCommand(Guid InvitationId, Guid UserId) : IRequest;

public class CancelInvitationCommandHandler(AppDbContext db) : IRequestHandler<CancelInvitationCommand>
{
    public async Task Handle(CancelInvitationCommand cmd, CancellationToken ct)
    {
        var invitation = await db.Invitations.FindAsync([cmd.InvitationId], ct);
        if (invitation is null)
            throw new NotFoundException("Invitation not found.");

        var membership = await db.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == invitation.TeamId && tm.UserId == cmd.UserId, ct);

        if (membership is null || membership.Role != TeamRole.ADMIN)
            throw new ForbiddenException("Only team admins can cancel invitations.");

        db.Invitations.Remove(invitation);
        await db.SaveChangesAsync(ct);
    }
}
