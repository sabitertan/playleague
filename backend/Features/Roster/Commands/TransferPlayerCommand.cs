using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Roster.Commands;

public record TransferPlayerCommand(
    Guid PlayerId,
    Guid FromTeamId,
    Guid ToTeamId,
    Guid LeagueId,
    string? Reason,
    Guid UserId) : IRequest;

public class TransferPlayerCommandHandler(AppDbContext db) : IRequestHandler<TransferPlayerCommand>
{
    public async Task Handle(TransferPlayerCommand cmd, CancellationToken ct)
    {
        var leagueUser = await db.LeagueUsers
            .FirstOrDefaultAsync(lu => lu.LeagueId == cmd.LeagueId && lu.UserId == cmd.UserId, ct);

        if (leagueUser is null || leagueUser.Role != LeagueRole.LEAGUE_ADMIN)
            throw new ForbiddenException("You must be a league admin to transfer players.");

        var player = await db.Players
            .FirstOrDefaultAsync(p => p.Id == cmd.PlayerId && p.TeamId == cmd.FromTeamId, ct);

        if (player is null)
            throw new NotFoundException($"Player {cmd.PlayerId} not found in source team {cmd.FromTeamId}.");

        var toTeamExists = await db.Teams
            .AnyAsync(t => t.Id == cmd.ToTeamId && t.LeagueId == cmd.LeagueId, ct);

        if (!toTeamExists)
            throw new NotFoundException($"Destination team {cmd.ToTeamId} not found in league {cmd.LeagueId}.");

        // Move the player
        player.TeamId = cmd.ToTeamId;

        // Audit trail
        db.PlayerTransfers.Add(new PlayerTransfer
        {
            PlayerId = cmd.PlayerId,
            FromTeamId = cmd.FromTeamId,
            ToTeamId = cmd.ToTeamId,
            LeagueId = cmd.LeagueId,
            Reason = cmd.Reason,
            TransferredById = cmd.UserId,
        });

        await db.SaveChangesAsync(ct);
    }
}
