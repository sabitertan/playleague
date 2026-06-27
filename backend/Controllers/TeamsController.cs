using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayLeague.Api.Features.Teams.Commands;
using PlayLeague.Api.Features.Teams.Queries;
using PlayLeague.Api.Features.Roster.Commands;
using PlayLeague.Api.Features.Roster.Queries;

namespace PlayLeague.Api.Controllers;

[ApiController]
[Route("api/teams")]
[Authorize]
public class TeamsController(IMediator mediator) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetTeams()
    {
        var result = await mediator.Send(new GetTeamsQuery(UserId));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTeamById(Guid id)
    {
        var result = await mediator.Send(new GetTeamByIdQuery(id, UserId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateTeam(CreateTeamCommand cmd)
    {
        var id = await mediator.Send(cmd with { CreatedByUserId = UserId });
        return Ok(id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateTeam(Guid id, UpdateTeamCommand cmd)
    {
        await mediator.Send(cmd with { TeamId = id, UserId = UserId });
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTeam(Guid id)
    {
        await mediator.Send(new DeleteTeamCommand(id, UserId));
        return NoContent();
    }

    [HttpGet("{id:guid}/roster")]
    public async Task<IActionResult> GetRoster(Guid id)
    {
        var result = await mediator.Send(new GetRosterQuery(id, UserId));
        return Ok(result);
    }

    [HttpPost("{id:guid}/roster")]
    public async Task<IActionResult> AddPlayer(Guid id, AddPlayerCommand cmd)
    {
        var playerId = await mediator.Send(cmd with { TeamId = id, UserId = UserId });
        return Ok(playerId);
    }

    [HttpPut("{id:guid}/roster/{playerId:guid}")]
    public async Task<IActionResult> UpdatePlayer(Guid id, Guid playerId, UpdatePlayerCommand cmd)
    {
        await mediator.Send(cmd with { PlayerId = playerId, TeamId = id, UserId = UserId });
        return Ok();
    }

    [HttpGet("{id:guid}/roster/{playerId:guid}")]
    public async Task<IActionResult> GetPlayerById(Guid id, Guid playerId)
    {
        var result = await mediator.Send(new GetPlayerByIdQuery(playerId, UserId));
        return Ok(result);
    }

    [HttpDelete("{id:guid}/roster/{playerId:guid}")]
    public async Task<IActionResult> RemovePlayer(Guid id, Guid playerId)
    {
        await mediator.Send(new RemovePlayerCommand(playerId, id, UserId));
        return NoContent();
    }
}
