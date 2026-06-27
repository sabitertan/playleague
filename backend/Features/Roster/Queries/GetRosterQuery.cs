using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Roster.Queries;

public record PlayerDto(
    Guid Id,
    string Name,
    string? Email,
    string? Phone,
    int? JerseyNumber,
    string? Position,
    string? EmergencyContact,
    string? EmergencyPhone,
    Guid? UserId);

public record GetRosterQuery(Guid TeamId, Guid UserId) : IRequest<List<PlayerDto>>;

public class GetRosterQueryHandler(AppDbContext db) : IRequestHandler<GetRosterQuery, List<PlayerDto>>
{
    public async Task<List<PlayerDto>> Handle(GetRosterQuery query, CancellationToken ct)
    {
        var isMember = await db.TeamMembers
            .AnyAsync(tm => tm.TeamId == query.TeamId && tm.UserId == query.UserId, ct);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this team.");

        return await db.Players
            .Where(p => p.TeamId == query.TeamId)
            .OrderBy(p => p.JerseyNumber)
            .ThenBy(p => p.Name)
            .Select(p => new PlayerDto(
                p.Id,
                p.Name,
                p.Email,
                p.Phone,
                p.JerseyNumber,
                p.Position,
                p.EmergencyContact,
                p.EmergencyPhone,
                p.UserId))
            .ToListAsync(ct);
    }
}
