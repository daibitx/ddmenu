using Daibitx.AspNetCore.Utils.Models;
using Microsoft.EntityFrameworkCore;
using XinMenu.Data;
using XinMenu.DTOs;
using XinMenu.Entitys;
using XinMenu.Services.Abstractions;

namespace XinMenu.Services.Inplementations;

public class FoodLogService : IFoodLogService
{
    private readonly AppDbContext _context;
    private readonly ILogger<FoodLogService> _logger;

    public FoodLogService(AppDbContext context, ILogger<FoodLogService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OperateResult<FoodLogDayDto>> GetByDateAsync(int userId, string date)
    {
        if (!DateTime.TryParse(date, out var targetDate))
        {
            return OperateResult<FoodLogDayDto>.Fail("日期格式不正确");
        }

        var logs = await _context.FoodLogs
            .AsNoTracking()
            .Where(fl => fl.UserId == userId && fl.Date.Date == targetDate.Date)
            .OrderBy(fl => fl.Time)
            .ToListAsync();

        var grouped = logs
            .GroupBy(fl => fl.MealType)
            .Select(g => new FoodLogGroupDto
            {
                MealType = g.Key,
                Items = g.Select(fl => new FoodLogItemDto
                {
                    Id = fl.Id,
                    Time = fl.Time.ToString(@"hh\:mm"),
                    MealType = fl.MealType,
                    Name = fl.Name,
                    RecipeId = fl.RecipeId,
                    CreatedAt = fl.CreatedAt
                }).ToList()
            })
            .ToList();

        var dto = new FoodLogDayDto
        {
            Date = date,
            Items = grouped
        };

        return OperateResult<FoodLogDayDto>.Succeed(dto);
    }

    public async Task<OperateResult<FoodLogCalendarDto>> GetCalendarAsync(int userId, int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var logs = await _context.FoodLogs
            .AsNoTracking()
            .Where(fl => fl.UserId == userId && fl.Date >= startDate && fl.Date <= endDate)
            .GroupBy(fl => fl.Date.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var data = logs.ToDictionary(
            x => x.Date.ToString("yyyy-MM-dd"),
            x => x.Count
        );

        var dto = new FoodLogCalendarDto { Data = data };
        return OperateResult<FoodLogCalendarDto>.Succeed(dto);
    }

    public async Task<OperateResult<FoodLogItemDto>> CreateAsync(int userId, CreateFoodLogRequest request)
    {
        if (!DateTime.TryParse(request.Date, out var date))
        {
            return OperateResult<FoodLogItemDto>.Fail("日期格式不正确");
        }

        if (!TimeSpan.TryParse(request.Time, out var time))
        {
            return OperateResult<FoodLogItemDto>.Fail("时间格式不正确");
        }

        // 验证菜谱ID是否存在
        if (request.RecipeId.HasValue)
        {
            var recipeExists = await _context.Recipes.AnyAsync(r => r.Id == request.RecipeId.Value);
            if (!recipeExists)
            {
                return OperateResult<FoodLogItemDto>.Fail("指定的菜谱不存在");
            }
        }

        var foodLog = new FoodLog
        {
            UserId = userId,
            Date = date,
            Time = time,
            MealType = request.MealType,
            Name = request.Name,
            RecipeId = request.RecipeId,
            CreatedAt = DateTime.UtcNow
        };

        _context.FoodLogs.Add(foodLog);
        await _context.SaveChangesAsync();

        var dto = new FoodLogItemDto
        {
            Id = foodLog.Id,
            Time = foodLog.Time.ToString(@"hh\:mm"),
            MealType = foodLog.MealType,
            Name = foodLog.Name,
            RecipeId = foodLog.RecipeId,
            CreatedAt = foodLog.CreatedAt
        };

        return OperateResult<FoodLogItemDto>.Succeed(dto, "创建成功");
    }

    public async Task<OperateResult<FoodLogItemDto>> UpdateAsync(int id, int userId, UpdateFoodLogRequest request)
    {
        var foodLog = await _context.FoodLogs.FirstOrDefaultAsync(fl => fl.Id == id && fl.UserId == userId);
        if (foodLog == null)
        {
            return OperateResult<FoodLogItemDto>.Fail("记录不存在");
        }

        if (!DateTime.TryParse(request.Date, out var date))
        {
            return OperateResult<FoodLogItemDto>.Fail("日期格式不正确");
        }

        if (!TimeSpan.TryParse(request.Time, out var time))
        {
            return OperateResult<FoodLogItemDto>.Fail("时间格式不正确");
        }

        // 验证菜谱ID是否存在
        if (request.RecipeId.HasValue)
        {
            var recipeExists = await _context.Recipes.AnyAsync(r => r.Id == request.RecipeId.Value);
            if (!recipeExists)
            {
                return OperateResult<FoodLogItemDto>.Fail("指定的菜谱不存在");
            }
        }

        foodLog.Date = date;
        foodLog.Time = time;
        foodLog.MealType = request.MealType;
        foodLog.Name = request.Name;
        foodLog.RecipeId = request.RecipeId;

        await _context.SaveChangesAsync();

        var dto = new FoodLogItemDto
        {
            Id = foodLog.Id,
            Time = foodLog.Time.ToString(@"hh\:mm"),
            MealType = foodLog.MealType,
            Name = foodLog.Name,
            RecipeId = foodLog.RecipeId,
            CreatedAt = foodLog.CreatedAt
        };

        return OperateResult<FoodLogItemDto>.Succeed(dto, "更新成功");
    }

    public async Task<OperateResult<bool>> DeleteAsync(int id, int userId)
    {
        var foodLog = await _context.FoodLogs.FirstOrDefaultAsync(fl => fl.Id == id && fl.UserId == userId);
        if (foodLog == null)
        {
            return OperateResult<bool>.Fail("记录不存在");
        }

        _context.FoodLogs.Remove(foodLog);
        await _context.SaveChangesAsync();

        return OperateResult<bool>.Succeed(true, "删除成功");
    }
}
