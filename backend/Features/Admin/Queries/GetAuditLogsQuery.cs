using MediatR;
using Microsoft.EntityFrameworkCore;
using PlayLeague.Api.Data;
using PlayLeague.Api.Models;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Features.Admin.Queries;

public record AuditLogDto(
    Guid Id,
    string Action,
    string? ResourceType,
    string? ResourceId,
    string? Details,
    AuditSeverity Severity,
    DateTime CreatedAt,
    string? UserEmail);

public record AuditLogPageDto(List<AuditLogDto> Items, int Total);

public record GetAuditLogsQuery(
    Guid? LeagueId,
    string? Action,
    int Page,
    int Limit,
    Guid UserId) : IRequest<AuditLogPageDto>;

public class GetAuditLogsQueryHandler(AppDbContext db) : IRequestHandler<GetAuditLogsQuery, AuditLogPageDto>
{
    public async Task<AuditLogPageDto> Handle(GetAuditLogsQuery query, CancellationToken ct)
    {
        var limit = Math.Min(query.Limit, 100);
        var page = Math.Max(query.Page, 1);

        var logsQuery = db.AuditLogs
            .Include(al => al.User)
            .AsQueryable();

        if (query.LeagueId.HasValue)
            logsQuery = logsQuery.Where(al => al.LeagueId == query.LeagueId.Value);

        if (!string.IsNullOrWhiteSpace(query.Action))
            logsQuery = logsQuery.Where(al => al.Action == query.Action);

        var total = await logsQuery.CountAsync(ct);

        var items = await logsQuery
            .OrderByDescending(al => al.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(al => new AuditLogDto(
                al.Id,
                al.Action,
                al.ResourceType,
                al.ResourceId,
                al.Details,
                al.Severity,
                al.CreatedAt,
                al.User != null ? al.User.Email : null))
            .ToListAsync(ct);

        return new AuditLogPageDto(items, total);
    }
}
