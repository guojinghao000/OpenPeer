using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenPeer.Application.DTOs.Common;
using OpenPeer.Application.DTOs.Users;
using OpenPeer.Application.Interfaces;

namespace OpenPeer.Api.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
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

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetPublicProfile(Guid id)
    {
        var profile = await _userService.GetPublicProfileAsync(id);
        if (profile is null)
            return NotFound(ApiResponse.Error(404, "用户不存在"));

        return Ok(ApiResponse<UserProfileDto>.Success(profile));
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return Guid.Parse(claim!.Value);
    }
}
