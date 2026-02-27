using Daibitx.AspNetCore.Utils.Models;
using Daibitx.Identity.Core.Interfaces;
using Daibitx.Identity.Core.Stores;
using Microsoft.EntityFrameworkCore;
using XinMenu.Data;
using XinMenu.DTOs;
using XinMenu.Entitys;
using XinMenu.Services.Abstractions;

namespace XinMenu.Services.Inplementations;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IDxUserStore<User, Role> _userStore;
    private readonly IDxRoleStore<User, Role> _roleStore;
    private readonly IDxPasswordHasher<User> _passwordHasher;
    private readonly ILogger<UserService> _logger;
    private readonly IWebHostEnvironment _environment;

    public UserService(
        AppDbContext context,
        IDxUserStore<User, Role> userStore,
        IDxRoleStore<User, Role> roleStore,
        IDxPasswordHasher<User> passwordHasher,
        ILogger<UserService> logger,
        IWebHostEnvironment environment)
    {
        _context = context;
        _userStore = userStore;
        _roleStore = roleStore;
        _passwordHasher = passwordHasher;
        _logger = logger;
        _environment = environment;
    }

    public async Task<OperateResult<UserProfileDto>> GetProfileAsync(int userId)
    {
        var user = await _userStore.FindByIdAsync(userId);
        if (user == null)
        {
            return OperateResult<UserProfileDto>.Fail("用户不存在");
        }

        // 获取用户角色
        var roles = await _roleStore.GetRolesByUserIdAsync(userId);
        var roleName = roles.FirstOrDefault()?.Description ?? "用户";

        var dto = new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName,
            AvatarUrl = user.AvaterUrl,
            Role = roleName
        };

        return OperateResult<UserProfileDto>.Succeed(dto);
    }

    public async Task<OperateResult<UserProfileDto>> UpdateProfileAsync(int userId, UpdateUserProfileRequest request)
    {
        var user = await _userStore.FindByIdAsync(userId);
        if (user == null)
        {
            return OperateResult<UserProfileDto>.Fail("用户不存在");
        }

        user.AvaterUrl = request.AvatarUrl;
        await _userStore.UpdateAsync(user);

        // 获取用户角色
        var roles = await _roleStore.GetRolesByUserIdAsync(userId);
        var roleName = roles.FirstOrDefault()?.Description ?? "用户";

        var dto = new UserProfileDto
        {
            Id = user.Id,
            UserName = user.UserName,
            AvatarUrl = user.AvaterUrl,
            Role = roleName
        };

        return OperateResult<UserProfileDto>.Succeed(dto, "更新成功");
    }

    public async Task<OperateResult<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword))
        {
            return OperateResult<bool>.Fail("当前密码不能为空");
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return OperateResult<bool>.Fail("新密码不能为空");
        }

        if (request.NewPassword.Length < 6)
        {
            return OperateResult<bool>.Fail("新密码长度不能少于6位");
        }

        var user = await _userStore.FindByIdAsync(userId);
        if (user == null)
        {
            return OperateResult<bool>.Fail("用户不存在");
        }

        // 验证当前密码
        if (!_passwordHasher.VerifyPassword(user, user.PasswordHash, request.CurrentPassword))
        {
            return OperateResult<bool>.Fail("当前密码不正确");
        }

        // 更新密码
        user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
        await _userStore.UpdateAsync(user);

        return OperateResult<bool>.Succeed(true, "密码修改成功");
    }

    public async Task<OperateResult<string>> UploadAvatarAsync(int userId, Stream fileStream, string fileName)
    {
        var user = await _userStore.FindByIdAsync(userId);
        if (user == null)
        {
            return OperateResult<string>.Fail("用户不存在");
        }

        // 验证文件类型
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        if (!allowedExtensions.Contains(extension))
        {
            return OperateResult<string>.Fail("不支持的文件类型，仅支持 jpg、jpeg、png、gif、webp");
        }

        // 创建上传目录
        var uploadPath = Path.Combine(_environment.WebRootPath, "uploads", "avatars");
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        // 生成唯一文件名
        var uniqueFileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        var filePath = Path.Combine(uploadPath, uniqueFileName);

        // 删除旧头像文件
        if (!string.IsNullOrEmpty(user.AvaterUrl))
        {
            var oldFileName = Path.GetFileName(user.AvaterUrl);
            var oldFilePath = Path.Combine(uploadPath, oldFileName);
            if (File.Exists(oldFilePath))
            {
                File.Delete(oldFilePath);
            }
        }

        // 保存新文件
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(stream);
        }

        // 生成URL
        var avatarUrl = $"/uploads/avatars/{uniqueFileName}";

        // 更新用户头像URL
        user.AvaterUrl = avatarUrl;
        await _userStore.UpdateAsync(user);

        return OperateResult<string>.Succeed(avatarUrl, "上传成功");
    }
}
