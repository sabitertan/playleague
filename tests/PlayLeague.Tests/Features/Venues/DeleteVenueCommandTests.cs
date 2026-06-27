using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Features.Venues.Commands;
using PlayLeague.Api.Models;
using PlayLeague.Tests.Helpers;

namespace PlayLeague.Tests.Features.Venues;

public class DeleteVenueCommandTests
{
    [Fact]
    public async Task Handle_NonExistentVenue_ThrowsNotFoundException()
    {
        await using var db = DbContextFactory.Create();
        var handler = new DeleteVenueCommandHandler(db);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new DeleteVenueCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UnauthorizedUser_ThrowsForbiddenException()
    {
        await using var db = DbContextFactory.Create();
        var creatorId = Guid.NewGuid();
        var venue = new Venue { Name = "Ice Arena", CreatedById = creatorId, TeamId = null, LeagueId = null };
        db.Venues.Add(venue);
        await db.SaveChangesAsync();

        var handler = new DeleteVenueCommandHandler(db);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new DeleteVenueCommand(venue.Id, Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_VenueCreator_DeletesVenue()
    {
        await using var db = DbContextFactory.Create();
        var creatorId = Guid.NewGuid();
        var venue = new Venue { Name = "Ice Arena", CreatedById = creatorId };
        db.Venues.Add(venue);
        await db.SaveChangesAsync();

        var handler = new DeleteVenueCommandHandler(db);
        await handler.Handle(new DeleteVenueCommand(venue.Id, creatorId), CancellationToken.None);

        Assert.Null(await db.Venues.FindAsync(venue.Id));
    }

    [Fact]
    public async Task Handle_TeamAdminForVenueTeam_DeletesVenue()
    {
        await using var db = DbContextFactory.Create();
        var adminId = Guid.NewGuid();
        var team = new Team { Name = "Hawks", Sport = "Hockey" };
        db.Teams.Add(team);
        var venue = new Venue { Name = "Ice Arena", CreatedById = Guid.NewGuid(), TeamId = team.Id };
        db.Venues.Add(venue);
        db.TeamMembers.Add(new TeamMember { UserId = adminId, TeamId = team.Id, Role = TeamRole.ADMIN });
        await db.SaveChangesAsync();

        var handler = new DeleteVenueCommandHandler(db);
        await handler.Handle(new DeleteVenueCommand(venue.Id, adminId), CancellationToken.None);

        Assert.Null(await db.Venues.FindAsync(venue.Id));
    }
}
