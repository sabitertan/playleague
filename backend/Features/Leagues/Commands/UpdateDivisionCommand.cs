using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Leagues.Commands;

public record UpdateDivisionCommand(
    Guid DivisionId,
    string Name,
    string? AgeGroup,
    string? SkillLevel,
    Guid UserId) : IRequest;

public class UpdateDivisionCommandHandler(AppDbContext db) : IRequestHandler<UpdateDivisionCommand>
{
    public async Task Handle(UpdateDivisionCommand cmd, CancellationToken ct)
    {
        var division = await db.Divisions.FindAsync([cmd.DivisionId], ct);
        if (division is null)
            throw new NotFoundException("Division not found.");

        var isAdmin = await db.LeagueUsers
            .AnyAsync(lu => lu.LeagueId == division.LeagueId
                         && lu.UserId == cmd.UserId
                         && lu.Role == LeagueRole.LEAGUE_ADMIN, ct);

        if (!isAdmin)
            throw new ForbiddenException("Only league admins can update divisions.");

        division.Name = cmd.Name;
        division.AgeGroup = cmd.AgeGroup;
        division.SkillLevel = cmd.SkillLevel;

        await db.SaveChangesAsync(ct);
    }
}
