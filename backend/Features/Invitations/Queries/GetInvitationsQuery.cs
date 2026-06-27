using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Exceptions;
using PlayLeague.Api.Models;

namespace PlayLeague.Api.Features.Invitations.Queries;

public record InvitationDto(Guid Id, string Email, InvitationStatus Status, DateTime ExpiresAt, DateTime CreatedAt);

public record GetInvitationsQuery(Guid TeamId, Guid UserId) : IRequest<List<InvitationDto>>;

public class GetInvitationsQueryHandler(AppDbContext db) : IRequestHandler<GetInvitationsQuery, List<InvitationDto>>
{
    public async Task<List<InvitationDto>> Handle(GetInvitationsQuery query, CancellationToken ct)
    {
        var membership = await db.TeamMembers
            .FirstOrDefaultAsync(tm => tm.TeamId == query.TeamId && tm.UserId == query.UserId, ct);

        if (membership is null || membership.Role != TeamRole.ADMIN)
            throw new ForbiddenException("Only team admins can view invitations.");

        return await db.Invitations
            .Where(i => i.TeamId == query.TeamId && i.Status == InvitationStatus.PENDING)
            .Select(i => new InvitationDto(i.Id, i.Email, i.Status, i.ExpiresAt, i.CreatedAt))
            .ToListAsync(ct);
    }
}
