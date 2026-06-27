using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.PracticePlanner.Commands;

public record CreatePlayCommand(
    Guid TeamId,
    string Name,
    string? Description,
    string? PlayData,
    string? Thumbnail,
    bool IsTemplate,
    Guid UserId
) : IRequest<Guid>;

public class CreatePlayCommandHandler(AppDbContext db) : IRequestHandler<CreatePlayCommand, Guid>
{
    public async Task<Guid> Handle(CreatePlayCommand cmd, CancellationToken ct)
    {
        var isMember = await db.TeamMembers
            .AnyAsync(tm => tm.TeamId == cmd.TeamId && tm.UserId == cmd.UserId, ct);

        if (!isMember)
            throw new ForbiddenException("You are not a member of this team.");

        var play = new Play
        {
            TeamId = cmd.TeamId,
            Name = cmd.Name,
            Description = cmd.Description,
            PlayData = cmd.PlayData,
            Thumbnail = cmd.Thumbnail,
            IsTemplate = cmd.IsTemplate,
            CreatedById = cmd.UserId,
        };

        db.Plays.Add(play);
        await db.SaveChangesAsync(ct);

        return play.Id;
    }
}
