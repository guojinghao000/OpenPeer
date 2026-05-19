using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenPeer.Application.DTOs.Comments;
using OpenPeer.Application.DTOs.Common;
using OpenPeer.Application.Interfaces;
using OpenPeer.Application.Validators;

namespace OpenPeer.Api.Controllers;

[ApiController]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _commentService;

    public CommentsController(ICommentService commentService)
    {
        _commentService = commentService;
    }

    [HttpGet("api/papers/{paperId:guid}/comments")]
    public async Task<IActionResult> GetList(Guid paperId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var items = await _commentService.GetListAsync(paperId, page, pageSize);
        return Ok(ApiResponse<PagedResponse<CommentDto>>.Success(items));
    }

    [HttpPost("api/papers/{paperId:guid}/comments")]
    [Authorize]
    public async Task<IActionResult> Create(Guid paperId, [FromBody] CreateCommentRequest request)
    {
        try
        {
            var validator = new CreateCommentRequestValidator();
            await validator.ValidateAndThrowAsync(request);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Errors.Select(e => new ValidationError
            {
                Field = e.PropertyName,
                Message = e.ErrorMessage
            }).ToList();
            return BadRequest(ApiResponse.Error(400, "参数校验失败", errors));
        }

        try
        {
            var userId = GetUserId();
            var result = await _commentService.CreateAsync(paperId, userId, request.Content, request.ParentId);
            return CreatedAtAction(nameof(GetList), new { paperId },
                ApiResponse<CommentDto>.Success(201, "评论发表成功", result));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Error(404, ex.Message));
        }
    }

    [HttpPut("api/comments/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCommentRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _commentService.UpdateAsync(id, userId, request.Content);
            return Ok(ApiResponse<CommentDto>.Success(result, "评论更新成功"));
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

    [HttpDelete("api/comments/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var userId = GetUserId();
            await _commentService.DeleteAsync(id, userId);
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
