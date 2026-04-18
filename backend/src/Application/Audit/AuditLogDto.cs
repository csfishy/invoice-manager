namespace InvoiceManager.Application.Audit;

public sealed record AuditLogDto(
    Guid Id,
    DateTime OccurredAtUtc,
    string Username,
    string Action,
    string EntityType,
    string EntityId,
    string Summary,
    string MetadataJson);
