using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.PracticePlanner.Commands;

public record UpdatePlayCommand(
    Guid PlayId,
    string Name,
    string? Description,
    string? PlayData,
    string? Thumbnail,
    Guid UserId
) : IRequest;

public class UpdatePlayCommandHandler(AppDbContext db) : IRequestHandler<UpdatePlayCommand>
{
    public async Task Handle(UpdatePlayCommand cmd, CancellationToken ct)
    {
        var play = await db.Plays
            .FirstOrDefaultAsync(p => p.Id == cmd.PlayId, ct);

        if (play is null)
            throw new NotFoundException($"Play {cmd.PlayId} not found.");

        if (play.CreatedById != cmd.UserId)
        {
            var teamId = play.TeamId
                ?? throw new ForbiddenException("Only the play creator can update template plays.");

            var isAdmin = await db.TeamMembers
                .AnyAsync(tm => tm.TeamId == teamId && tm.UserId == cmd.UserId && tm.Role == TeamRole.ADMIN, ct);

            if (!isAdmin)
                throw new ForbiddenException("Only the play creator or a team admin can update this play.");
        }

        play.Name = cmd.Name;
        play.Description = cmd.Description;
        play.PlayData = cmd.PlayData;
        play.Thumbnail = cmd.Thumbnail;
        play.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
    }
}
