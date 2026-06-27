using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayLeague.Api.Features.Invitations.Commands;
using PlayLeague.Api.Features.Invitations.Queries;

namespace PlayLeague.Api.Controllers;

[ApiController]
[Route("api/invitations")]
[Authorize]
public class InvitationsController(IMediator mediator) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("team/{teamId:guid}")]
    public async Task<IActionResult> GetInvitations(Guid teamId)
    {
        var result = await mediator.Send(new GetInvitationsQuery(teamId, UserId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> SendInvitation(SendInvitationCommand cmd)
    {
        await mediator.Send(cmd with { UserId = UserId });
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> CancelInvitation(Guid id)
    {
        await mediator.Send(new CancelInvitationCommand(id, UserId));
        return NoContent();
    }

    [HttpPost("accept/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> AcceptInvitation(string token)
    {
        // UserId may be Guid.Empty for unauthenticated callers; handler should validate accordingly
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userId = userIdClaim is not null ? Guid.Parse(userIdClaim) : Guid.Empty;

        await mediator.Send(new AcceptInvitationCommand(token, userId));
        return Ok();
    }
}
