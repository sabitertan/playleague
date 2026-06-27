using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.PracticePlanner.Commands;

public record AddPlayToSessionCommand(
    Guid SessionId,
    Guid PlayId,
    int DurationMinutes,
    string? Instructions,
    Guid UserId
) : IRequest<Guid>;

public class AddPlayToSessionCommandHandler(AppDbContext db) : IRequestHandler<AddPlayToSessionCommand, Guid>
{
    public async Task<Guid> Handle(AddPlayToSessionCommand cmd, CancellationToken ct)
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
                throw new ForbiddenException("Only the session creator or a team admin can add plays to this session.");
        }

        var playExists = await db.Plays.AnyAsync(p => p.Id == cmd.PlayId, ct);
        if (!playExists)
            throw new NotFoundException($"Play {cmd.PlayId} not found.");

        var maxSequence = await db.PracticeSessionPlays
            .Where(sp => sp.SessionId == cmd.SessionId)
            .Select(sp => (int?)sp.Sequence)
            .MaxAsync(ct) ?? 0;

        var sessionPlay = new PracticeSessionPlay
        {
            SessionId = cmd.SessionId,
            PlayId = cmd.PlayId,
            DurationMinutes = cmd.DurationMinutes,
            Instructions = cmd.Instructions,
            Sequence = maxSequence + 1,
        };

        db.PracticeSessionPlays.Add(sessionPlay);
        await db.SaveChangesAsync(ct);

        return sessionPlay.Id;
    }
}
