using PlayLeague.Api.Features.Teams.Commands;
using PlayLeague.Api.Models;
using PlayLeague.Tests.Helpers;

namespace PlayLeague.Tests.Features.Teams;

public class CreateTeamCommandTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesTeamWithAdminMembership()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreateTeamCommandHandler(db);
        var userId = Guid.NewGuid();

        var id = await handler.Handle(new CreateTeamCommand("Hawks", "Hockey", "2024", userId), CancellationToken.None);

        var team = await db.Teams.FindAsync(id);
        var member = await db.TeamMembers.FindAsync(userId, id);

        Assert.NotNull(team);
        Assert.Equal("Hawks", team.Name);
        Assert.Equal("Hockey", team.Sport);
        Assert.Equal("2024", team.Season);
        Assert.NotNull(member);
        Assert.Equal(TeamRole.ADMIN, member.Role);
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsNewNonEmptyId()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreateTeamCommandHandler(db);

        var id = await handler.Handle(new CreateTeamCommand("Eagles", "Baseball", null, Guid.NewGuid()), CancellationToken.None);

        Assert.NotEqual(Guid.Empty, id);
    }

    [Fact]
    public async Task Handle_MultipleTeams_EachGetsSeparateAdminMember()
    {
        await using var db = DbContextFactory.Create();
        var handler = new CreateTeamCommandHandler(db);
        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        var id1 = await handler.Handle(new CreateTeamCommand("Team A", "Soccer", null, user1), CancellationToken.None);
        var id2 = await handler.Handle(new CreateTeamCommand("Team B", "Soccer", null, user2), CancellationToken.None);

        Assert.NotEqual(id1, id2);
        Assert.NotNull(await db.TeamMembers.FindAsync(user1, id1));
        Assert.NotNull(await db.TeamMembers.FindAsync(user2, id2));
        Assert.Null(await db.TeamMembers.FindAsync(user1, id2));
    }
}
