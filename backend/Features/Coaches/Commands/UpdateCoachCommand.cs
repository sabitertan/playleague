using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Coaches.Commands;

public record UpdateCoachCommand(
    Guid CoachId,
    string Name,
    string? Title,
    string? Email,
    string? Phone,
    Guid UserId) : IRequest;

public class UpdateCoachCommandHandler(AppDbContext db) : IRequestHandler<UpdateCoachCommand>
{
    public async Task Handle(UpdateCoachCommand cmd, CancellationToken ct)
    {
        var coach = await db.Coaches.FirstOrDefaultAsync(c => c.Id == cmd.CoachId, ct);

        if (coach is null)
            throw new NotFoundException($"Coach {cmd.CoachId} not found.");

        if (coach.CreatedById != cmd.UserId)
            throw new ForbiddenException("You can only edit coaches you created.");

        coach.Name = cmd.Name;
        coach.Title = cmd.Title;
        coach.Email = cmd.Email;
        coach.Phone = cmd.Phone;

        await db.SaveChangesAsync(ct);
    }
}
