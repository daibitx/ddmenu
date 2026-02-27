using Daibitx.AspNetCore.Utils.Models;
using XinMenu.DTOs;

namespace XinMenu.Services.Abstractions;

public interface IFoodLogService
{
    Task<OperateResult<FoodLogDayDto>> GetByDateAsync(int userId, string date);
    Task<OperateResult<FoodLogCalendarDto>> GetCalendarAsync(int userId, int year, int month);
    Task<OperateResult<FoodLogItemDto>> CreateAsync(int userId, CreateFoodLogRequest request);
    Task<OperateResult<FoodLogItemDto>> UpdateAsync(int id, int userId, UpdateFoodLogRequest request);
    Task<OperateResult<bool>> DeleteAsync(int id, int userId);
}
