using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Features.Teams.Commands;
using PlayLeague.Api.Models;
using PlayLeague.Tests.Helpers;

namespace PlayLeague.Tests.Features.Teams;

public class UpdateTeamCommandTests
{
    [Fact]
    public async Task Handle_NonMember_ThrowsForbiddenException()
    {
        await using var db = DbContextFactory.Create();
        var team = new Team { Name = "Hawks", Sport = "Hockey" };
        db.Teams.Add(team);
        await db.SaveChangesAsync();

        var handler = new UpdateTeamCommandHandler(db);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new UpdateTeamCommand(team.Id, "Renamed", null, Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonAdminMember_ThrowsForbiddenException()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();
        var team = new Team { Name = "Hawks", Sport = "Hockey" };
        db.Teams.Add(team);
        db.TeamMembers.Add(new TeamMember { UserId = userId, TeamId = team.Id, Role = TeamRole.MEMBER });
        await db.SaveChangesAsync();

        var handler = new UpdateTeamCommandHandler(db);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new UpdateTeamCommand(team.Id, "Renamed", null, userId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AdminMember_UpdatesNameAndSeason()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();
        var team = new Team { Name = "Hawks", Sport = "Hockey", Season = "2023" };
        db.Teams.Add(team);
        db.TeamMembers.Add(new TeamMember { UserId = userId, TeamId = team.Id, Role = TeamRole.ADMIN });
        await db.SaveChangesAsync();

        var handler = new UpdateTeamCommandHandler(db);
        await handler.Handle(new UpdateTeamCommand(team.Id, "Falcons", "2025", userId), CancellationToken.None);

        var updated = await db.Teams.FindAsync(team.Id);
        Assert.Equal("Falcons", updated!.Name);
        Assert.Equal("2025", updated.Season);
    }
}
