using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayLeague.Api.Features.GameSchedules.Commands;
using PlayLeague.Api.Features.GameSchedules.Queries;

namespace PlayLeague.Api.Controllers;

[ApiController]
[Route("api/schedules")]
[Authorize]
public class SchedulesController(IMediator mediator) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetSchedules([FromQuery] Guid? teamId, [FromQuery] Guid? leagueId)
    {
        var result = await mediator.Send(new GetSchedulesQuery(teamId, leagueId, UserId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSchedule(CreateScheduleCommand cmd)
    {
        var id = await mediator.Send(cmd with { UserId = UserId });
        return Ok(id);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetScheduleById(Guid id)
    {
        var result = await mediator.Send(new GetScheduleByIdQuery(id, UserId));
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSchedule(Guid id, UpdateScheduleCommand cmd)
    {
        await mediator.Send(cmd with { ScheduleId = id, UserId = UserId });
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSchedule(Guid id)
    {
        await mediator.Send(new DeleteScheduleCommand(id, UserId));
        return NoContent();
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> PublishSchedule(Guid id)
    {
        await mediator.Send(new PublishScheduleCommand(id, UserId));
        return Ok();
    }
}
