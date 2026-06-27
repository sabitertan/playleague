using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.PracticePlanner.Commands;

public record UpdateSessionCommand(
    Guid SessionId,
    string Title,
    DateTime Date,
    int DurationMinutes,
    bool IsShared,
    Guid UserId
) : IRequest;

public class UpdateSessionCommandHandler(AppDbContext db) : IRequestHandler<UpdateSessionCommand>
{
    public async Task Handle(UpdateSessionCommand cmd, CancellationToken ct)
    {
        var session = await db.PracticeSessions
            .FirstOrDefaultAsync(s => s.Id == cmd.SessionId, ct);

        if (session is null)
            throw new NotFoundException($"Practice session {cmd.SessionId} not found.");

        if (session.CreatedById != cmd.UserId)
        {
            var isAdmin = await db.TeamMembers
                .AnyAsync(tm => tm.TeamId == session.TeamId && tm.UserId == cmd.UserId && tm.Role == TeamRole.ADMIN, ct);

            if (!isAdmin)
                throw new ForbiddenException("Only the session creator or a team admin can update this session.");
        }

        session.Title = cmd.Title;
        session.Date = cmd.Date;
        session.DurationMinutes = cmd.DurationMinutes;
        session.IsShared = cmd.IsShared;
        session.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
