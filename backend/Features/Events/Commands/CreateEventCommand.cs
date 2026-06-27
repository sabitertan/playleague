using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Events.Commands;

public record CreateEventCommand(
    Guid TeamId,
    string Title,
    EventType Type,
    DateTime StartAt,
    DateTime? EndAt,
    string? Location,
    string? Opponent,
    string? Notes,
    Guid? VenueId,
    Guid UserId
) : IRequest<Guid>;

public class CreateEventCommandHandler(AppDbContext db) : IRequestHandler<CreateEventCommand, Guid>
{
    public async Task<Guid> Handle(CreateEventCommand cmd, CancellationToken ct)
    {
        var membership = await db.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == cmd.TeamId && tm.UserId == cmd.UserId, ct);

        if (membership is null || membership.Role != TeamRole.ADMIN)
            throw new ForbiddenException("Only team admins can create events.");

        var ev = new Event
        {
            TeamId = cmd.TeamId,
            Title = cmd.Title,
            Type = cmd.Type,
            StartAt = cmd.StartAt,
            EndAt = cmd.EndAt,
            Location = cmd.Location,
            Opponent = cmd.Opponent,
            Notes = cmd.Notes,
            VenueId = cmd.VenueId,
        };

        db.Events.Add(ev);

        var memberIds = await db.TeamMembers
            .Where(tm => tm.TeamId == cmd.TeamId)
            .Select(tm => tm.UserId)
            .ToListAsync(ct);

        foreach (var memberId in memberIds)
        {
            db.Rsvps.Add(new Models.Rsvp
            {
                EventId = ev.Id,
                UserId = memberId,
                Status = RsvpStatus.NO_RESPONSE,
            });
        }

        await db.SaveChangesAsync(ct);

        return ev.Id;
    }
}
