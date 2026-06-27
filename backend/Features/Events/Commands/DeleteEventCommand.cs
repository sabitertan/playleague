using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Events.Commands;

public record DeleteEventCommand(Guid EventId, Guid UserId) : IRequest;

public class DeleteEventCommandHandler(AppDbContext db) : IRequestHandler<DeleteEventCommand>
{
    public async Task Handle(DeleteEventCommand cmd, CancellationToken ct)
    {
        var ev = await db.Events.FirstOrDefaultAsync(e => e.Id == cmd.EventId, ct);

        if (ev is null)
            throw new NotFoundException("Event not found.");

        var membership = await db.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == ev.TeamId && tm.UserId == cmd.UserId, ct);

        if (membership is null || membership.Role != TeamRole.ADMIN)
            throw new ForbiddenException("Only team admins can delete events.");

        db.Events.Remove(ev);
        await db.SaveChangesAsync(ct);
    }
}
