using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayLeague.Api.Features.Admin.Commands;
using PlayLeague.Api.Features.Admin.Queries;

namespace PlayLeague.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController(IMediator mediator) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("users/pending")]
    public async Task<IActionResult> GetPendingUsers()
    {
        var result = await mediator.Send(new GetPendingUsersQuery(UserId));
        return Ok(result);
    }

    [HttpPost("users/{id:guid}/approve")]
    public async Task<IActionResult> ApproveUser(Guid id)
    {
        await mediator.Send(new ApproveUserCommand(id, UserId));
        return Ok();
    }

    [HttpPost("users/{id:guid}/reject")]
    public async Task<IActionResult> RejectUser(Guid id)
    {
        await mediator.Send(new RejectUserCommand(id, UserId));
        return Ok();
    }
}
