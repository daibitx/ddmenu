using Daibitx.AspNetCore.Utils.Models;
using XinMenu.DTOs;

namespace XinMenu.Services.Abstractions
{
    public interface IAuthService
    {
        Task<OperateResult<LoginResponse>> LoginAsync(LoginRequest request);

        Task<OperateResult<LoginResponse>> RefreshTokenAsync();
    }

}
