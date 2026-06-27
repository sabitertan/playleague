using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Coaches.Commands;

public record DeleteCoachCommand(Guid CoachId, Guid UserId) : IRequest;

public class DeleteCoachCommandHandler(AppDbContext db) : IRequestHandler<DeleteCoachCommand>
{
    public async Task Handle(DeleteCoachCommand cmd, CancellationToken ct)
    {
        var coach = await db.Coaches.FirstOrDefaultAsync(c => c.Id == cmd.CoachId, ct);

        if (coach is null)
            throw new NotFoundException($"Coach {cmd.CoachId} not found.");

        if (coach.CreatedById != cmd.UserId)
            throw new ForbiddenException("You can only delete coaches you created.");

        db.Coaches.Remove(coach);
        await db.SaveChangesAsync(ct);
    }
}
