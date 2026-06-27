using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Admin.Commands;

public record ApproveUserCommand(Guid TargetUserId, Guid RequestingUserId) : IRequest;

public class ApproveUserCommandHandler(AppDbContext db) : IRequestHandler<ApproveUserCommand>
{
    public async Task Handle(ApproveUserCommand cmd, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([cmd.TargetUserId], ct);
        if (user is null)
            throw new NotFoundException("User not found.");

        user.Approved = true;
        user.UpdatedAt = DateTime.UtcNow;

        db.AuditLogs.Add(new AuditLog
        {
            Action = "USER_APPROVED",
            ResourceType = "User",
            ResourceId = cmd.TargetUserId.ToString(),
            Severity = AuditSeverity.MEDIUM,
            UserId = cmd.RequestingUserId,
        });

        await db.SaveChangesAsync(ct);
    }
}
