using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Roster.Queries;

public record GetPlayerByIdQuery(Guid PlayerId, Guid UserId) : IRequest<PlayerDto>;

public class GetPlayerByIdQueryHandler(AppDbContext db) : IRequestHandler<GetPlayerByIdQuery, PlayerDto>
{
    public async Task<PlayerDto> Handle(GetPlayerByIdQuery query, CancellationToken ct)
    {
        var player = await db.Players
            .FirstOrDefaultAsync(p => p.Id == query.PlayerId, ct);

        if (player is null)
            throw new NotFoundException("Player not found.");

        var isMember = await db.TeamMembers
            .AnyAsync(tm => tm.TeamId == player.TeamId && tm.UserId == query.UserId, ct);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this team.");

        return new PlayerDto(
            player.Id,
            player.Name,
            player.Email,
            player.Phone,
            player.JerseyNumber,
            player.Position,
            player.EmergencyContact,
            player.EmergencyPhone,
            player.UserId);
    }
}
