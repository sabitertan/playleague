using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.PracticePlanner.Commands;

public record DeletePlayCommand(Guid PlayId, Guid UserId) : IRequest;

public class DeletePlayCommandHandler(AppDbContext db) : IRequestHandler<DeletePlayCommand>
{
    public async Task Handle(DeletePlayCommand cmd, CancellationToken ct)
    {
        var play = await db.Plays
            .FirstOrDefaultAsync(p => p.Id == cmd.PlayId, ct);

        if (play is null)
            throw new NotFoundException($"Play {cmd.PlayId} not found.");

        if (play.CreatedById != cmd.UserId)
        {
            var teamId = play.TeamId
                ?? throw new ForbiddenException("Only the play creator can delete template plays.");

            var isAdmin = await db.TeamMembers
                .AnyAsync(tm => tm.TeamId == teamId && tm.UserId == cmd.UserId && tm.Role == TeamRole.ADMIN, ct);

            if (!isAdmin)
                throw new ForbiddenException("Only the play creator or a team admin can delete this play.");
        }

        db.Plays.Remove(play);
        await db.SaveChangesAsync(ct);
    }
}
