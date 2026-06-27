using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayLeague.Api.Features.Events.Commands;
using PlayLeague.Api.Features.Events.Queries;
using PlayLeague.Api.Features.Rsvp.Commands;

namespace PlayLeague.Api.Controllers;

[ApiController]
[Route("api/events")]
[Authorize]
public class EventsController(IMediator mediator) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("team/{teamId:guid}")]
    public async Task<IActionResult> GetEvents(Guid teamId)
    {
        var result = await mediator.Send(new GetEventsQuery(teamId, UserId));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetEventById(Guid id)
    {
        var result = await mediator.Send(new GetEventByIdQuery(id, UserId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateEvent(CreateEventCommand cmd)
    {
        var id = await mediator.Send(cmd with { UserId = UserId });
        return Ok(id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateEvent(Guid id, UpdateEventCommand cmd)
    {
        await mediator.Send(cmd with { EventId = id, UserId = UserId });
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteEvent(Guid id)
    {
        await mediator.Send(new DeleteEventCommand(id, UserId));
        return NoContent();
    }

    [HttpPut("{id:guid}/rsvp")]
    public async Task<IActionResult> UpdateRsvp(Guid id, UpdateRsvpCommand cmd)
    {
        await mediator.Send(cmd with { EventId = id, UserId = UserId });
        return Ok();
    }
}
