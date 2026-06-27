using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Admin.Queries;

public record PendingUserDto(
    Guid Id,
    string Email,
    string? Name,
    DateTime CreatedAt);

public record GetPendingUsersQuery(Guid RequestingUserId) : IRequest<List<PendingUserDto>>;

public class GetPendingUsersQueryHandler(AppDbContext db) : IRequestHandler<GetPendingUsersQuery, List<PendingUserDto>>
{
    public async Task<List<PendingUserDto>> Handle(GetPendingUsersQuery query, CancellationToken ct)
    {
        var pendingUsers = await db.Users
            .Where(u => !u.Approved)
            .OrderBy(u => u.CreatedAt)
            .Select(u => new PendingUserDto(u.Id, u.Email, u.Name, u.CreatedAt))
            .ToListAsync(ct);

        return pendingUsers;
    }
}
