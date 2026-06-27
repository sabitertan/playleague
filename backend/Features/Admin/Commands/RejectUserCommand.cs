using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Admin.Commands;

public record RejectUserCommand(Guid TargetUserId, Guid RequestingUserId) : IRequest;

public class RejectUserCommandHandler(AppDbContext db) : IRequestHandler<RejectUserCommand>
{
    public async Task Handle(RejectUserCommand cmd, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([cmd.TargetUserId], ct);
        if (user is null)
            throw new NotFoundException("User not found.");

        // Log before removal so the audit entry can reference the user id
        db.AuditLogs.Add(new AuditLog
        {
            Action = "USER_REJECTED",
            ResourceType = "User",
            ResourceId = cmd.TargetUserId.ToString(),
            Severity = AuditSeverity.MEDIUM,
            UserId = cmd.RequestingUserId,
        });

        db.Users.Remove(user);

        await db.SaveChangesAsync(ct);
    }
}
