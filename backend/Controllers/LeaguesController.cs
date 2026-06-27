using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayLeague.Api.Features.Admin.Queries;
using PlayLeague.Api.Features.Communication.Commands;
using PlayLeague.Api.Features.Communication.Queries;
using PlayLeague.Api.Features.Leagues.Commands;
using PlayLeague.Api.Features.Leagues.Queries;

namespace PlayLeague.Api.Controllers;

[ApiController]
[Route("api/leagues")]
[Authorize]
public class LeaguesController(IMediator mediator) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpPost]
    public async Task<IActionResult> CreateLeague(CreateLeagueCommand cmd)
    {
        var id = await mediator.Send(cmd with { UserId = UserId });
        return Ok(id);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetLeague(Guid id)
    {
        var result = await mediator.Send(new GetLeagueQuery(id, UserId));
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateLeague(Guid id, UpdateLeagueCommand cmd)
    {
        await mediator.Send(cmd with { LeagueId = id, UserId = UserId });
        return Ok();
    }

    [HttpGet("{id:guid}/teams")]
    public async Task<IActionResult> GetLeagueTeams(Guid id)
    {
        var result = await mediator.Send(new GetLeagueTeamsQuery(id, UserId));
        return Ok(result);
    }

    [HttpPost("{id:guid}/teams")]
    public async Task<IActionResult> AddTeamToLeague(Guid id, AddTeamToLeagueCommand cmd)
    {
        await mediator.Send(cmd with { LeagueId = id, UserId = UserId });
        return Ok();
    }

    [HttpPost("{id:guid}/divisions")]
    public async Task<IActionResult> CreateDivision(Guid id, CreateDivisionCommand cmd)
    {
        var divisionId = await mediator.Send(cmd with { LeagueId = id, UserId = UserId });
        return Ok(divisionId);
    }

    [HttpPost("{id:guid}/messages")]
    public async Task<IActionResult> SendMessage(Guid id, SendMessageCommand cmd)
    {
        var messageId = await mediator.Send(cmd with { LeagueId = id, UserId = UserId });
        return Ok(messageId);
    }

    [HttpGet("{id:guid}/messages")]
    public async Task<IActionResult> GetMessages(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await mediator.Send(new GetMessagesQuery(id, UserId, page, pageSize));
        return Ok(result);
    }

    [HttpGet("{id:guid}/audit")]
    public async Task<IActionResult> GetAuditLogs(
        Guid id,
        [FromQuery] string? action,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 50)
    {
        var result = await mediator.Send(new GetAuditLogsQuery(id, action, page, limit, UserId));
        return Ok(result);
    }
}
