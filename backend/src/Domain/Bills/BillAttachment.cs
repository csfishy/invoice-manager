namespace InvoiceManager.Domain.Bills;

public sealed class BillAttachment
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid BillId { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public long FileSize { get; set; }
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
    public Guid UploadedByUserId { get; set; }

    public Bill? Bill { get; set; }
}
