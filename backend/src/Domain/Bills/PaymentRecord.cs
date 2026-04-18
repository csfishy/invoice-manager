namespace InvoiceManager.Domain.Bills;

public sealed class PaymentRecord
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid BillId { get; set; }
    public decimal AmountPaid { get; set; }
    public DateOnly PaidOn { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public Guid RecordedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Bill? Bill { get; set; }
}
