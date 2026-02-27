using Daibitx.AspNetCore.Utils.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XinMenu.DTOs;
using XinMenu.Models;
using XinMenu.Services.Abstractions;

namespace XinMenu.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IngredientsController : ControllerBase
{
    private readonly IIngredientService _ingredientService;
    private readonly ILogger<IngredientsController> _logger;

    public IngredientsController(IIngredientService ingredientService, ILogger<IngredientsController> logger)
    {
        _ingredientService = ingredientService;
        _logger = logger;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    [HttpGet]
    public async Task<OperateResult<List<IngredientGroupDto>>> GetList([FromQuery] IngredientQueryRequest request)
    {
        return await _ingredientService.GetGroupedAsync(request);
    }

    [HttpGet("{id:int}")]
    public async Task<OperateResult<IngredientDto>> GetDetail(int id)
    {
        return await _ingredientService.GetByIdAsync(id);
    }

    [HttpPost]
    [Authorize(Roles = nameof(RoleDefinetion.Admin))]
    public async Task<OperateResult<IngredientDto>> Create([FromBody] CreateIngredientRequest request)
    {
        var userId = CurrentUserId;
        return await _ingredientService.CreateAsync(request, userId);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(RoleDefinetion.Admin))]
    public async Task<OperateResult<IngredientDto>> Update(int id, [FromBody] UpdateIngredientRequest request)
    {
        return await _ingredientService.UpdateAsync(id, request);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(RoleDefinetion.Admin))]
    public async Task<OperateResult<bool>> Delete(int id)
    {
        return await _ingredientService.DeleteAsync(id);
    }
}
