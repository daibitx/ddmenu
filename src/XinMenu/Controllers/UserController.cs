using Daibitx.AspNetCore.Utils.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XinMenu.DTOs;
using XinMenu.Services.Abstractions;

namespace XinMenu.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    [HttpGet("profile")]
    public async Task<OperateResult<UserProfileDto>> GetProfile()
    {
        var userId = CurrentUserId;
        return await _userService.GetProfileAsync(userId);
    }

    [HttpPut("profile")]
    public async Task<OperateResult<UserProfileDto>> UpdateProfile([FromBody] UpdateUserProfileRequest request)
    {
        var userId = CurrentUserId;
        return await _userService.UpdateProfileAsync(userId, request);
    }

    [HttpPut("password")]
    public async Task<OperateResult<bool>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = CurrentUserId;
        return await _userService.ChangePasswordAsync(userId, request);
    }

    [HttpPost("avatar")]
    public async Task<OperateResult<string>> UploadAvatar(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return OperateResult<string>.Fail("请选择要上传的文件");
        }

        // 检查文件大小 (限制为5MB)
        if (file.Length > 5 * 1024 * 1024)
        {
            return OperateResult<string>.Fail("文件大小不能超过5MB");
        }

        var userId = CurrentUserId;
        using var stream = file.OpenReadStream();
        return await _userService.UploadAvatarAsync(userId, stream, file.FileName);
    }
}
