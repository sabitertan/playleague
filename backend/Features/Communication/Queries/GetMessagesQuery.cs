using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Communication.Queries;

public record MessageSummaryDto(
    Guid Id,
    string Subject,
    MessageType MessageType,
    MessagePriority Priority,
    DateTime CreatedAt,
    string SenderName,
    int RecipientCount
);

public record MessagePageDto(List<MessageSummaryDto> Items, int Total);

public record GetMessagesQuery(Guid LeagueId, Guid UserId, int Page, int PageSize) : IRequest<MessagePageDto>;

public class GetMessagesQueryHandler(AppDbContext db) : IRequestHandler<GetMessagesQuery, MessagePageDto>
{
    public async Task<MessagePageDto> Handle(GetMessagesQuery query, CancellationToken ct)
    {
        var isLeagueUser = await db.LeagueUsers
            .AnyAsync(lu => lu.LeagueId == query.LeagueId && lu.UserId == query.UserId, ct);

        if (!isLeagueUser)
            throw new ForbiddenException("You are not a member of this league.");

        var baseQuery = db.LeagueMessages
            .Where(m => m.LeagueId == query.LeagueId)
            .Include(m => m.Sender)
            .Include(m => m.Targeting)
            .Include(m => m.Recipients);

        var total = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .OrderByDescending(m => m.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(m => new MessageSummaryDto(
                m.Id,
                m.Subject,
                m.MessageType,
                m.Priority,
                m.CreatedAt,
                m.Sender.Name ?? m.Sender.Email,
                m.Recipients.Count
            ))
            .ToListAsync(ct);

        return new MessagePageDto(items, total);
    }
}
