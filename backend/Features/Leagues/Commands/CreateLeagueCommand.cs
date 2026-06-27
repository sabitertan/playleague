using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Leagues.Commands;

public record CreateLeagueCommand(
    string Name,
    string Sport,
    string? ContactEmail,
    string? ContactPhone,
    Guid UserId) : IRequest<Guid>;

public class CreateLeagueCommandHandler(AppDbContext db) : IRequestHandler<CreateLeagueCommand, Guid>
{
    public async Task<Guid> Handle(CreateLeagueCommand cmd, CancellationToken ct)
    {
        var league = new League
        {
            Name = cmd.Name,
            Sport = cmd.Sport,
            ContactEmail = cmd.ContactEmail,
            ContactPhone = cmd.ContactPhone,
        };

        db.Leagues.Add(league);

        db.LeagueUsers.Add(new LeagueUser
        {
            UserId = cmd.UserId,
            LeagueId = league.Id,
            Role = LeagueRole.LEAGUE_ADMIN,
        });

        db.AuditLogs.Add(new AuditLog
        {
            Action = "LEAGUE_CREATED",
            ResourceType = "League",
            ResourceId = league.Id.ToString(),
            Severity = AuditSeverity.LOW,
            UserId = cmd.UserId,
            LeagueId = league.Id,
        });

        await db.SaveChangesAsync(ct);

        return league.Id;
    }
}
