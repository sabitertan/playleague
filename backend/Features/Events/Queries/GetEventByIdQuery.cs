using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Events.Queries;

public record RsvpSummaryDto(Guid UserId, string? Name, string Email, RsvpStatus Status);

public record EventDetailDto(
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
    RsvpStatus? UserRsvp,
    List<RsvpSummaryDto> Rsvps
);

public record GetEventByIdQuery(Guid EventId, Guid UserId) : IRequest<EventDetailDto>;

public class GetEventByIdQueryHandler(AppDbContext db) : IRequestHandler<GetEventByIdQuery, EventDetailDto>
{
    public async Task<EventDetailDto> Handle(GetEventByIdQuery query, CancellationToken ct)
    {
        var ev = await db.Events
            .Include(e => e.Rsvps)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(e => e.Id == query.EventId, ct);

        if (ev is null)
            throw new NotFoundException("Event not found.");

        var isMember = await db.TeamMembers
            .AnyAsync(tm => tm.TeamId == ev.TeamId && tm.UserId == query.UserId, ct);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this team.");

        var rsvpSummaries = ev.Rsvps
            .Select(r => new RsvpSummaryDto(r.UserId, r.User.Name, r.User.Email, r.Status))
            .ToList();

        return new EventDetailDto(
            ev.Id,
            ev.Title,
            ev.Type,
            ev.StartAt,
            ev.EndAt,
            ev.Location,
            ev.Opponent,
            ev.Notes,
            ev.VenueId,
            ev.Rsvps.Count(r => r.Status == RsvpStatus.GOING),
            ev.Rsvps.Count(r => r.Status == RsvpStatus.NOT_GOING),
            ev.Rsvps.Count(r => r.Status == RsvpStatus.MAYBE),
            ev.Rsvps.FirstOrDefault(r => r.UserId == query.UserId)?.Status,
            rsvpSummaries
        );
    }
}
