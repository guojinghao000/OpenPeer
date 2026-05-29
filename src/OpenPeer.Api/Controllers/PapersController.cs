using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using OpenPeer.Application.DTOs.AiConfig;
using OpenPeer.Application.DTOs.Common;
using OpenPeer.Application.DTOs.Papers;
using OpenPeer.Application.Interfaces;

namespace OpenPeer.Api.Controllers;

[ApiController]
[Route("api/papers")]
public class PapersController : ControllerBase
{
    private readonly IPaperService _paperService;
    private readonly IAiPaperService _aiPaperService;

    public PapersController(IPaperService paperService, IAiPaperService aiPaperService)
    {
        _paperService = paperService;
        _aiPaperService = aiPaperService;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] PaperListRequest request)
    {
        var result = await _paperService.GetListAsync(request);
        return Ok(ApiResponse<PagedResponse<PaperDto>>.Success(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            var result = await _paperService.GetDetailAsync(id, currentUserId);
            return Ok(ApiResponse<PaperDetailDto>.Success(result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Error(404, ex.Message));
        }
    }

    [HttpPost]
    [Authorize]
    [EnableRateLimiting("Upload")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> Upload(
        [FromForm] string title,
        [FromForm] string @abstract,
        [FromForm] string? categoryIds,
        IFormFile file)
    {
        try
        {
            var authorId = GetUserId();

            var categoryGuidList = new List<Guid>();
            if (!string.IsNullOrWhiteSpace(categoryIds))
            {
                categoryGuidList = categoryIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(Guid.Parse)
                    .ToList();
            }

            var request = new CreatePaperRequest
            {
                Title = title,
                Abstract = @abstract,
                CategoryIds = categoryGuidList
            };

            using var stream = file.OpenReadStream();
            var result = await _paperService.CreateAsync(request, stream, file.FileName,
                file.ContentType, file.Length, authorId);

            return CreatedAtAction(nameof(GetDetail), new { id = result.Id },
                ApiResponse<PaperDto>.Success(201, "论文发布成功", result));
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

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePaperRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _paperService.UpdateAsync(id, request, userId);
            return Ok(ApiResponse<PaperDto>.Success(result, "论文更新成功"));
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

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = GetUserId();
            await _paperService.DeleteAsync(id, userId);
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

    [HttpPost("{id:guid}/retract")]
    [Authorize]
    public async Task<IActionResult> Retract(Guid id, [FromBody] RetractRequest request)
    {
        try
        {
            var userId = GetUserId();
            await _paperService.RetractAsync(id, request.Reason, userId);
            return Ok(ApiResponse.Success("论文已撤回"));
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

    [HttpPost("generate")]
    [Authorize]
    public async Task<IActionResult> GenerateLatex([FromBody] GenerateLatexRequest request)
    {
        try
        {
            var userId = GetUserId();
            var latex = await _aiPaperService.GenerateLatexAsync(userId, request.Title, request.DataIds, request.Prompt);
            return Ok(ApiResponse<object>.Success(new { latex }, "LaTeX 论文生成成功"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Error(400, ex.Message));
        }
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return Guid.Parse(claim!.Value);
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim is not null ? Guid.Parse(claim.Value) : null;
    }
}

public class RetractRequest
{
    public string Reason { get; set; } = string.Empty;
}
