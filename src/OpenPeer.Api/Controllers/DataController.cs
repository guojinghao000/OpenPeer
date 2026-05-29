using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OpenPeer.Application.DTOs.Common;
using OpenPeer.Application.DTOs.SupportingData;
using OpenPeer.Application.Interfaces;

namespace OpenPeer.Api.Controllers;

[ApiController]
[Route("api/papers/{paperId:guid}/data")]
public class DataController : ControllerBase
{
    private readonly ISupportingDataService _dataService;

    public DataController(ISupportingDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpPost]
    [Authorize]
    [EnableRateLimiting("Upload")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> Upload(Guid paperId, IFormFile file, [FromForm] string? description)
    {
        try
        {
            if (file is null || file.Length == 0)
                return BadRequest(ApiResponse.Error(400, "请选择文件"));

            var userId = GetUserId();
            await using var stream = file.OpenReadStream();
            var result = await _dataService.UploadAsync(paperId, userId, stream, file.FileName, file.ContentType, file.Length, description);

            return CreatedAtAction(nameof(GetList), new { paperId },
                ApiResponse<SupportingDataDto>.Success(201, "支撑数据上传成功", result));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Error(400, ex.Message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Error(404, ex.Message));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetList(Guid paperId)
    {
        var result = await _dataService.GetListAsync(paperId);
        return Ok(ApiResponse<List<SupportingDataDto>>.Success(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid paperId, Guid id)
    {
        try
        {
            var userId = GetUserId();
            await _dataService.DeleteAsync(paperId, id, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Error(404, ex.Message));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return Guid.Parse(claim!.Value);
    }
}
