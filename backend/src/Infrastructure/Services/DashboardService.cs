using InvoiceManager.Application.Dashboard;
using InvoiceManager.Application.Services;
using InvoiceManager.Domain.Bills;
using InvoiceManager.Infrastructure.Configuration;
using InvoiceManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace InvoiceManager.Infrastructure.Services;

public sealed class DashboardService(
    InvoiceManagerDbContext dbContext,
    IOptions<StorageOptions> storageOptions) : IDashboardService
{
    public async Task<DashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);
        var endOfWeek = today.AddDays(7);

        var bills = await dbContext.Bills
            .AsNoTracking()
            .Include(x => x.Attachments)
            .ToListAsync(cancellationToken);

        var byType = bills
            .GroupBy(x => x.Type)
            .Select(group => new BillTypeSummaryDto(group.Key, group.Count(), group.Sum(x => x.Amount)))
            .OrderBy(x => x.Type)
            .ToList();

        var dueSoon = bills
            .Where(x => x.PaymentStatus != PaymentStatus.Paid)
            .Where(x => x.DueDate >= today && x.DueDate <= endOfWeek)
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

        var overdue = bills
            .Where(x => x.PaymentStatus == PaymentStatus.Overdue || (x.PaymentStatus != PaymentStatus.Paid && x.DueDate < today))
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

        var latestUploads = bills
            .Where(x => x.Attachments.Count > 0)
            .Select(x => new
            {
                Bill = x,
                Latest = x.Attachments.OrderByDescending(a => a.UploadedAtUtc).First()
            })
            .OrderByDescending(x => x.Latest.UploadedAtUtc)
            .Take(5)
            .Select(x => new LatestUploadedBillDto(
                x.Bill.Id,
                x.Bill.ReferenceNumber,
                x.Bill.CustomerName,
                x.Latest.OriginalFileName,
                x.Latest.UploadedAtUtc,
                x.Bill.Attachments.Count))
            .ToList();

        var storagePath = storageOptions.Value.Path;
        var storageDirectory = new DirectoryInfo(storagePath);
        var fileCount = storageDirectory.Exists ? storageDirectory.EnumerateFiles("*", SearchOption.AllDirectories).LongCount() : 0L;
        var totalBytes = storageDirectory.Exists ? storageDirectory.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length) : 0L;

        return new DashboardSummaryDto(
            bills.Count,
            bills.Count(x => x.PaymentStatus == PaymentStatus.Pending),
            bills.Count(x => x.PaymentStatus == PaymentStatus.Overdue),
            bills.Count(x => x.PaymentStatus == PaymentStatus.Paid),
            bills.Count(x => x.PaymentStatus != PaymentStatus.Paid),
            bills.Count(x => x.PaymentStatus != PaymentStatus.Paid && x.DueDate >= today && x.DueDate <= endOfWeek),
            bills.Sum(x => x.Amount),
            bills.Where(x => x.PaymentStatus == PaymentStatus.Pending).Sum(x => x.Amount),
            bills.Where(x => x.PaymentStatus == PaymentStatus.Overdue).Sum(x => x.Amount),
            bills.Where(x => x.PaymentStatus != PaymentStatus.Paid).Sum(x => x.Amount),
            byType,
            dueSoon,
            dueSoon,
            overdue,
            latestUploads,
            new StorageUsageSummaryDto(fileCount, totalBytes, storagePath));
    }
}
