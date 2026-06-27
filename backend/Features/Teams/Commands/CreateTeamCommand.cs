using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Teams.Commands;

public record CreateTeamCommand(
    string Name,
    string Sport,
    string? Season,
    Guid CreatedByUserId) : IRequest<Guid>;

public class CreateTeamCommandHandler(AppDbContext db) : IRequestHandler<CreateTeamCommand, Guid>
{
    public async Task<Guid> Handle(CreateTeamCommand cmd, CancellationToken ct)
    {
        var team = new Team
        {
            Name = cmd.Name,
            Sport = cmd.Sport,
            Season = cmd.Season,
        };

        db.Teams.Add(team);

        db.TeamMembers.Add(new TeamMember
        {
            UserId = cmd.CreatedByUserId,
            TeamId = team.Id,
            Role = TeamRole.ADMIN,
        });

        await db.SaveChangesAsync(ct);

        return team.Id;
    }
}
