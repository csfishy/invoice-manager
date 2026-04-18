using InvoiceManager.Domain.Bills;

namespace InvoiceManager.Application.Bills;

public sealed record BillListItemDto(
    Guid Id,
    string ReferenceNumber,
    BillType Type,
    string CategoryName,
    PaymentStatus PaymentStatus,
    string CustomerName,
    string PropertyName,
    string ProviderName,
    string AccountNumber,
    decimal Amount,
    string Currency,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    DateOnly IssueDate,
    DateOnly DueDate,
    DateOnly? PaidDate,
    int AttachmentCount,
    DateTime UpdatedAtUtc);

public sealed record BillDetailDto(
    Guid Id,
    string ReferenceNumber,
    BillType Type,
    Guid BillCategoryId,
    string CategoryName,
    PaymentStatus PaymentStatus,
    string CustomerName,
    string PropertyName,
    string ProviderName,
    string AccountNumber,
    decimal Amount,
    string Currency,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    DateOnly IssueDate,
    DateOnly DueDate,
    DateOnly? PaidDate,
    string Notes,
    string Keywords,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyCollection<BillAttachmentDto> Attachments);

public sealed record BillAttachmentDto(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long FileSize,
    DateTime UploadedAtUtc);

public sealed record CreateBillRequestDto(
    BillType Type,
    Guid BillCategoryId,
    PaymentStatus PaymentStatus,
    string ReferenceNumber,
    string CustomerName,
    string PropertyName,
    string ProviderName,
    string AccountNumber,
    decimal Amount,
    string Currency,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    DateOnly IssueDate,
    DateOnly DueDate,
    DateOnly? PaidDate,
    string Notes,
    string Keywords);

public sealed record UpdateBillRequestDto(
    BillType Type,
    Guid BillCategoryId,
    PaymentStatus PaymentStatus,
    string ReferenceNumber,
    string CustomerName,
    string PropertyName,
    string ProviderName,
    string AccountNumber,
    decimal Amount,
    string Currency,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    DateOnly IssueDate,
    DateOnly DueDate,
    DateOnly? PaidDate,
    string Notes,
    string Keywords);

public sealed record BillQueryDto(
    BillType? BillType,
    PaymentStatus? PaymentStatus,
    DateOnly? DueDateFrom,
    DateOnly? DueDateTo,
    DateOnly? PeriodFrom,
    DateOnly? PeriodTo,
    string? Customer,
    string? Keyword,
    int Page = 1,
    int PageSize = 20);

public sealed record BillCategoryDto(
    Guid Id,
    string Name,
    BillType Type,
    string Description,
    int SortOrder,
    bool IsActive,
    bool IsSystemDefault,
    DateTime CreatedAtUtc,
    int BillCount);

public sealed record CreateBillCategoryRequestDto(
    string Name,
    BillType Type,
    string Description,
    int SortOrder,
    bool IsActive,
    bool IsSystemDefault);

public sealed record UpdateBillCategoryRequestDto(
    string Name,
    BillType Type,
    string Description,
    int SortOrder,
    bool IsActive,
    bool IsSystemDefault);
