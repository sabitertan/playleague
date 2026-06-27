using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Communication.Commands;

public record SendMessageCommand(
    Guid LeagueId,
    string Subject,
    string Content,
    MessageType MessageType,
    MessagePriority Priority,
    bool EntireLeague,
    Guid? DivisionId,
    Guid? TeamId,
    Guid UserId
) : IRequest<Guid>;

public class SendMessageCommandHandler(AppDbContext db) : IRequestHandler<SendMessageCommand, Guid>
{
    public async Task<Guid> Handle(SendMessageCommand cmd, CancellationToken ct)
    {
        var callerMembership = await db.LeagueUsers
            .FirstOrDefaultAsync(lu => lu.LeagueId == cmd.LeagueId && lu.UserId == cmd.UserId, ct);

        if (callerMembership is null || callerMembership.Role != LeagueRole.LEAGUE_ADMIN)
            throw new ForbiddenException("Only league admins can send messages.");

        var message = new LeagueMessage
        {
            LeagueId = cmd.LeagueId,
            Subject = cmd.Subject,
            Content = cmd.Content,
            MessageType = cmd.MessageType,
            Priority = cmd.Priority,
            SenderId = cmd.UserId,
        };

        db.LeagueMessages.Add(message);

        var targeting = new MessageTargeting
        {
            MessageId = message.Id,
            EntireLeague = cmd.EntireLeague,
            DivisionId = cmd.DivisionId,
            TeamId = cmd.TeamId,
        };

        db.MessageTargetings.Add(targeting);

        List<Guid> recipientUserIds;

        if (cmd.EntireLeague)
        {
            recipientUserIds = await db.LeagueUsers
                .Where(lu => lu.LeagueId == cmd.LeagueId)
                .Select(lu => lu.UserId)
                .ToListAsync(ct);
        }
        else if (cmd.DivisionId.HasValue)
        {
            recipientUserIds = await db.TeamMembers
                .Where(tm => tm.Team.DivisionId == cmd.DivisionId.Value)
                .Select(tm => tm.UserId)
                .Distinct()
                .ToListAsync(ct);
        }
        else if (cmd.TeamId.HasValue)
        {
            recipientUserIds = await db.TeamMembers
                .Where(tm => tm.TeamId == cmd.TeamId.Value)
                .Select(tm => tm.UserId)
                .ToListAsync(ct);
        }
        else
        {
            recipientUserIds = [];
        }

        foreach (var userId in recipientUserIds)
        {
            db.MessageRecipients.Add(new MessageRecipient
            {
                MessageId = message.Id,
                UserId = userId,
                DeliveryStatus = DeliveryStatus.PENDING,
            });
        }

        await db.SaveChangesAsync(ct);

        return message.Id;
    }
}
