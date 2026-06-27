using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Venues.Commands;

public record CreateVenueCommand(
    string Name,
    string? Address,
    string? City,
    string? State,
    string? ZipCode,
    string? Phone,
    string? Website,
    VenueVisibility Visibility,
    Guid? TeamId,
    Guid? LeagueId,
    Guid UserId
) : IRequest<Guid>;

public class CreateVenueCommandHandler(AppDbContext db) : IRequestHandler<CreateVenueCommand, Guid>
{
    public async Task<Guid> Handle(CreateVenueCommand cmd, CancellationToken ct)
    {
        // Verify authorization
        if (cmd.TeamId.HasValue)
        {
            var membership = await db.TeamMembers
                .FirstOrDefaultAsync(tm => tm.TeamId == cmd.TeamId && tm.UserId == cmd.UserId, ct);

            if (membership is null || membership.Role != TeamRole.ADMIN)
                throw new ForbiddenException("Only team admins can create venues for a team.");
        }
        else if (cmd.LeagueId.HasValue)
        {
            var leagueMembership = await db.LeagueUsers
                .FirstOrDefaultAsync(lu => lu.LeagueId == cmd.LeagueId && lu.UserId == cmd.UserId, ct);

            if (leagueMembership is null || leagueMembership.Role != LeagueRole.LEAGUE_ADMIN)
                throw new ForbiddenException("Only league admins can create venues for a league.");
        }
        else
        {
            throw new ForbiddenException("A TeamId or LeagueId must be provided.");
        }

        var slug = GenerateSlug(cmd.Name);

        var venue = new Venue
        {
            Name = cmd.Name,
            Address = cmd.Address,
            City = cmd.City,
            State = cmd.State,
            ZipCode = cmd.ZipCode,
            Phone = cmd.Phone,
            Website = cmd.Website,
            Visibility = cmd.Visibility,
            TeamId = cmd.TeamId,
            LeagueId = cmd.LeagueId,
            CreatedById = cmd.UserId,
            Slug = slug,
        };

        db.Venues.Add(venue);
        await db.SaveChangesAsync(ct);

        return venue.Id;
    }

    private static string GenerateSlug(string name)
    {
        var normalized = name.ToLowerInvariant()
            .Replace(" ", "-");

        // Keep only alphanumeric and hyphens
        var cleaned = new string(normalized
            .Where(c => char.IsLetterOrDigit(c) || c == '-')
            .ToArray());

        // Trim leading/trailing hyphens and collapse multiple hyphens
        while (cleaned.Contains("--"))
            cleaned = cleaned.Replace("--", "-");

        cleaned = cleaned.Trim('-');

        var suffix = Guid.NewGuid().ToString("N")[..8];
        return $"{cleaned}-{suffix}";
    }
}
