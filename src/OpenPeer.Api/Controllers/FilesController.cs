using Microsoft.AspNetCore.Mvc;
using OpenPeer.Application.Interfaces;

namespace OpenPeer.Api.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private readonly IFileStorageService _fileStorage;

    public FilesController(IFileStorageService fileStorage)
    {
        _fileStorage = fileStorage;
    }

    [HttpGet("papers/{fileName}")]
    public async Task<IActionResult> GetPaperFile(string fileName)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "Papers", fileName);
        var stream = await _fileStorage.GetFileAsync(filePath);

        if (stream is null)
            return NotFound();

        return File(stream, "application/pdf", fileName);
    }
}
