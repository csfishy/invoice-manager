namespace InvoiceManager.Infrastructure.Configuration;

public sealed class AttachmentOptions
{
    public const string SectionName = "Attachments";

    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;
    public string AllowedExtensions { get; set; } = ".pdf,.png,.jpg,.jpeg,.gif,.bmp,.webp,.tif,.tiff";
}
