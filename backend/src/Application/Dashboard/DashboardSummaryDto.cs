using InvoiceManager.Domain.Bills;

namespace InvoiceManager.Application.Dashboard;

public sealed record DashboardSummaryDto(
    int TotalBills,
    int PendingBills,
    int OverdueBills,
    int PaidBills,
    int UnpaidBills,
    int BillsDueThisWeek,
    decimal TotalAmount,
    decimal PendingAmount,
    decimal OverdueAmount,
    decimal TotalUnpaidAmount,
    IReadOnlyCollection<BillTypeSummaryDto> ByType,
    IReadOnlyCollection<UpcomingDueBillDto> UpcomingDueBills,
    IReadOnlyCollection<UpcomingDueBillDto> DueSoonBills,
    IReadOnlyCollection<UpcomingDueBillDto> OverdueBillList,
    IReadOnlyCollection<LatestUploadedBillDto> LatestUploadedBills,
    StorageUsageSummaryDto StorageUsage);

public sealed record BillTypeSummaryDto(BillType Type, int Count, decimal Amount);

public sealed record UpcomingDueBillDto(
    Guid Id,
    string ReferenceNumber,
    string CustomerName,
    BillType Type,
    PaymentStatus PaymentStatus,
    DateOnly DueDate,
    decimal Amount);

public sealed record LatestUploadedBillDto(
    Guid BillId,
    string ReferenceNumber,
    string CustomerName,
    string LatestAttachmentName,
    DateTime UploadedAtUtc,
    int AttachmentCount);

public sealed record StorageUsageSummaryDto(
    long AttachmentFileCount,
    long AttachmentBytes,
    string StoragePath);
