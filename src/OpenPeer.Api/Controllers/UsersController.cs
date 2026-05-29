using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenPeer.Application.DTOs.Common;
using OpenPeer.Application.DTOs.AiConfig;
using OpenPeer.Application.DTOs.Papers;
using OpenPeer.Application.DTOs.Users;
using OpenPeer.Application.Interfaces;

namespace OpenPeer.Api.Controllers;

[ApiController]
[Route("api/users")]
    public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAiConfigService _aiConfigService;

    public UsersController(IUserService userService, IAiConfigService aiConfigService)
    {
        _userService = userService;
        _aiConfigService = aiConfigService;
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = GetUserId();
            var profile = await _userService.GetProfileAsync(userId);
            return Ok(ApiResponse<UserProfileDto>.Success(profile));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Error(404, ex.Message));
        }
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var userId = GetUserId();
            await _userService.UpdateProfileAsync(userId, request);
            return Ok(ApiResponse.Success("个人资料已更新"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Error(404, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Error(400, ex.Message));
        }
    }

    [HttpPost("me/change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userId = GetUserId();
            await _userService.ChangePasswordAsync(userId, request);
            return Ok(ApiResponse.Success("密码已修改"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Error(400, ex.Message));
        }
    }

    [HttpGet("me/papers")]
    [Authorize]
    public async Task<IActionResult> GetMyPapers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var result = await _userService.GetMyPapersAsync(userId, page, pageSize);
        return Ok(ApiResponse<PagedResponse<PaperDto>>.Success(result));
    }

    [HttpGet("me/ratings")]
    [Authorize]
    public async Task<IActionResult> GetMyRatings([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var result = await _userService.GetMyRatingsAsync(userId, page, pageSize);
        return Ok(ApiResponse<PagedResponse<UserRatingItemDto>>.Success(result));
    }

    [HttpGet("me/comments")]
    [Authorize]
    public async Task<IActionResult> GetMyComments([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        var result = await _userService.GetMyCommentsAsync(userId, page, pageSize);
        return Ok(ApiResponse<PagedResponse<UserCommentItemDto>>.Success(result));
    }

    [HttpGet("admin/list")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> GetAdminUserList(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var result = await _userService.GetAdminUserListAsync(page, pageSize, search);
        return Ok(ApiResponse<PagedResponse<AdminUserItemDto>>.Success(result));
    }

    [HttpPut("admin/{id:guid}/role")]
    [Authorize(Policy = "RequireAdminRole")]
    public async Task<IActionResult> UpdateUserRole(Guid id, [FromBody] UpdateRoleRequest request)
    {
        await _userService.UpdateUserRoleAsync(id, request.Role);
        return Ok(ApiResponse<object>.Success(null!, "角色已更新"));
    }

    [HttpPost("me/avatar")]
    [Authorize]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> UploadAvatar(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse.Error(400, "请选择文件"));

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        if (!allowedTypes.Contains(file.ContentType))
            return BadRequest(ApiResponse.Error(400, "仅支持 jpg/png/webp/gif 格式"));

        if (file.Length > 2 * 1024 * 1024)
            return BadRequest(ApiResponse.Error(400, "头像文件不能超过 2MB"));

        var userId = GetUserId();
        await using var stream = file.OpenReadStream();
        var path = await _userService.UploadAvatarAsync(userId, stream, file.FileName);
        return Ok(ApiResponse<object>.Success(new { avatarPath = path }, "头像已更新"));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPublicProfile(Guid id)
    {
        var profile = await _userService.GetPublicProfileAsync(id);
        if (profile is null)
            return NotFound(ApiResponse.Error(404, "用户不存在"));

        return Ok(ApiResponse<UserProfileDto>.Success(profile));
    }

    [HttpGet("me/ai-config")]
    [Authorize]
    public async Task<IActionResult> GetAiConfig()
    {
        var userId = GetUserId();
        var config = await _aiConfigService.GetConfigAsync(userId);
        return Ok(ApiResponse<AiConfigDto>.Success(config));
    }

    [HttpPut("me/ai-config")]
    [Authorize]
    public async Task<IActionResult> UpdateAiConfig([FromBody] UpdateAiConfigRequest request)
    {
        try
        {
            var userId = GetUserId();
            await _aiConfigService.UpdateConfigAsync(userId, request);
            return Ok(ApiResponse.Success("AI 配置已更新"));
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
}
