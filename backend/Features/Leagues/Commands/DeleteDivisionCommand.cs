using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Leagues.Commands;

public record DeleteDivisionCommand(Guid DivisionId, Guid UserId) : IRequest;

public class DeleteDivisionCommandHandler(AppDbContext db) : IRequestHandler<DeleteDivisionCommand>
{
    public async Task Handle(DeleteDivisionCommand cmd, CancellationToken ct)
    {
        var division = await db.Divisions.FindAsync([cmd.DivisionId], ct);
        if (division is null)
            throw new NotFoundException("Division not found.");

        var isAdmin = await db.LeagueUsers
            .AnyAsync(lu => lu.LeagueId == division.LeagueId
                         && lu.UserId == cmd.UserId
                         && lu.Role == LeagueRole.LEAGUE_ADMIN, ct);

        if (!isAdmin)
            throw new ForbiddenException("Only league admins can delete divisions.");

        db.Divisions.Remove(division);
        await db.SaveChangesAsync(ct);
    }
}
