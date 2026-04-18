using System.Text.Json;
using InvoiceManager.Application.Audit;
using InvoiceManager.Application.Common;
using InvoiceManager.Application.Services;
using InvoiceManager.Domain.Audit;
using InvoiceManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Infrastructure.Services;

public sealed class AuditLogService(InvoiceManagerDbContext dbContext) : IAuditLogService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task LogAsync(
        Guid? userId,
        string username,
        string action,
        string entityType,
        string entityId,
        string summary,
        object? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var log = new AuditLog
        {
            UserId = userId,
            Username = username,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            Summary = summary,
            MetadataJson = metadata is null ? "{}" : JsonSerializer.Serialize(metadata, JsonOptions)
        };

        dbContext.AuditLogs.Add(log);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = dbContext.AuditLogs
            .AsNoTracking()
            .OrderByDescending(x => x.OccurredAtUtc);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AuditLogDto(
                x.Id,
                x.OccurredAtUtc,
                x.Username,
                x.Action,
                x.EntityType,
                x.EntityId,
                x.Summary,
                x.MetadataJson))
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogDto>(items, totalCount, page, pageSize);
    }
}
