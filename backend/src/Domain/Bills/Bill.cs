namespace InvoiceManager.Domain.Bills;

public sealed class Bill
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ReferenceNumber { get; set; } = string.Empty;
    public BillType Type { get; set; }
    public BillStatus Status { get; set; } = BillStatus.Draft;
    public decimal Amount { get; set; }
    public DateOnly IssueDate { get; set; }
    public DateOnly DueDate { get; set; }
}
