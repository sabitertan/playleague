using PlayLeague.Api.Features.Teams.Queries;
using PlayLeague.Api.Models;
using PlayLeague.Tests.Helpers;

namespace PlayLeague.Tests.Features.Teams;

public class GetTeamsQueryTests
{
    [Fact]
    public async Task Handle_UserWithNoTeams_ReturnsEmptyList()
    {
        await using var db = DbContextFactory.Create();
        var handler = new GetTeamsQueryHandler(db);

        var result = await handler.Handle(new GetTeamsQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_UserWithTeams_ReturnsOnlyTheirTeams()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var myTeam = new Team { Name = "My Team", Sport = "Hockey" };
        var otherTeam = new Team { Name = "Other Team", Sport = "Soccer" };
        db.Teams.AddRange(myTeam, otherTeam);
        db.TeamMembers.Add(new TeamMember { UserId = userId, TeamId = myTeam.Id, Role = TeamRole.ADMIN });
        db.TeamMembers.Add(new TeamMember { UserId = otherUserId, TeamId = otherTeam.Id, Role = TeamRole.ADMIN });
        await db.SaveChangesAsync();

        var handler = new GetTeamsQueryHandler(db);
        var result = await handler.Handle(new GetTeamsQuery(userId), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("My Team", result[0].Name);
        Assert.Equal("ADMIN", result[0].Role);
    }

    [Fact]
    public async Task Handle_UserMemberOfMultipleTeams_ReturnsAll()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();

        var team1 = new Team { Name = "Team A", Sport = "Hockey" };
        var team2 = new Team { Name = "Team B", Sport = "Soccer" };
        db.Teams.AddRange(team1, team2);
        db.TeamMembers.AddRange(
            new TeamMember { UserId = userId, TeamId = team1.Id, Role = TeamRole.ADMIN },
            new TeamMember { UserId = userId, TeamId = team2.Id, Role = TeamRole.MEMBER });
        await db.SaveChangesAsync();

        var handler = new GetTeamsQueryHandler(db);
        var result = await handler.Handle(new GetTeamsQuery(userId), CancellationToken.None);

        Assert.Equal(2, result.Count);
    }
}
