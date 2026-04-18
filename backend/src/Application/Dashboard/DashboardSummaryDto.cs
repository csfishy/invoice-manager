using InvoiceManager.Domain.Bills;

namespace InvoiceManager.Application.Dashboard;

public sealed record DashboardSummaryDto(
    int TotalBills,
    int PendingBills,
    int OverdueBills,
    int PaidBills,
    decimal TotalAmount,
    decimal PendingAmount,
    decimal OverdueAmount,
    IReadOnlyCollection<BillTypeSummaryDto> ByType,
    IReadOnlyCollection<UpcomingDueBillDto> UpcomingDueBills);

public sealed record BillTypeSummaryDto(BillType Type, int Count, decimal Amount);

public sealed record UpcomingDueBillDto(
    Guid Id,
    string ReferenceNumber,
    string CustomerName,
    BillType Type,
    PaymentStatus PaymentStatus,
    DateOnly DueDate,
    decimal Amount);
