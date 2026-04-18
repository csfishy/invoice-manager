namespace InvoiceManager.Domain.Audit;

public sealed class AuditLog
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;
    public Guid? UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string MetadataJson { get; set; } = "{}";
}
