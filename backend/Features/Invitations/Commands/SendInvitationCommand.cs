using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Invitations.Commands;

public record SendInvitationCommand(Guid TeamId, string Email, Guid UserId) : IRequest;

public class SendInvitationCommandHandler(AppDbContext db) : IRequestHandler<SendInvitationCommand>
{
    public async Task Handle(SendInvitationCommand cmd, CancellationToken ct)
    {
        var callerMembership = await db.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == cmd.TeamId && tm.UserId == cmd.UserId, ct);

        if (callerMembership is null || callerMembership.Role != TeamRole.ADMIN)
            throw new ForbiddenException("Only team admins can send invitations.");

        var existingUser = await db.Users
            .FirstOrDefaultAsync(u => u.Email == cmd.Email, ct);

        if (existingUser is not null)
        {
            var alreadyMember = await db.TeamMembers
                .AnyAsync(tm => tm.TeamId == cmd.TeamId && tm.UserId == existingUser.Id, ct);

            if (alreadyMember)
                throw new ConflictException("Already a team member.");

            db.TeamMembers.Add(new TeamMember
            {
                UserId = existingUser.Id,
                TeamId = cmd.TeamId,
                Role = TeamRole.MEMBER,
            });

            await db.SaveChangesAsync(ct);
            return;
        }

        var pendingInvitation = await db.Invitations
            .AnyAsync(i => i.TeamId == cmd.TeamId && i.Email == cmd.Email && i.Status == InvitationStatus.PENDING, ct);

        if (pendingInvitation)
            throw new ConflictException("A pending invitation already exists for this email.");

        db.Invitations.Add(new Invitation
        {
            TeamId = cmd.TeamId,
            Email = cmd.Email,
            InvitedById = cmd.UserId,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            Status = InvitationStatus.PENDING,
        });

        await db.SaveChangesAsync(ct);
    }
}
