using PlayLeague.Api.Features.Leagues.Queries;
using PlayLeague.Api.Models;
using PlayLeague.Tests.Helpers;

namespace PlayLeague.Tests.Features.Leagues;

public class GetLeaguesQueryTests
{
    [Fact]
    public async Task Handle_UserWithNoLeagues_ReturnsEmptyList()
    {
        await using var db = DbContextFactory.Create();
        var handler = new GetLeaguesQueryHandler(db);

        var result = await handler.Handle(new GetLeaguesQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_UserWithLeagues_ReturnsTheirLeagues()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var myLeague = new League { Name = "My League", Sport = "Hockey" };
        var otherLeague = new League { Name = "Other League", Sport = "Soccer" };
        db.Leagues.AddRange(myLeague, otherLeague);
        db.LeagueUsers.Add(new LeagueUser { UserId = userId, LeagueId = myLeague.Id, Role = LeagueRole.LEAGUE_ADMIN });
        db.LeagueUsers.Add(new LeagueUser { UserId = otherUserId, LeagueId = otherLeague.Id, Role = LeagueRole.LEAGUE_ADMIN });
        await db.SaveChangesAsync();

        var handler = new GetLeaguesQueryHandler(db);
        var result = await handler.Handle(new GetLeaguesQuery(userId), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("My League", result[0].Name);
        Assert.Equal("LEAGUE_ADMIN", result[0].Role);
    }

    [Fact]
    public async Task Handle_UserInMultipleLeagues_ReturnsAll()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();

        var league1 = new League { Name = "League A", Sport = "Hockey" };
        var league2 = new League { Name = "League B", Sport = "Soccer" };
        db.Leagues.AddRange(league1, league2);
        db.LeagueUsers.AddRange(
            new LeagueUser { UserId = userId, LeagueId = league1.Id, Role = LeagueRole.LEAGUE_ADMIN },
            new LeagueUser { UserId = userId, LeagueId = league2.Id, Role = LeagueRole.MEMBER });
        await db.SaveChangesAsync();

        var handler = new GetLeaguesQueryHandler(db);
        var result = await handler.Handle(new GetLeaguesQuery(userId), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }
}
