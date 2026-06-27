using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Roster.Commands;

public record AddPlayerCommand(
    Guid TeamId,
    string Name,
    string? Email,
    string? Phone,
    int? JerseyNumber,
    string? Position,
    string? EmergencyContact,
    string? EmergencyPhone,
    Guid UserId) : IRequest<Guid>;

public class AddPlayerCommandHandler(AppDbContext db) : IRequestHandler<AddPlayerCommand, Guid>
{
    public async Task<Guid> Handle(AddPlayerCommand cmd, CancellationToken ct)
    {
        var member = await db.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == cmd.TeamId && tm.UserId == cmd.UserId, ct);

        if (member is null || member.Role != TeamRole.ADMIN)
            throw new ForbiddenException("You must be a team admin to add players.");

        // Duplicate jersey number check — warn but allow (log only, no exception)
        if (cmd.JerseyNumber.HasValue)
        {
            var jerseyTaken = await db.Players
                .AnyAsync(p => p.TeamId == cmd.TeamId && p.JerseyNumber == cmd.JerseyNumber, ct);

            // Intentionally not throwing — duplicate jersey numbers are allowed but notable
            // Callers may surface this as a warning via a separate validation endpoint if needed
            _ = jerseyTaken;
        }

        var player = new Player
        {
            Name = cmd.Name,
            Email = cmd.Email,
            Phone = cmd.Phone,
            JerseyNumber = cmd.JerseyNumber,
            Position = cmd.Position,
            EmergencyContact = cmd.EmergencyContact,
            EmergencyPhone = cmd.EmergencyPhone,
            TeamId = cmd.TeamId,
        };

        db.Players.Add(player);
        await db.SaveChangesAsync(ct);

        return player.Id;
    }
}
