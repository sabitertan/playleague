using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PlayLeague.Api.Features.Venues.Commands;
using PlayLeague.Api.Features.Venues.Queries;

namespace PlayLeague.Api.Controllers;

[ApiController]
[Route("api/venues")]
[Authorize]
public class VenuesController(IMediator mediator) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetVenues([FromQuery] Guid? teamId, [FromQuery] Guid? leagueId)
    {
        var result = await mediator.Send(new GetVenuesQuery(teamId, leagueId, UserId));
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateVenue(CreateVenueCommand cmd)
    {
        var id = await mediator.Send(cmd with { UserId = UserId });
        return Ok(id);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetVenueById(Guid id)
    {
        var result = await mediator.Send(new GetVenueByIdQuery(id, UserId));
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateVenue(Guid id, UpdateVenueCommand cmd)
    {
        await mediator.Send(cmd with { VenueId = id, UserId = UserId });
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteVenue(Guid id)
    {
        await mediator.Send(new DeleteVenueCommand(id, UserId));
        return NoContent();
    }

    [HttpPost("{id:guid}/surfaces")]
    public async Task<IActionResult> AddSurface(Guid id, AddSurfaceCommand cmd)
    {
        var surfaceId = await mediator.Send(cmd with { VenueId = id, UserId = UserId });
        return Ok(surfaceId);
    }

    [HttpGet("{id:guid}/surfaces/{surfaceId:guid}")]
    public async Task<IActionResult> GetSurfaceById(Guid id, Guid surfaceId)
    {
        var result = await mediator.Send(new GetSurfaceByIdQuery(surfaceId, UserId));
        return Ok(result);
    }

    [HttpPut("{id:guid}/surfaces/{surfaceId:guid}")]
    public async Task<IActionResult> UpdateSurface(Guid id, Guid surfaceId, UpdateSurfaceCommand cmd)
    {
        await mediator.Send(cmd with { SurfaceId = surfaceId, UserId = UserId });
        return Ok();
    }

    [HttpDelete("{id:guid}/surfaces/{surfaceId:guid}")]
    public async Task<IActionResult> DeleteSurface(Guid id, Guid surfaceId)
    {
        await mediator.Send(new DeleteSurfaceCommand(surfaceId, UserId));
        return NoContent();
    }
}
