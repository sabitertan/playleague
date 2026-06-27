using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Rsvp.Commands;

public record UpdateRsvpCommand(Guid EventId, RsvpStatus Status, Guid UserId) : IRequest;

public class UpdateRsvpCommandHandler(AppDbContext db) : IRequestHandler<UpdateRsvpCommand>
{
    public async Task Handle(UpdateRsvpCommand cmd, CancellationToken ct)
    {
        var ev = await db.Events.FirstOrDefaultAsync(e => e.Id == cmd.EventId, ct);

        if (ev is null)
            throw new NotFoundException("Event not found.");

        var isMember = await db.TeamMembers
            .AnyAsync(tm => tm.TeamId == ev.TeamId && tm.UserId == cmd.UserId, ct);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this team.");

        var existing = await db.Rsvps
            .FirstOrDefaultAsync(r => r.EventId == cmd.EventId && r.UserId == cmd.UserId, ct);

        if (existing is not null)
        {
            existing.Status = cmd.Status;
            existing.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            db.Rsvps.Add(new Models.Rsvp
            {
                EventId = cmd.EventId,
                UserId = cmd.UserId,
                Status = cmd.Status,
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
