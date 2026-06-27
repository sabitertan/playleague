using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Features.Leagues.Queries;
using PlayLeague.Api.Models;
using PlayLeague.Tests.Helpers;

namespace PlayLeague.Tests.Features.Leagues;

public class GetLeagueQueryTests
{
    [Fact]
    public async Task Handle_NonMember_ThrowsForbiddenException()
    {
        await using var db = DbContextFactory.Create();
        var league = new League { Name = "City League", Sport = "Hockey" };
        db.Leagues.Add(league);
        await db.SaveChangesAsync();

        var handler = new GetLeagueQueryHandler(db);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new GetLeagueQuery(league.Id, Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistentLeague_ThrowsNotFoundException()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();
        var missingId = Guid.NewGuid();
        db.LeagueUsers.Add(new LeagueUser { UserId = userId, LeagueId = missingId, Role = LeagueRole.LEAGUE_ADMIN });
        await db.SaveChangesAsync();

        var handler = new GetLeagueQueryHandler(db);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new GetLeagueQuery(missingId, userId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ValidMember_ReturnsLeagueDetail()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();
        var league = new League { Name = "City League", Sport = "Hockey", ContactEmail = "info@league.com" };
        db.Leagues.Add(league);
        db.LeagueUsers.Add(new LeagueUser { UserId = userId, LeagueId = league.Id, Role = LeagueRole.LEAGUE_ADMIN });
        await db.SaveChangesAsync();

        var handler = new GetLeagueQueryHandler(db);
        var result = await handler.Handle(new GetLeagueQuery(league.Id, userId), CancellationToken.None);

        Assert.Equal(league.Id, result.Id);
        Assert.Equal("City League", result.Name);
        Assert.Equal("Hockey", result.Sport);
        Assert.Equal("info@league.com", result.ContactEmail);
        Assert.Equal(1, result.MemberCount);
        Assert.Empty(result.Divisions);
    }

    [Fact]
    public async Task Handle_LeagueWithDivisions_ReturnsDivisionList()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();
        var league = new League { Name = "City League", Sport = "Hockey" };
        db.Leagues.Add(league);
        db.LeagueUsers.Add(new LeagueUser { UserId = userId, LeagueId = league.Id, Role = LeagueRole.LEAGUE_ADMIN });
        db.Divisions.AddRange(
            new Division { Name = "Division A", LeagueId = league.Id },
            new Division { Name = "Division B", LeagueId = league.Id });
        await db.SaveChangesAsync();

        var handler = new GetLeagueQueryHandler(db);
        var result = await handler.Handle(new GetLeagueQuery(league.Id, userId), CancellationToken.None);

        Assert.Equal(2, result.Divisions.Count);
    }
}
