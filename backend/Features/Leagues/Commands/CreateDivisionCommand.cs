using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Leagues.Commands;

public record CreateDivisionCommand(
    Guid LeagueId,
    string Name,
    string? AgeGroup,
    string? SkillLevel,
    Guid UserId) : IRequest<Guid>;

public class CreateDivisionCommandHandler(AppDbContext db) : IRequestHandler<CreateDivisionCommand, Guid>
{
    public async Task<Guid> Handle(CreateDivisionCommand cmd, CancellationToken ct)
    {
        var isLeagueAdmin = await db.LeagueUsers
            .AnyAsync(lu => lu.LeagueId == cmd.LeagueId
                         && lu.UserId == cmd.UserId
                         && lu.Role == LeagueRole.LEAGUE_ADMIN, ct);

        if (!isLeagueAdmin)
            throw new ForbiddenException("Only league admins can create divisions.");

        var leagueExists = await db.Leagues.AnyAsync(l => l.Id == cmd.LeagueId, ct);
        if (!leagueExists)
            throw new NotFoundException("League not found.");

        var division = new Division
        {
            Name = cmd.Name,
            AgeGroup = cmd.AgeGroup,
            SkillLevel = cmd.SkillLevel,
            LeagueId = cmd.LeagueId,
        };

        db.Divisions.Add(division);
        await db.SaveChangesAsync(ct);

        return division.Id;
    }
}
