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
public class FoodLogController : ControllerBase
{
    private readonly IFoodLogService _foodLogService;
    private readonly ILogger<FoodLogController> _logger;

    public FoodLogController(IFoodLogService foodLogService, ILogger<FoodLogController> logger)
    {
        _foodLogService = foodLogService;
        _logger = logger;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    [HttpGet]
    public async Task<OperateResult<FoodLogDayDto>> GetByDate([FromQuery] FoodLogQueryRequest request)
    {
        var userId = CurrentUserId;
        return await _foodLogService.GetByDateAsync(userId, request.Date);
    }

    [HttpGet("calendar")]
    public async Task<OperateResult<FoodLogCalendarDto>> GetCalendar([FromQuery] FoodLogCalendarQueryRequest request)
    {
        var userId = CurrentUserId;
        return await _foodLogService.GetCalendarAsync(userId, request.Year, request.Month);
    }

    [HttpPost]
    public async Task<OperateResult<FoodLogItemDto>> Create([FromBody] CreateFoodLogRequest request)
    {
        var userId = CurrentUserId;
        return await _foodLogService.CreateAsync(userId, request);
    }

    [HttpPut("{id:int}")]
    public async Task<OperateResult<FoodLogItemDto>> Update(int id, [FromBody] UpdateFoodLogRequest request)
    {
        var userId = CurrentUserId;
        return await _foodLogService.UpdateAsync(id, userId, request);
    }

    [HttpDelete("{id:int}")]
    public async Task<OperateResult<bool>> Delete(int id)
    {
        var userId = CurrentUserId;
        return await _foodLogService.DeleteAsync(id, userId);
    }
}
