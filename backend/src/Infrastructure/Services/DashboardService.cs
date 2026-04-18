using InvoiceManager.Application.Dashboard;
using InvoiceManager.Application.Services;
using InvoiceManager.Domain.Bills;
using InvoiceManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InvoiceManager.Infrastructure.Services;

public sealed class DashboardService(InvoiceManagerDbContext dbContext) : IDashboardService
{
    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var bills = await dbContext.Bills.AsNoTracking().ToListAsync(cancellationToken);

        var byType = bills
            .GroupBy(x => x.Type)
            .Select(group => new BillTypeSummaryDto(group.Key, group.Count(), group.Sum(x => x.Amount)))
            .OrderBy(x => x.Type)
            .ToList();

        var upcomingDue = bills
            .Where(x => x.PaymentStatus != PaymentStatus.Paid)
            .OrderBy(x => x.DueDate)
            .Take(5)
            .Select(x => new UpcomingDueBillDto(
                x.Id,
                x.ReferenceNumber,
                x.CustomerName,
                x.Type,
                x.PaymentStatus,
                x.DueDate,
                x.Amount))
            .ToList();

        return new DashboardSummaryDto(
            bills.Count,
            bills.Count(x => x.PaymentStatus == PaymentStatus.Pending),
            bills.Count(x => x.PaymentStatus == PaymentStatus.Overdue),
            bills.Count(x => x.PaymentStatus == PaymentStatus.Paid),
            bills.Sum(x => x.Amount),
            bills.Where(x => x.PaymentStatus == PaymentStatus.Pending).Sum(x => x.Amount),
            bills.Where(x => x.PaymentStatus == PaymentStatus.Overdue).Sum(x => x.Amount),
            byType,
            upcomingDue);
    }
}
