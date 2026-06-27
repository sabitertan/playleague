using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Events.Commands;

public record UpdateEventCommand(
    Guid EventId,
    string Title,
    EventType Type,
    DateTime StartAt,
    DateTime? EndAt,
    string? Location,
    string? Opponent,
    string? Notes,
    Guid? VenueId,
    Guid UserId
) : IRequest;

public class UpdateEventCommandHandler(AppDbContext db) : IRequestHandler<UpdateEventCommand>
{
    public async Task Handle(UpdateEventCommand cmd, CancellationToken ct)
    {
        var ev = await db.Events.FirstOrDefaultAsync(e => e.Id == cmd.EventId, ct);

        if (ev is null)
            throw new NotFoundException("Event not found.");

        var membership = await db.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == ev.TeamId && tm.UserId == cmd.UserId, ct);

        if (membership is null || membership.Role != TeamRole.ADMIN)
            throw new ForbiddenException("Only team admins can update events.");

        ev.Title = cmd.Title;
        ev.Type = cmd.Type;
        ev.StartAt = cmd.StartAt;
        ev.EndAt = cmd.EndAt;
        ev.Location = cmd.Location;
        ev.Opponent = cmd.Opponent;
        ev.Notes = cmd.Notes;
        ev.VenueId = cmd.VenueId;
        ev.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
