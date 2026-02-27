using Daibitx.AspNetCore.Utils.Models;
using XinMenu.DTOs;

namespace XinMenu.Services.Abstractions;

public interface IUserService
{
    Task<OperateResult<UserProfileDto>> GetProfileAsync(int userId);
    Task<OperateResult<UserProfileDto>> UpdateProfileAsync(int userId, UpdateUserProfileRequest request);
    Task<OperateResult<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<OperateResult<string>> UploadAvatarAsync(int userId, Stream fileStream, string fileName);
}
