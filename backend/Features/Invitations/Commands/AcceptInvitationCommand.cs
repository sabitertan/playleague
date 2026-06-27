using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Invitations.Commands;

public record AcceptInvitationCommand(string Token, Guid UserId) : IRequest;

public class AcceptInvitationCommandHandler(AppDbContext db) : IRequestHandler<AcceptInvitationCommand>
{
    public async Task Handle(AcceptInvitationCommand cmd, CancellationToken ct)
    {
        var invitation = await db.Invitations
            .FirstOrDefaultAsync(i => i.Token == cmd.Token && i.Status == InvitationStatus.PENDING, ct);

        if (invitation is null)
            throw new NotFoundException("Invitation not found.");

        if (invitation.ExpiresAt < DateTime.UtcNow)
            throw new ForbiddenException("Invitation has expired.");

        db.TeamMembers.Add(new TeamMember
        {
            UserId = cmd.UserId,
            TeamId = invitation.TeamId,
            Role = TeamRole.MEMBER,
        });

        invitation.Status = InvitationStatus.ACCEPTED;

        await db.SaveChangesAsync(ct);
    }
}
