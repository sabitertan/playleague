using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Features.Leagues.Commands;
using PlayLeague.Api.Models;
using PlayLeague.Tests.Helpers;

namespace PlayLeague.Tests.Features.Leagues;

public class DeleteLeagueCommandTests
{
    [Fact]
    public async Task Handle_NonMember_ThrowsForbiddenException()
    {
        await using var db = DbContextFactory.Create();
        var league = new League { Name = "City League", Sport = "Hockey" };
        db.Leagues.Add(league);
        await db.SaveChangesAsync();

        var handler = new DeleteLeagueCommandHandler(db);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new DeleteLeagueCommand(league.Id, Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonAdminMember_ThrowsForbiddenException()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();
        var league = new League { Name = "City League", Sport = "Hockey" };
        db.Leagues.Add(league);
        db.LeagueUsers.Add(new LeagueUser { UserId = userId, LeagueId = league.Id, Role = LeagueRole.MEMBER });
        await db.SaveChangesAsync();

        var handler = new DeleteLeagueCommandHandler(db);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new DeleteLeagueCommand(league.Id, userId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AdminMember_DeletesLeague()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();
        var league = new League { Name = "City League", Sport = "Hockey" };
        db.Leagues.Add(league);
        db.LeagueUsers.Add(new LeagueUser { UserId = userId, LeagueId = league.Id, Role = LeagueRole.LEAGUE_ADMIN });
        await db.SaveChangesAsync();

        var handler = new DeleteLeagueCommandHandler(db);
        await handler.Handle(new DeleteLeagueCommand(league.Id, userId), CancellationToken.None);

        Assert.Null(await db.Leagues.FindAsync(league.Id));
    }

    [Fact]
    public async Task Handle_AdminOfNonExistentLeague_ThrowsNotFoundException()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();
        var missingLeagueId = Guid.NewGuid();
        db.LeagueUsers.Add(new LeagueUser { UserId = userId, LeagueId = missingLeagueId, Role = LeagueRole.LEAGUE_ADMIN });
        await db.SaveChangesAsync();

        var handler = new DeleteLeagueCommandHandler(db);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new DeleteLeagueCommand(missingLeagueId, userId), CancellationToken.None));
    }
}
