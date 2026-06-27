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

    [HttpGet]
    public async Task<IActionResult> GetLeagues()
    {
        var result = await mediator.Send(new GetLeaguesQuery(UserId));
        return Ok(result);
    }

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

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteLeague(Guid id)
    {
        await mediator.Send(new DeleteLeagueCommand(id, UserId));
        return NoContent();
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

    [HttpGet("{id:guid}/divisions")]
    public async Task<IActionResult> GetDivisions(Guid id)
    {
        var result = await mediator.Send(new GetDivisionsQuery(id, UserId));
        return Ok(result);
    }

    [HttpPost("{id:guid}/divisions")]
    public async Task<IActionResult> CreateDivision(Guid id, CreateDivisionCommand cmd)
    {
        var divisionId = await mediator.Send(cmd with { LeagueId = id, UserId = UserId });
        return Ok(divisionId);
    }

    [HttpGet("{id:guid}/divisions/{divisionId:guid}")]
    public async Task<IActionResult> GetDivisionById(Guid id, Guid divisionId)
    {
        var result = await mediator.Send(new GetDivisionByIdQuery(divisionId, UserId));
        return Ok(result);
    }

    [HttpPut("{id:guid}/divisions/{divisionId:guid}")]
    public async Task<IActionResult> UpdateDivision(Guid id, Guid divisionId, UpdateDivisionCommand cmd)
    {
        await mediator.Send(cmd with { DivisionId = divisionId, UserId = UserId });
        return Ok();
    }

    [HttpDelete("{id:guid}/divisions/{divisionId:guid}")]
    public async Task<IActionResult> DeleteDivision(Guid id, Guid divisionId)
    {
        await mediator.Send(new DeleteDivisionCommand(divisionId, UserId));
        return NoContent();
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

    [HttpGet("{id:guid}/messages/{messageId:guid}")]
    public async Task<IActionResult> GetMessageById(Guid id, Guid messageId)
    {
        var result = await mediator.Send(new GetMessageByIdQuery(id, messageId, UserId));
        return Ok(result);
    }

    [HttpPut("{id:guid}/messages/{messageId:guid}")]
    public async Task<IActionResult> UpdateMessage(Guid id, Guid messageId, UpdateMessageCommand cmd)
    {
        await mediator.Send(cmd with { LeagueId = id, MessageId = messageId, UserId = UserId });
        return Ok();
    }

    [HttpDelete("{id:guid}/messages/{messageId:guid}")]
    public async Task<IActionResult> DeleteMessage(Guid id, Guid messageId)
    {
        await mediator.Send(new DeleteMessageCommand(id, messageId, UserId));
        return NoContent();
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
