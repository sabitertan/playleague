using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Communication.Commands;

public record UpdateMessageCommand(
    Guid LeagueId,
    Guid MessageId,
    string Subject,
    string Content,
    MessagePriority Priority,
    Guid UserId) : IRequest;

public class UpdateMessageCommandHandler(AppDbContext db) : IRequestHandler<UpdateMessageCommand>
{
    public async Task Handle(UpdateMessageCommand cmd, CancellationToken ct)
    {
        var isAdmin = await db.LeagueUsers
            .AnyAsync(lu => lu.LeagueId == cmd.LeagueId && lu.UserId == cmd.UserId && lu.Role == LeagueRole.LEAGUE_ADMIN, ct);

        if (!isAdmin)
            throw new ForbiddenException("Only league admins can update messages.");

        var message = await db.LeagueMessages
            .FirstOrDefaultAsync(m => m.Id == cmd.MessageId && m.LeagueId == cmd.LeagueId, ct);

        if (message is null)
            throw new NotFoundException("Message not found.");

        message.Subject = cmd.Subject;
        message.Content = cmd.Content;
        message.Priority = cmd.Priority;

        await db.SaveChangesAsync(ct);
    }
}
