using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Communication.Queries;

public record MessageDetailDto(
    Guid Id,
    string Subject,
    string Content,
    MessageType MessageType,
    MessagePriority Priority,
    DateTime CreatedAt,
    string SenderName,
    int RecipientCount);

public record GetMessageByIdQuery(Guid LeagueId, Guid MessageId, Guid UserId) : IRequest<MessageDetailDto>;

public class GetMessageByIdQueryHandler(AppDbContext db) : IRequestHandler<GetMessageByIdQuery, MessageDetailDto>
{
    public async Task<MessageDetailDto> Handle(GetMessageByIdQuery query, CancellationToken ct)
    {
        var isMember = await db.LeagueUsers
            .AnyAsync(lu => lu.LeagueId == query.LeagueId && lu.UserId == query.UserId, ct);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this league.");

        var message = await db.LeagueMessages
            .Include(m => m.Sender)
            .Include(m => m.Recipients)
            .FirstOrDefaultAsync(m => m.Id == query.MessageId && m.LeagueId == query.LeagueId, ct);

        if (message is null)
            throw new NotFoundException("Message not found.");

        return new MessageDetailDto(
            message.Id,
            message.Subject,
            message.Content,
            message.MessageType,
            message.Priority,
            message.CreatedAt,
            message.Sender.Name ?? message.Sender.Email,
            message.Recipients.Count);
    }
}
