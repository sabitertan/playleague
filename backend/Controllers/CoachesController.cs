using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayLeague.Api.Features.Coaches.Commands;
using PlayLeague.Api.Features.Coaches.Queries;

namespace PlayLeague.Api.Controllers;

[ApiController]
[Route("api/coaches")]
[Authorize]
public class CoachesController(IMediator mediator) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetMyCoaches()
    {
        var result = await mediator.Send(new GetMyCoachesQuery(UserId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCoach(CreateCoachCommand cmd)
    {
        var id = await mediator.Send(cmd with { UserId = UserId });
        return Ok(id);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCoach(Guid id, UpdateCoachCommand cmd)
    {
        await mediator.Send(cmd with { CoachId = id, UserId = UserId });
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCoach(Guid id)
    {
        await mediator.Send(new DeleteCoachCommand(id, UserId));
        return NoContent();
    }
}
