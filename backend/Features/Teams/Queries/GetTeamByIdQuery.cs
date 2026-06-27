using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Teams.Queries;

public record MemberDto(Guid UserId, string? Name, string Email, string Role);

public record TeamDetailDto(
    Guid Id,
    string Name,
    string Sport,
    string? Season,
    List<MemberDto> Members,
    int PlayerCount);

public record GetTeamByIdQuery(Guid TeamId, Guid UserId) : IRequest<TeamDetailDto>;

public class GetTeamByIdQueryHandler(AppDbContext db) : IRequestHandler<GetTeamByIdQuery, TeamDetailDto>
{
    public async Task<TeamDetailDto> Handle(GetTeamByIdQuery query, CancellationToken ct)
    {
        var team = await db.Teams
            .Include(t => t.TeamMembers)
                .ThenInclude(tm => tm.User)
            .Include(t => t.Players)
            .FirstOrDefaultAsync(t => t.Id == query.TeamId, ct);

        if (team is null)
            throw new NotFoundException($"Team {query.TeamId} not found.");

        var isMember = team.TeamMembers.Any(tm => tm.UserId == query.UserId);
        if (!isMember)
            throw new ForbiddenException("You are not a member of this team.");

        var members = team.TeamMembers
            .Select(tm => new MemberDto(
                tm.UserId,
                tm.User.Name,
                tm.User.Email,
                tm.Role.ToString()))
            .ToList();

        return new TeamDetailDto(
            team.Id,
            team.Name,
            team.Sport,
            team.Season,
            members,
            team.Players.Count);
    }
}
