using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Features.Leagues.Commands;
using PlayLeague.Api.Models;
using PlayLeague.Tests.Helpers;

namespace PlayLeague.Tests.Features.Leagues;

public class DeleteDivisionCommandTests
{
    [Fact]
    public async Task Handle_NonExistentDivision_ThrowsNotFoundException()
    {
        await using var db = DbContextFactory.Create();
        var handler = new DeleteDivisionCommandHandler(db);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new DeleteDivisionCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonAdmin_ThrowsForbiddenException()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();
        var league = new League { Name = "City League", Sport = "Hockey" };
        var division = new Division { Name = "Division A", LeagueId = league.Id };
        db.Leagues.Add(league);
        db.Divisions.Add(division);
        db.LeagueUsers.Add(new LeagueUser { UserId = userId, LeagueId = league.Id, Role = LeagueRole.MEMBER });
        await db.SaveChangesAsync();

        var handler = new DeleteDivisionCommandHandler(db);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new DeleteDivisionCommand(division.Id, userId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AdminUser_DeletesDivision()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();
        var league = new League { Name = "City League", Sport = "Hockey" };
        var division = new Division { Name = "Division A", LeagueId = league.Id };
        db.Leagues.Add(league);
        db.Divisions.Add(division);
        db.LeagueUsers.Add(new LeagueUser { UserId = userId, LeagueId = league.Id, Role = LeagueRole.LEAGUE_ADMIN });
        await db.SaveChangesAsync();

        var handler = new DeleteDivisionCommandHandler(db);
        await handler.Handle(new DeleteDivisionCommand(division.Id, userId), CancellationToken.None);

        Assert.Null(await db.Divisions.FindAsync(division.Id));
    }
}
