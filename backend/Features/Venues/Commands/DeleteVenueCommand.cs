using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Venues.Commands;

public record DeleteVenueCommand(Guid VenueId, Guid UserId) : IRequest;

public class DeleteVenueCommandHandler(AppDbContext db) : IRequestHandler<DeleteVenueCommand>
{
    public async Task Handle(DeleteVenueCommand cmd, CancellationToken ct)
    {
        var venue = await db.Venues.FindAsync([cmd.VenueId], ct);
        if (venue is null)
            throw new NotFoundException("Venue not found.");

        bool authorized = venue.CreatedById == cmd.UserId;

        if (!authorized && venue.TeamId.HasValue)
        {
            authorized = await db.TeamMembers
                .AnyAsync(tm => tm.TeamId == venue.TeamId && tm.UserId == cmd.UserId && tm.Role == TeamRole.ADMIN, ct);
        }

        if (!authorized && venue.LeagueId.HasValue)
        {
            authorized = await db.LeagueUsers
                .AnyAsync(lu => lu.LeagueId == venue.LeagueId && lu.UserId == cmd.UserId && lu.Role == LeagueRole.LEAGUE_ADMIN, ct);
        }

        if (!authorized)
            throw new ForbiddenException("You do not have permission to delete this venue.");

        db.Venues.Remove(venue);
        await db.SaveChangesAsync(ct);
    }
}
