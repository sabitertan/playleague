using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Coaches.Commands;

public record AssignCoachCommand(Guid TeamId, Guid CoachId, Guid UserId) : IRequest;

public class AssignCoachCommandHandler(AppDbContext db) : IRequestHandler<AssignCoachCommand>
{
    public async Task Handle(AssignCoachCommand cmd, CancellationToken ct)
    {
        var member = await db.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == cmd.TeamId && tm.UserId == cmd.UserId, ct);

        if (member is null || member.Role != TeamRole.ADMIN)
            throw new ForbiddenException("You must be a team admin to assign coaches.");

        var coach = await db.Coaches.FirstOrDefaultAsync(c => c.Id == cmd.CoachId, ct);

        if (coach is null)
            throw new NotFoundException($"Coach {cmd.CoachId} not found.");

        if (coach.CreatedById != cmd.UserId)
            throw new ForbiddenException("You can only assign coaches you created.");

        var alreadyAssigned = await db.CoachAssignments
            .AnyAsync(ca => ca.CoachId == cmd.CoachId && ca.TeamId == cmd.TeamId, ct);

        if (alreadyAssigned)
            throw new ConflictException("This coach is already assigned to the team.");

        db.CoachAssignments.Add(new CoachAssignment
        {
            CoachId = cmd.CoachId,
            TeamId = cmd.TeamId,
        });

        await db.SaveChangesAsync(ct);
    }
}
