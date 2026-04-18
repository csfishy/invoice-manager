namespace InvoiceManager.Domain.Bills;

public sealed class Bill
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ReferenceNumber { get; set; } = string.Empty;
    public BillType Type { get; set; }
    public Guid BillCategoryId { get; set; }
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
    public string CustomerName { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TWD";
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly DueDate { get; set; }
    public DateOnly? PaidDate { get; set; }
    public string Notes { get; set; } = string.Empty;
    public string Keywords { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public Guid CreatedByUserId { get; set; }
    public Guid UpdatedByUserId { get; set; }

    public BillCategory? BillCategory { get; set; }
    public List<BillAttachment> Attachments { get; set; } = new();
    public List<PaymentRecord> PaymentRecords { get; set; } = new();
}
