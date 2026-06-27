using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Leagues.Commands;

public record UpdateLeagueCommand(
    Guid LeagueId,
    string Name,
    string? ContactEmail,
    string? ContactPhone,
    Guid UserId) : IRequest;

public class UpdateLeagueCommandHandler(AppDbContext db) : IRequestHandler<UpdateLeagueCommand>
{
    public async Task Handle(UpdateLeagueCommand cmd, CancellationToken ct)
    {
        var isLeagueAdmin = await db.LeagueUsers
            .AnyAsync(lu => lu.LeagueId == cmd.LeagueId
                         && lu.UserId == cmd.UserId
                         && lu.Role == LeagueRole.LEAGUE_ADMIN, ct);

        if (!isLeagueAdmin)
            throw new ForbiddenException("Only league admins can update the league.");

        var league = await db.Leagues.FindAsync([cmd.LeagueId], ct);
        if (league is null)
            throw new NotFoundException("League not found.");

        league.Name = cmd.Name;
        league.ContactEmail = cmd.ContactEmail;
        league.ContactPhone = cmd.ContactPhone;
        league.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
