using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Features.Roster.Queries;
using PlayLeague.Api.Models;
using PlayLeague.Tests.Helpers;

namespace PlayLeague.Tests.Features.Roster;

public class GetPlayerByIdQueryTests
{
    [Fact]
    public async Task Handle_NonExistentPlayer_ThrowsNotFoundException()
    {
        await using var db = DbContextFactory.Create();
        var handler = new GetPlayerByIdQueryHandler(db);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new GetPlayerByIdQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonMember_ThrowsForbiddenException()
    {
        await using var db = DbContextFactory.Create();
        var team = new Team { Name = "Hawks", Sport = "Hockey" };
        db.Teams.Add(team);
        var player = new Player { Name = "John Doe", TeamId = team.Id };
        db.Players.Add(player);
        await db.SaveChangesAsync();

        var handler = new GetPlayerByIdQueryHandler(db);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new GetPlayerByIdQuery(player.Id, Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ValidMember_ReturnsPlayerDto()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();
        var team = new Team { Name = "Hawks", Sport = "Hockey" };
        db.Teams.Add(team);
        db.TeamMembers.Add(new TeamMember { UserId = userId, TeamId = team.Id, Role = TeamRole.ADMIN });
        var player = new Player { Name = "John Doe", TeamId = team.Id, JerseyNumber = 10, Position = "Forward" };
        db.Players.Add(player);
        await db.SaveChangesAsync();

        var handler = new GetPlayerByIdQueryHandler(db);
        var result = await handler.Handle(new GetPlayerByIdQuery(player.Id, userId), CancellationToken.None);

        Assert.Equal(player.Id, result.Id);
        Assert.Equal("John Doe", result.Name);
        Assert.Equal(10, result.JerseyNumber);
        Assert.Equal("Forward", result.Position);
    }
}
