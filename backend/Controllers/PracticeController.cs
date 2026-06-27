using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayLeague.Api.Features.PracticePlanner.Commands;
using PlayLeague.Api.Features.PracticePlanner.Queries;

namespace PlayLeague.Api.Controllers;

[ApiController]
[Route("api/practice")]
[Authorize]
public class PracticeController(IMediator mediator) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    // Sessions

    [HttpGet("sessions/team/{teamId:guid}")]
    public async Task<IActionResult> GetSessions(Guid teamId)
    {
        var result = await mediator.Send(new GetSessionsQuery(teamId, UserId));
        return Ok(result);
    }

    [HttpGet("sessions/{id:guid}")]
    public async Task<IActionResult> GetSessionById(Guid id)
    {
        var result = await mediator.Send(new GetSessionByIdQuery(id, UserId));
        return Ok(result);
    }

    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession(CreateSessionCommand cmd)
    {
        var id = await mediator.Send(cmd with { UserId = UserId });
        return Ok(id);
    }

    [HttpPut("sessions/{id:guid}")]
    public async Task<IActionResult> UpdateSession(Guid id, UpdateSessionCommand cmd)
    {
        await mediator.Send(cmd with { SessionId = id, UserId = UserId });
        return Ok();
    }

    [HttpDelete("sessions/{id:guid}")]
    public async Task<IActionResult> DeleteSession(Guid id)
    {
        await mediator.Send(new DeleteSessionCommand(id, UserId));
        return NoContent();
    }

    [HttpPost("sessions/{id:guid}/plays")]
    public async Task<IActionResult> AddPlayToSession(Guid id, AddPlayToSessionCommand cmd)
    {
        var sessionPlayId = await mediator.Send(cmd with { SessionId = id, UserId = UserId });
        return Ok(sessionPlayId);
    }

    // Plays

    [HttpGet("plays/team/{teamId:guid}")]
    public async Task<IActionResult> GetPlays(Guid teamId)
    {
        var result = await mediator.Send(new GetPlaysQuery(teamId, UserId));
        return Ok(result);
    }

    [HttpPost("plays")]
    public async Task<IActionResult> CreatePlay(CreatePlayCommand cmd)
    {
        var id = await mediator.Send(cmd with { UserId = UserId });
        return Ok(id);
    }

    [HttpPut("plays/{id:guid}")]
    public async Task<IActionResult> UpdatePlay(Guid id, UpdatePlayCommand cmd)
    {
        await mediator.Send(cmd with { PlayId = id, UserId = UserId });
        return Ok();
    }

    [HttpDelete("plays/{id:guid}")]
    public async Task<IActionResult> DeletePlay(Guid id)
    {
        await mediator.Send(new DeletePlayCommand(id, UserId));
        return NoContent();
    }
}
