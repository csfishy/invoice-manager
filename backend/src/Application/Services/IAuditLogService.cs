using InvoiceManager.Application.Audit;
using InvoiceManager.Application.Common;

namespace InvoiceManager.Application.Services;

public interface IAuditLogService
{
    Task LogAsync(Guid? userId, string username, string action, string entityType, string entityId, string summary, object? metadata = null, CancellationToken cancellationToken = default);
    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
