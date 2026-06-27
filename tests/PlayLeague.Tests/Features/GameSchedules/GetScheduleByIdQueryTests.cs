using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Features.GameSchedules.Queries;
using PlayLeague.Api.Models;
using PlayLeague.Tests.Helpers;

namespace PlayLeague.Tests.Features.GameSchedules;

public class GetScheduleByIdQueryTests
{
    [Fact]
    public async Task Handle_NonExistentSchedule_ThrowsNotFoundException()
    {
        await using var db = DbContextFactory.Create();
        var handler = new GetScheduleByIdQueryHandler(db);

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new GetScheduleByIdQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NonMember_ThrowsForbiddenException()
    {
        await using var db = DbContextFactory.Create();
        var team = new Team { Name = "Hawks", Sport = "Hockey" };
        db.Teams.Add(team);
        var schedule = new GameSchedule { Name = "Season 2024", TeamId = team.Id, CreatedById = Guid.NewGuid() };
        db.GameSchedules.Add(schedule);
        await db.SaveChangesAsync();

        var handler = new GetScheduleByIdQueryHandler(db);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new GetScheduleByIdQuery(schedule.Id, Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_TeamMember_ReturnsScheduleDetail()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();
        var team = new Team { Name = "Hawks", Sport = "Hockey" };
        db.Teams.Add(team);
        db.TeamMembers.Add(new TeamMember { UserId = userId, TeamId = team.Id, Role = TeamRole.MEMBER });
        var schedule = new GameSchedule
        {
            Name = "Season 2024",
            TeamId = team.Id,
            CreatedById = userId,
            Status = ScheduleStatus.DRAFT,
            RoundRobin = true,
            Rounds = 3
        };
        db.GameSchedules.Add(schedule);
        await db.SaveChangesAsync();

        var handler = new GetScheduleByIdQueryHandler(db);
        var result = await handler.Handle(new GetScheduleByIdQuery(schedule.Id, userId), CancellationToken.None);

        Assert.Equal(schedule.Id, result.Id);
        Assert.Equal("Season 2024", result.Name);
        Assert.Equal(ScheduleStatus.DRAFT, result.Status);
        Assert.True(result.RoundRobin);
        Assert.Equal(3, result.Rounds);
        Assert.Equal(team.Id, result.TeamId);
    }

    [Fact]
    public async Task Handle_LeagueMember_ReturnsScheduleDetail()
    {
        await using var db = DbContextFactory.Create();
        var userId = Guid.NewGuid();
        var league = new League { Name = "City League", Sport = "Hockey" };
        db.Leagues.Add(league);
        db.LeagueUsers.Add(new LeagueUser { UserId = userId, LeagueId = league.Id, Role = LeagueRole.MEMBER });
        var schedule = new GameSchedule { Name = "League Schedule", LeagueId = league.Id, CreatedById = userId };
        db.GameSchedules.Add(schedule);
        await db.SaveChangesAsync();

        var handler = new GetScheduleByIdQueryHandler(db);
        var result = await handler.Handle(new GetScheduleByIdQuery(schedule.Id, userId), CancellationToken.None);

        Assert.Equal(league.Id, result.LeagueId);
    }
}
