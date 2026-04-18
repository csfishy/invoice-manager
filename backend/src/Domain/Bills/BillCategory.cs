namespace InvoiceManager.Domain.Bills;

public sealed class BillCategory
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public BillType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSystemDefault { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<Bill> Bills { get; set; } = new();
    public List<ReminderRule> ReminderRules { get; set; } = new();
}
