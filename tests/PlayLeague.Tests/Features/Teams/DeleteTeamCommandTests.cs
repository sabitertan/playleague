using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Features.Teams.Commands;
using PlayLeague.Api.Models;
using PlayLeague.Tests.Helpers;

namespace PlayLeague.Tests.Features.Teams;

public class DeleteTeamCommandTests
{
    [Fact]
    public async Task Handle_NonMember_ThrowsForbiddenException()
    {
        await using var db = DbContextFactory.Create();
        var team = new Team { Name = "Hawks", Sport = "Hockey" };
        db.Teams.Add(team);
        await db.SaveChangesAsync();

        var handler = new DeleteTeamCommandHandler(db);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new DeleteTeamCommand(team.Id, Guid.NewGuid()), CancellationToken.None));
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

        var handler = new DeleteTeamCommandHandler(db);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new DeleteTeamCommand(team.Id, userId), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AdminMember_DeletesTeam()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();
        var team = new Team { Name = "Hawks", Sport = "Hockey" };
        db.Teams.Add(team);
        db.TeamMembers.Add(new TeamMember { UserId = userId, TeamId = team.Id, Role = TeamRole.ADMIN });
        await db.SaveChangesAsync();

        var handler = new DeleteTeamCommandHandler(db);
        await handler.Handle(new DeleteTeamCommand(team.Id, userId), CancellationToken.None);

        Assert.Null(await db.Teams.FindAsync(team.Id));
    }

    [Fact]
    public async Task Handle_AdminMemberOfNonExistentTeam_ThrowsNotFoundException()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();
        var missingTeamId = Guid.NewGuid();
        // Member row exists but the team entity does not
        db.TeamMembers.Add(new TeamMember { UserId = userId, TeamId = missingTeamId, Role = TeamRole.ADMIN });
        await db.SaveChangesAsync();

        var handler = new DeleteTeamCommandHandler(db);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new DeleteTeamCommand(missingTeamId, userId), CancellationToken.None));
    }
}
