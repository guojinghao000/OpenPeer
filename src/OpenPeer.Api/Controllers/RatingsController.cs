using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenPeer.Application.DTOs.Common;
using OpenPeer.Application.DTOs.Ratings;
using OpenPeer.Application.Interfaces;
using OpenPeer.Application.Validators;

namespace OpenPeer.Api.Controllers;

[ApiController]
[Route("api/papers/{paperId:guid}/ratings")]
public class RatingsController : ControllerBase
{
    private readonly IRatingService _ratingService;

    public RatingsController(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SubmitOrUpdate(Guid paperId, [FromBody] CreateRatingRequest request)
    {
        try
        {
            var validator = new CreateRatingRequestValidator();
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
            var result = await _ratingService.SubmitOrUpdateAsync(paperId, userId, request.Score);
            return Ok(ApiResponse<RatingDto>.Success(result, "评分成功"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Error(404, ex.Message));
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetList(Guid paperId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _ratingService.GetListAsync(paperId, page, pageSize);
        return Ok(ApiResponse<RatingListResponse>.Success(result));
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> Delete(Guid paperId)
    {
        try
        {
            var userId = GetUserId();
            await _ratingService.DeleteAsync(paperId, userId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Error(404, ex.Message));
        }
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return Guid.Parse(claim!.Value);
    }
}
