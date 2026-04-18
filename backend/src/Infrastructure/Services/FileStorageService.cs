using InvoiceManager.Application.Services;
using InvoiceManager.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace InvoiceManager.Infrastructure.Services;

public sealed class FileStorageService(IOptions<StorageOptions> options) : IFileStorageService
{
    private readonly string _rootPath = options.Value.Path;

    public async Task<string> SaveAsync(string originalFileName, Stream content, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(_rootPath);

        var extension = Path.GetExtension(originalFileName);
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var destinationPath = Path.Combine(_rootPath, storedFileName);

        await using var fileStream = File.Create(destinationPath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return storedFileName;
    }

    public Task<Stream?> OpenReadAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_rootPath, storedFileName);
        if (!File.Exists(path))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = File.OpenRead(path);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string storedFileName, CancellationToken cancellationToken = default)
    {
        var path = Path.Combine(_rootPath, storedFileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return Task.CompletedTask;
    }
}
