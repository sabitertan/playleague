using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.PracticePlanner.Queries;

public record PlayDto(
    Guid Id,
    string Name,
    string? Description,
    string? Thumbnail,
    bool IsTemplate,
    DateTime UpdatedAt
);

public record GetPlaysQuery(Guid TeamId, Guid UserId) : IRequest<List<PlayDto>>;

public class GetPlaysQueryHandler(AppDbContext db) : IRequestHandler<GetPlaysQuery, List<PlayDto>>
{
    public async Task<List<PlayDto>> Handle(GetPlaysQuery query, CancellationToken ct)
    {
        var isMember = await db.TeamMembers
            .AnyAsync(tm => tm.TeamId == query.TeamId && tm.UserId == query.UserId, ct);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this team.");

        return await db.Plays
            .Where(p => p.TeamId == query.TeamId || p.IsTemplate)
            .OrderBy(p => p.IsTemplate)
            .ThenBy(p => p.Name)
            .Select(p => new PlayDto(
                p.Id,
                p.Name,
                p.Description,
                p.Thumbnail,
                p.IsTemplate,
                p.UpdatedAt
            ))
            .ToListAsync(ct);
    }
}
