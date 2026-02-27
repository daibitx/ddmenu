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
public class RecipesController : ControllerBase
{
    private readonly IRecipeService _recipeService;
    private readonly ILogger<RecipesController> _logger;

    public RecipesController(IRecipeService recipeService, ILogger<RecipesController> logger)
    {
        _recipeService = recipeService;
        _logger = logger;
    }

    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

    [HttpGet]
    public async Task<OperateResult<PagedList<RecipeListItemDto>>> GetList([FromQuery] RecipeQueryRequest request)
    {
        return await _recipeService.QueryAsync(request);
    }

    [HttpGet("{id:int}")]
    public async Task<OperateResult<RecipeDetailDto>> GetDetail(int id)
    {
        return await _recipeService.GetByIdAsync(id);
    }

    [HttpPost]
    [Authorize(Roles = nameof(RoleDefinetion.Admin))]
    public async Task<OperateResult<RecipeDetailDto>> Create([FromBody] CreateRecipeRequest request)
    {
        var userId = CurrentUserId;
        return await _recipeService.CreateAsync(request, userId);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = nameof(RoleDefinetion.Admin))]
    public async Task<OperateResult<RecipeDetailDto>> Update(int id, [FromBody] UpdateRecipeRequest request)
    {
        return await _recipeService.UpdateAsync(id, request);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = nameof(RoleDefinetion.Admin))]
    public async Task<OperateResult<bool>> Delete(int id)
    {
        return await _recipeService.DeleteAsync(id);
    }

    [HttpPost("{id:int}/favorite")]
    public async Task<OperateResult<bool>> ToggleFavorite(int id, [FromBody] FavoriteRequest request)
    {
        var userId = CurrentUserId;
        return await _recipeService.ToggleFavoriteAsync(id, userId, request.IsFavorite);
    }
}
