namespace InvoiceManager.Application.Services;

public interface IFileStorageService
{
    Task<string> SaveAsync(string originalFileName, Stream content, CancellationToken cancellationToken = default);
    Task<Stream?> OpenReadAsync(string storedFileName, CancellationToken cancellationToken = default);
    Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default);
}
