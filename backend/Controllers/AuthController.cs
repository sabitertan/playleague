using MediatR;
using Microsoft.AspNetCore.Mvc;
using PlayLeague.Api.Features.Auth.Commands;

namespace PlayLeague.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand cmd)
    {
        await mediator.Send(cmd);
        return Ok(new { message = "Account created. Awaiting admin approval." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginCommand cmd)
    {
        var result = await mediator.Send(cmd);
        return Ok(result);
    }
}
