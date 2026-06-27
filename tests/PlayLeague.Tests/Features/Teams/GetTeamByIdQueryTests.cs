using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Features.Teams.Queries;
using PlayLeague.Api.Models;
using PlayLeague.Tests.Helpers;

namespace PlayLeague.Tests.Features.Teams;

public class GetTeamByIdQueryTests
{
    [Fact]
    public async Task Handle_NonExistentTeam_ThrowsNotFoundException()
    {
        await using var db = DbContextFactory.Create();
        var handler = new GetTeamByIdQueryHandler(db);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new GetTeamByIdQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonMember_ThrowsForbiddenException()
    {
        await using var db = DbContextFactory.Create();
        var team = new Team { Name = "Hawks", Sport = "Hockey" };
        db.Teams.Add(team);
        await db.SaveChangesAsync();

        var handler = new GetTeamByIdQueryHandler(db);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new GetTeamByIdQuery(team.Id, Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ValidMember_ReturnsTeamDetail()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "admin@test.com", PasswordHash = "hash", Name = "Admin" };
        var team = new Team { Name = "Hawks", Sport = "Hockey", Season = "2024" };
        db.Users.Add(user);
        db.Teams.Add(team);
        db.TeamMembers.Add(new TeamMember { UserId = userId, TeamId = team.Id, Role = TeamRole.ADMIN });
        await db.SaveChangesAsync();

        var handler = new GetTeamByIdQueryHandler(db);
        var result = await handler.Handle(new GetTeamByIdQuery(team.Id, userId), CancellationToken.None);

        Assert.Equal(team.Id, result.Id);
        Assert.Equal("Hawks", result.Name);
        Assert.Equal("Hockey", result.Sport);
        Assert.Single(result.Members);
        Assert.Equal("Admin", result.Members[0].Name);
    }

    [Fact]
    public async Task Handle_TeamWithPlayers_ReturnsCorrectPlayerCount()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "admin@test.com", PasswordHash = "hash" };
        var team = new Team { Name = "Hawks", Sport = "Hockey" };
        db.Users.Add(user);
        db.Teams.Add(team);
        db.TeamMembers.Add(new TeamMember { UserId = userId, TeamId = team.Id, Role = TeamRole.ADMIN });
        db.Players.AddRange(
            new Player { Name = "Player 1", TeamId = team.Id },
            new Player { Name = "Player 2", TeamId = team.Id });
        await db.SaveChangesAsync();

        var handler = new GetTeamByIdQueryHandler(db);
        var result = await handler.Handle(new GetTeamByIdQuery(team.Id, userId), CancellationToken.None);

        Assert.Equal(2, result.PlayerCount);
    }
}
