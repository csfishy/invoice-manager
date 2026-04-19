using InvoiceManager.Domain.Bills;

namespace InvoiceManager.Application.Bills;

public sealed record ReminderRuleDto(
    Guid Id,
    string Name,
    Guid? BillCategoryId,
    string? BillCategoryName,
    BillType? BillType,
    int DaysBeforeDue,
    string Recipient,
    string Channel,
    bool IsEnabled,
    DateTime CreatedAtUtc);

public sealed record CreateReminderRuleRequestDto(
    string Name,
    Guid? BillCategoryId,
    BillType? BillType,
    int DaysBeforeDue,
    string Recipient,
    string Channel,
    bool IsEnabled);

public sealed record UpdateReminderRuleRequestDto(
    string Name,
    Guid? BillCategoryId,
    BillType? BillType,
    int DaysBeforeDue,
    string Recipient,
    string Channel,
    bool IsEnabled);

public sealed record BillReportSummaryDto(
    int BillCount,
    int PaidCount,
    int UnpaidCount,
    int OverdueCount,
    decimal TotalAmount,
    decimal UnpaidAmount,
    decimal OverdueAmount,
    IReadOnlyCollection<BillListItemDto> Items);
