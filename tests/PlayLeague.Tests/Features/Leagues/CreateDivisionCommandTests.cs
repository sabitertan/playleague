using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Features.Leagues.Commands;
using PlayLeague.Api.Models;
using PlayLeague.Tests.Helpers;

namespace PlayLeague.Tests.Features.Leagues;

public class CreateDivisionCommandTests
{
    [Fact]
    public async Task Handle_NonAdmin_ThrowsForbiddenException()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();
        var league = new League { Name = "City League", Sport = "Hockey" };
        db.Leagues.Add(league);
        db.LeagueUsers.Add(new LeagueUser { UserId = userId, LeagueId = league.Id, Role = LeagueRole.MEMBER });
        await db.SaveChangesAsync();

        var handler = new CreateDivisionCommandHandler(db);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new CreateDivisionCommand(league.Id, "Division A", null, null, userId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonExistentLeague_ThrowsNotFoundException()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();
        var missingId = Guid.NewGuid();
        db.LeagueUsers.Add(new LeagueUser { UserId = userId, LeagueId = missingId, Role = LeagueRole.LEAGUE_ADMIN });
        await db.SaveChangesAsync();

        var handler = new CreateDivisionCommandHandler(db);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new CreateDivisionCommand(missingId, "Division A", null, null, userId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AdminUser_CreatesDivisionWithCorrectLeague()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();
        var league = new League { Name = "City League", Sport = "Hockey" };
        db.Leagues.Add(league);
        db.LeagueUsers.Add(new LeagueUser { UserId = userId, LeagueId = league.Id, Role = LeagueRole.LEAGUE_ADMIN });
        await db.SaveChangesAsync();

        var handler = new CreateDivisionCommandHandler(db);
        var id = await handler.Handle(
            new CreateDivisionCommand(league.Id, "Division A", "U18", "Elite", userId),
            CancellationToken.None);

        var division = await db.Divisions.FindAsync(id);
        Assert.NotNull(division);
        Assert.Equal("Division A", division.Name);
        Assert.Equal("U18", division.AgeGroup);
        Assert.Equal("Elite", division.SkillLevel);
        Assert.Equal(league.Id, division.LeagueId);
    }
}
