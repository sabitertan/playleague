using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.PracticePlanner.Commands;

public record DeleteSessionCommand(Guid SessionId, Guid UserId) : IRequest;

public class DeleteSessionCommandHandler(AppDbContext db) : IRequestHandler<DeleteSessionCommand>
{
    public async Task Handle(DeleteSessionCommand cmd, CancellationToken ct)
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
                throw new ForbiddenException("Only the session creator or a team admin can delete this session.");
        }

        db.PracticeSessions.Remove(session);
        await db.SaveChangesAsync(ct);
    }
}
