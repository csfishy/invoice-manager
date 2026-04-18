namespace InvoiceManager.Domain.Bills;

public sealed class ReminderRule
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public Guid? BillCategoryId { get; set; }
    public BillType? BillType { get; set; }
    public int DaysBeforeDue { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public string Channel { get; set; } = "InApp";
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public BillCategory? BillCategory { get; set; }
}
