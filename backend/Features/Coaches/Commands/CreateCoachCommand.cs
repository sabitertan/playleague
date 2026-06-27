using MediatR;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Coaches.Commands;

public record CreateCoachCommand(
    string Name,
    string? Title,
    string? Email,
    string? Phone,
    Guid UserId) : IRequest<Guid>;

public class CreateCoachCommandHandler(AppDbContext db) : IRequestHandler<CreateCoachCommand, Guid>
{
    public async Task<Guid> Handle(CreateCoachCommand cmd, CancellationToken ct)
    {
        var coach = new Coach
        {
            Name = cmd.Name,
            Title = cmd.Title,
            Email = cmd.Email,
            Phone = cmd.Phone,
            CreatedById = cmd.UserId,
        };

        db.Coaches.Add(coach);
        await db.SaveChangesAsync(ct);

        return coach.Id;
    }
}
