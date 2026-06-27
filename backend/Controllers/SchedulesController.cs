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

    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> PublishSchedule(Guid id)
    {
        await mediator.Send(new PublishScheduleCommand(id, UserId));
        return Ok();
    }
}
