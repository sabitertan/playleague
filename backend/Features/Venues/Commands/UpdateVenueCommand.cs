using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Venues.Commands;

public record UpdateVenueCommand(
    Guid VenueId,
    string Name,
    string? Address,
    string? City,
    string? State,
    string? ZipCode,
    string? Phone,
    string? Website,
    string? Notes,
    Guid UserId
) : IRequest;

public class UpdateVenueCommandHandler(AppDbContext db) : IRequestHandler<UpdateVenueCommand>
{
    public async Task Handle(UpdateVenueCommand cmd, CancellationToken ct)
    {
        var venue = await db.Venues
            .FirstOrDefaultAsync(v => v.Id == cmd.VenueId, ct);

        if (venue is null)
            throw new NotFoundException("Venue not found.");

        // Allow if user created the venue
        bool authorized = venue.CreatedById == cmd.UserId;

        // Or if user is a team admin for the venue's team
        if (!authorized && venue.TeamId.HasValue)
        {
            var membership = await db.TeamMembers
                .FirstOrDefaultAsync(tm => tm.TeamId == venue.TeamId && tm.UserId == cmd.UserId, ct);

            authorized = membership?.Role == TeamRole.ADMIN;
        }

        if (!authorized)
            throw new ForbiddenException("You do not have permission to update this venue.");

        venue.Name = cmd.Name;
        venue.Address = cmd.Address;
        venue.City = cmd.City;
        venue.State = cmd.State;
        venue.ZipCode = cmd.ZipCode;
        venue.Phone = cmd.Phone;
        venue.Website = cmd.Website;
        venue.Notes = cmd.Notes;
        venue.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
