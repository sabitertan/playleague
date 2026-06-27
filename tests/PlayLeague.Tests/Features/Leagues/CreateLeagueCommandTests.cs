using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Features.Leagues.Commands;
using PlayLeague.Api.Models;
using PlayLeague.Tests.Helpers;

namespace PlayLeague.Tests.Features.Leagues;

public class CreateLeagueCommandTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesLeagueWithAdminUser()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreateLeagueCommandHandler(db);
        var userId = Guid.NewGuid();

        var id = await handler.Handle(
            new CreateLeagueCommand("City League", "Hockey", "contact@league.com", null, userId),
            CancellationToken.None);

        var league = await db.Leagues.FindAsync(id);
        var leagueUser = await db.LeagueUsers.FindAsync(userId, id);

        Assert.NotNull(league);
        Assert.Equal("City League", league.Name);
        Assert.Equal("Hockey", league.Sport);
        Assert.Equal("contact@league.com", league.ContactEmail);
        Assert.NotNull(leagueUser);
        Assert.Equal(LeagueRole.LEAGUE_ADMIN, leagueUser.Role);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesAuditLog()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreateLeagueCommandHandler(db);
        var userId = Guid.NewGuid();

        var id = await handler.Handle(
            new CreateLeagueCommand("City League", "Hockey", null, null, userId),
            CancellationToken.None);

        var log = await db.AuditLogs.FirstOrDefaultAsync(a => a.LeagueId == id);
        Assert.NotNull(log);
        Assert.Equal("LEAGUE_CREATED", log.Action);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewNonEmptyId()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreateLeagueCommandHandler(db);

        var id = await handler.Handle(
            new CreateLeagueCommand("Rec League", "Soccer", null, null, Guid.NewGuid()),
            CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
    }
}
