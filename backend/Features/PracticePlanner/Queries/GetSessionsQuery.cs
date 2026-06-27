using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.PracticePlanner.Queries;

public record SessionSummaryDto(
    Guid Id,
    string Title,
    DateTime Date,
    int DurationMinutes,
    bool IsShared,
    int PlayCount
);

public record GetSessionsQuery(Guid TeamId, Guid UserId) : IRequest<List<SessionSummaryDto>>;

public class GetSessionsQueryHandler(AppDbContext db) : IRequestHandler<GetSessionsQuery, List<SessionSummaryDto>>
{
    public async Task<List<SessionSummaryDto>> Handle(GetSessionsQuery query, CancellationToken ct)
    {
        var isMember = await db.TeamMembers
            .AnyAsync(tm => tm.TeamId == query.TeamId && tm.UserId == query.UserId, ct);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this team.");

        return await db.PracticeSessions
            .Where(s => s.TeamId == query.TeamId)
            .Include(s => s.SessionPlays)
            .OrderByDescending(s => s.Date)
            .Select(s => new SessionSummaryDto(
                s.Id,
                s.Title,
                s.Date,
                s.DurationMinutes,
                s.IsShared,
                s.SessionPlays.Count
            ))
            .ToListAsync(ct);
    }
}
