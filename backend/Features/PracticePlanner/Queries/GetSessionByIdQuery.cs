using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.PracticePlanner.Queries;

public record SessionPlayDto(
    Guid Id,
    int Sequence,
    int DurationMinutes,
    string? Instructions,
    Guid PlayId,
    string PlayName,
    string? PlayThumbnail
);

public record SessionDetailDto(
    Guid Id,
    string Title,
    DateTime Date,
    int DurationMinutes,
    bool IsShared,
    Guid TeamId,
    Guid CreatedById,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<SessionPlayDto> Plays
);

public record GetSessionByIdQuery(Guid SessionId, Guid UserId) : IRequest<SessionDetailDto>;

public class GetSessionByIdQueryHandler(AppDbContext db) : IRequestHandler<GetSessionByIdQuery, SessionDetailDto>
{
    public async Task<SessionDetailDto> Handle(GetSessionByIdQuery query, CancellationToken ct)
    {
        var session = await db.PracticeSessions
            .Include(s => s.SessionPlays)
                .ThenInclude(sp => sp.Play)
            .FirstOrDefaultAsync(s => s.Id == query.SessionId, ct);

        if (session is null)
            throw new NotFoundException($"Practice session {query.SessionId} not found.");

        var isMember = await db.TeamMembers
            .AnyAsync(tm => tm.TeamId == session.TeamId && tm.UserId == query.UserId, ct);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this team.");

        var plays = session.SessionPlays
            .OrderBy(sp => sp.Sequence)
            .Select(sp => new SessionPlayDto(
                sp.Id,
                sp.Sequence,
                sp.DurationMinutes,
                sp.Instructions,
                sp.PlayId,
                sp.Play.Name,
                sp.Play.Thumbnail
            ))
            .ToList();

        return new SessionDetailDto(
            session.Id,
            session.Title,
            session.Date,
            session.DurationMinutes,
            session.IsShared,
            session.TeamId,
            session.CreatedById,
            session.CreatedAt,
            session.UpdatedAt,
            plays
        );
    }
}
