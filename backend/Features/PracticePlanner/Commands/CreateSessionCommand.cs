using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.PracticePlanner.Commands;

public record CreateSessionCommand(
    Guid TeamId,
    string Title,
    DateTime Date,
    int DurationMinutes,
    Guid UserId
) : IRequest<Guid>;

public class CreateSessionCommandHandler(AppDbContext db) : IRequestHandler<CreateSessionCommand, Guid>
{
    public async Task<Guid> Handle(CreateSessionCommand cmd, CancellationToken ct)
    {
        var isMember = await db.TeamMembers
            .AnyAsync(tm => tm.TeamId == cmd.TeamId && tm.UserId == cmd.UserId, ct);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this team.");

        var session = new PracticeSession
        {
            TeamId = cmd.TeamId,
            Title = cmd.Title,
            Date = cmd.Date,
            DurationMinutes = cmd.DurationMinutes,
            CreatedById = cmd.UserId,
        };

        db.PracticeSessions.Add(session);
        await db.SaveChangesAsync(ct);

        return session.Id;
    }
}
