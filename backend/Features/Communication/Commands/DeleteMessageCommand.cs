using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Communication.Commands;

public record DeleteMessageCommand(Guid LeagueId, Guid MessageId, Guid UserId) : IRequest;

public class DeleteMessageCommandHandler(AppDbContext db) : IRequestHandler<DeleteMessageCommand>
{
    public async Task Handle(DeleteMessageCommand cmd, CancellationToken ct)
    {
        var isAdmin = await db.LeagueUsers
            .AnyAsync(lu => lu.LeagueId == cmd.LeagueId && lu.UserId == cmd.UserId && lu.Role == LeagueRole.LEAGUE_ADMIN, ct);

        if (!isAdmin)
            throw new ForbiddenException("Only league admins can delete messages.");

        var message = await db.LeagueMessages
            .FirstOrDefaultAsync(m => m.Id == cmd.MessageId && m.LeagueId == cmd.LeagueId, ct);

        if (message is null)
            throw new NotFoundException("Message not found.");

        db.LeagueMessages.Remove(message);
        await db.SaveChangesAsync(ct);
    }
}
