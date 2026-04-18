using InvoiceManager.Application.Dashboard;

namespace InvoiceManager.Application.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}
