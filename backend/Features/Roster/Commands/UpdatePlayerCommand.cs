using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Roster.Commands;

public record UpdatePlayerCommand(
    Guid PlayerId,
    Guid TeamId,
    string Name,
    string? Email,
    string? Phone,
    int? JerseyNumber,
    string? Position,
    string? EmergencyContact,
    string? EmergencyPhone,
    Guid UserId) : IRequest;

public class UpdatePlayerCommandHandler(AppDbContext db) : IRequestHandler<UpdatePlayerCommand>
{
    public async Task Handle(UpdatePlayerCommand cmd, CancellationToken ct)
    {
        var member = await db.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == cmd.TeamId && tm.UserId == cmd.UserId, ct);

        if (member is null || member.Role != TeamRole.ADMIN)
            throw new ForbiddenException("You must be a team admin to update players.");

        var player = await db.Players
            .FirstOrDefaultAsync(p => p.Id == cmd.PlayerId && p.TeamId == cmd.TeamId, ct);

        if (player is null)
            throw new NotFoundException($"Player {cmd.PlayerId} not found in this team.");

        player.Name = cmd.Name;
        player.Email = cmd.Email;
        player.Phone = cmd.Phone;
        player.JerseyNumber = cmd.JerseyNumber;
        player.Position = cmd.Position;
        player.EmergencyContact = cmd.EmergencyContact;
        player.EmergencyPhone = cmd.EmergencyPhone;

        await db.SaveChangesAsync(ct);
    }
}
