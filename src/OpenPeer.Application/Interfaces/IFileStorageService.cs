namespace OpenPeer.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string subDirectory);
    Task<Stream?> GetFileAsync(string filePath);
    Task DeleteFileAsync(string filePath);
}
