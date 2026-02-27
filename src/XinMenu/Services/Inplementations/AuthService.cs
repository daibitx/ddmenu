using Daibitx.AspNetCore.Utils.Models;
using Daibitx.Identity.Authentication.Jwt;
using Daibitx.Identity.Core.Interfaces;
using Daibitx.Identity.Core.Stores;
using Microsoft.EntityFrameworkCore;
using XinMenu.Data;
using XinMenu.DTOs;
using XinMenu.Entitys;
using XinMenu.Services.Abstractions;

namespace XinMenu.Services.Inplementations;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuthService> _logger;
    private readonly IJwtService _jwtService;
    private readonly IDxUserStore<User, Role> _userStore;
    private readonly IDxRoleStore<User, Role> _roleStore;
    private readonly IDxPasswordHasher<User> _passwordHasher;
    private readonly IHttpContextAccessor httpContextAccessor;
    public AuthService(AppDbContext context, ILogger<AuthService> logger, IDxUserStore<User, Role> userStore, IDxRoleStore<User, Role> roleStore, IDxPermissionStore<User, Role> permissionStore, IDxPasswordHasher<User> passwordHasher)
    {
        _context = context;
        _logger = logger;
        _userStore = userStore;
        _roleStore = roleStore;
        _passwordHasher = passwordHasher;
    }

    public async Task<OperateResult<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userStore.FindByNameAsync(request.UserName);

        if (user == null)
            return OperateResult<LoginResponse>.Fail("未找到账号信息");

        if (!_passwordHasher.VerifyPassword(user, user.PasswordHash, request.Password))
            return OperateResult<LoginResponse>.Fail("密码错误");

        var roles = await _roleStore.GetRolesByUserIdAsync(user.Id);

        var token = _jwtService.GenerateToken(user.Id, user.UserName, roles.Select(p => p.Id).ToList());

        var userAgent = httpContextAccessor?.HttpContext?.Request.Headers.UserAgent.ToString() ?? "";

        var refreshToken = await _context.Set<UserRefreshToken>().Where(p => p.Id == user.Id && p.UserAgent == userAgent).FirstOrDefaultAsync();

        string currentRefreshToken;

        if (refreshToken == null)
        {
            refreshToken = new UserRefreshToken(userAgent ?? "", user.Id);
            currentRefreshToken = refreshToken.CreateRefreshToken();
            await _context.Set<UserRefreshToken>().AddAsync(refreshToken);
        }
        else
        {
            currentRefreshToken = refreshToken.CreateRefreshToken();
            _context.Set<UserRefreshToken>().Update(refreshToken);
        }
        await _context.SaveChangesAsync();

        httpContextAccessor?.HttpContext?.Response.Cookies.Append("refreshToken", currentRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth/refresh",
            Expires = DateTimeOffset.Now.AddDays(7)
        });
        return OperateResult<LoginResponse>.Succeed(new LoginResponse
        {
            Token = token,
            UserName = user.UserName,
            Role = roles.FirstOrDefault()?.Description ?? "",
        }, "登录成功");
    }
    public async Task<OperateResult<LoginResponse>> RefreshTokenAsync()
    {
        var refreshTokenValue = httpContextAccessor?.HttpContext?.Request.Cookies["refreshToken"];
        if (string.IsNullOrWhiteSpace(refreshTokenValue))
        {
            return OperateResult<LoginResponse>.Fail("Refresh Token 不存在");
        }
        var tokenHash = UserRefreshToken.GetTokenHash(refreshTokenValue);
        var storedToken = await _context.Set<UserRefreshToken>()
            .FirstOrDefaultAsync(p => p.TokenHash == refreshTokenValue);

        if (storedToken == null)
        {
            _logger.LogWarning("Refresh Token 不存在或已被吊销: {Token}",
                refreshTokenValue[..Math.Min(10, refreshTokenValue.Length)] + "...");
            return OperateResult<LoginResponse>.Fail("无效的 Refresh Token");
        }

        if (storedToken.ExpiresAt <= DateTime.UtcNow)
        {
            _context.Set<UserRefreshToken>().Remove(storedToken);
            await _context.SaveChangesAsync();
            return OperateResult<LoginResponse>.Fail("登录已过期，请重新登录");
        }

        var user = await _userStore.FindByIdAsync(storedToken.UserId);
        if (user == null)
        {
            return OperateResult<LoginResponse>.Fail("用户不存在");
        }

        var roles = await _roleStore.GetRolesByUserIdAsync(user.Id);

        var newAccessToken = _jwtService.GenerateToken(
            user.Id,
            user.UserName,
            roles.Select(p => p.Id).ToList()
        );

        var newRefreshToken = storedToken.CreateRefreshToken();

        storedToken.ExpiresAt = DateTime.UtcNow.AddDays(7);

        _context.Set<UserRefreshToken>().Update(storedToken);

        await _context.SaveChangesAsync();

        httpContextAccessor?.HttpContext?.Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth/refresh",
            Expires = DateTimeOffset.Now.AddDays(7)
        });

        return OperateResult<LoginResponse>.Succeed(new LoginResponse
        {
            Token = newAccessToken,
            UserName = user.UserName,
            Role = roles.FirstOrDefault()?.Description ?? "",
        }, "刷新成功");

    }
}
