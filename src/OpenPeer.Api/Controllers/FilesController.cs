using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using OpenPeer.Application.Interfaces;
using OpenPeer.Domain.Enums;

namespace OpenPeer.Api.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _fileStorage;
    private readonly IPaperRepository _paperRepository;

    public FilesController(IFileStorageService fileStorage, IPaperRepository paperRepository)
    {
        _fileStorage = fileStorage;
        _paperRepository = paperRepository;
    }

    [HttpGet("papers/{fileName}")]
    public async Task<IActionResult> GetPaperFile(string fileName)
    {
        var paperId = Guid.Parse(Path.GetFileNameWithoutExtension(fileName));
        var paper = await _paperRepository.GetByIdAsync(paperId);

        if (paper is null || paper.IsDeleted)
            return NotFound();

        if (paper.Status == PaperStatus.Retracted)
        {
            var userId = GetUserIdOrNull();
            if (userId is null || userId.Value != paper.AuthorId)
                return NotFound();
        }

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Papers", fileName);
        var stream = await _fileStorage.GetFileAsync(filePath);

        return stream is null ? NotFound() : File(stream, "application/pdf");
    }

    [HttpGet("avatars/{fileName}")]
    public IActionResult GetAvatar(string fileName)
    {
        var avatarsDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Avatars");
        var filePath = Path.Combine(avatarsDir, fileName);

        if (!System.IO.File.Exists(filePath))
        {
            var dir = new DirectoryInfo(avatarsDir);
            if (!dir.Exists) return NotFound();
            var found = dir.EnumerateFiles()
                .FirstOrDefault(f => f.Name.Equals(fileName, StringComparison.OrdinalIgnoreCase));
            if (found is null) return NotFound();
            filePath = found.FullName;
        }

        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        var contentType = ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };

        var stream = System.IO.File.OpenRead(filePath);
        return File(stream, contentType);
    }

    [HttpGet("data/{fileName}")]
    public async Task<IActionResult> GetDataFile(string fileName)
    {
        var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "SupportingData");
        var filePath = Path.Combine(dataDir, fileName);

        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var contentType = ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".csv" => "text/csv",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".json" => "application/json",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };

        var stream = System.IO.File.OpenRead(filePath);
        return File(stream, contentType, fileName);
    }

    private Guid? GetUserIdOrNull()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null ? Guid.Parse(claim.Value) : null;
    }
}
