using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Coaches.Commands;

public record UnassignCoachCommand(Guid TeamId, Guid CoachId, Guid UserId) : IRequest;

public class UnassignCoachCommandHandler(AppDbContext db) : IRequestHandler<UnassignCoachCommand>
{
    public async Task Handle(UnassignCoachCommand cmd, CancellationToken ct)
    {
        var member = await db.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == cmd.TeamId && tm.UserId == cmd.UserId, ct);

        if (member is null || member.Role != TeamRole.ADMIN)
            throw new ForbiddenException("You must be a team admin to unassign coaches.");

        var assignment = await db.CoachAssignments
            .FirstOrDefaultAsync(ca => ca.CoachId == cmd.CoachId && ca.TeamId == cmd.TeamId, ct);

        if (assignment is null)
            throw new NotFoundException("This coach is not assigned to the team.");

        db.CoachAssignments.Remove(assignment);
        await db.SaveChangesAsync(ct);
    }
}
