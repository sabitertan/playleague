using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Events.Queries;

public record EventDto(
    Guid Id,
    string Title,
    EventType Type,
    DateTime StartAt,
    DateTime? EndAt,
    string? Location,
    string? Opponent,
    string? Notes,
    Guid? VenueId,
    int RsvpGoing,
    int RsvpNotGoing,
    int RsvpMaybe,
    RsvpStatus? UserRsvp
);

public record GetEventsQuery(Guid TeamId, Guid UserId) : IRequest<List<EventDto>>;

public class GetEventsQueryHandler(AppDbContext db) : IRequestHandler<GetEventsQuery, List<EventDto>>
{
    public async Task<List<EventDto>> Handle(GetEventsQuery query, CancellationToken ct)
    {
        var isMember = await db.TeamMembers
            .AnyAsync(tm => tm.TeamId == query.TeamId && tm.UserId == query.UserId, ct);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this team.");

        var events = await db.Events
            .Where(e => e.TeamId == query.TeamId)
            .Include(e => e.Rsvps)
            .OrderByDescending(e => e.StartAt)
            .ToListAsync(ct);

        return events.Select(e => new EventDto(
            e.Id,
            e.Title,
            e.Type,
            e.StartAt,
            e.EndAt,
            e.Location,
            e.Opponent,
            e.Notes,
            e.VenueId,
            e.Rsvps.Count(r => r.Status == RsvpStatus.GOING),
            e.Rsvps.Count(r => r.Status == RsvpStatus.NOT_GOING),
            e.Rsvps.Count(r => r.Status == RsvpStatus.MAYBE),
            e.Rsvps.FirstOrDefault(r => r.UserId == query.UserId)?.Status
        )).ToList();
    }
}
