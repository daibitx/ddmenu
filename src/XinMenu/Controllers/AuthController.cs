using Daibitx.AspNetCore.Utils.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XinMenu.DTOs;
using XinMenu.Services.Abstractions;

namespace XinMenu.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<OperateResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserName) || string.IsNullOrWhiteSpace(request.Password))
        {
            return OperateResult<LoginResponse>.Fail("用户名和密码不能为空");
        }

        return await _authService.LoginAsync(request);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<OperateResult<LoginResponse>> Refresh()
    {
        return await _authService.RefreshTokenAsync();
    }


}
