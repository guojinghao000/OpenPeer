using OpenPeer.Application.Interfaces;

namespace OpenPeer.Infrastructure.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService()
    {
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string subDirectory)
    {
        var dir = Path.Combine(_basePath, subDirectory);
        Directory.CreateDirectory(dir);
        var filePath = Path.Combine(dir, fileName);
        await using var output = File.Create(filePath);
        await fileStream.CopyToAsync(output);
        return filePath;
    }

    public Task<Stream?> GetFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return Task.FromResult<Stream?>(null);

        var stream = File.OpenRead(filePath);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteFileAsync(string filePath)
    {
        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }
}
