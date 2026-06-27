using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayLeague.Api.Features.Notifications.Commands;
using PlayLeague.Api.Features.Notifications.Queries;

namespace PlayLeague.Api.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(IMediator mediator) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences([FromQuery] Guid? leagueId)
    {
        var result = await mediator.Send(new GetNotificationPreferencesQuery(UserId, leagueId));
        return Ok(result);
    }

    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences(UpdateNotificationPreferencesCommand cmd)
    {
        await mediator.Send(cmd with { UserId = UserId });
        return Ok();
    }
}
