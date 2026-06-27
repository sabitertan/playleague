using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Leagues.Commands;

public record DeleteLeagueCommand(Guid LeagueId, Guid UserId) : IRequest;

public class DeleteLeagueCommandHandler(AppDbContext db) : IRequestHandler<DeleteLeagueCommand>
{
    public async Task Handle(DeleteLeagueCommand cmd, CancellationToken ct)
    {
        var isAdmin = await db.LeagueUsers
            .AnyAsync(lu => lu.LeagueId == cmd.LeagueId
                         && lu.UserId == cmd.UserId
                         && lu.Role == LeagueRole.LEAGUE_ADMIN, ct);

        if (!isAdmin)
            throw new ForbiddenException("Only league admins can delete the league.");

        var league = await db.Leagues.FindAsync([cmd.LeagueId], ct);
        if (league is null)
            throw new NotFoundException("League not found.");

        db.Leagues.Remove(league);
        await db.SaveChangesAsync(ct);
    }
}
