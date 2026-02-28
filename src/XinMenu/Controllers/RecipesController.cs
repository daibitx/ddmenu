using Daibitx.AspNetCore.Utils.Models;
using Daibitx.Identity.Core.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using XinMenu.DTOs;
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
    [RAMAuthorize("Recipe", "Read")]
    public async Task<OperateResult<PagedList<RecipeListItemDto>>> GetList([FromQuery] RecipeQueryRequest request)
    {
        return await _recipeService.QueryAsync(request);
    }

    [HttpGet("{id:int}")]
    [RAMAuthorize("Recipe", "ReadDetail")]
    public async Task<OperateResult<RecipeDetailDto>> GetDetail(int id)
    {
        return await _recipeService.GetByIdAsync(id);
    }

    [HttpPost]
    [RAMAuthorize("Recipe", "Create")]
    public async Task<OperateResult<RecipeDetailDto>> Create([FromBody] CreateRecipeRequest request)
    {
        var userId = CurrentUserId;
        return await _recipeService.CreateAsync(request, userId);
    }

    [HttpPut("{id:int}")]
    [RAMAuthorize("Recipe", "Update")]
    public async Task<OperateResult<RecipeDetailDto>> Update(int id, [FromBody] UpdateRecipeRequest request)
    {
        return await _recipeService.UpdateAsync(id, request);
    }

    [HttpDelete("{id:int}")]
    [RAMAuthorize("Recipe", "Delete")]
    public async Task<OperateResult<bool>> Delete(int id)
    {
        return await _recipeService.DeleteAsync(id);
    }

    [HttpPost("{id:int}/favorite")]
    [RAMAuthorize("Recipe", "Favorite")]
    public async Task<OperateResult<bool>> ToggleFavorite(int id, [FromBody] FavoriteRequest request)
    {
        var userId = CurrentUserId;
        return await _recipeService.ToggleFavoriteAsync(id, userId, request.IsFavorite);
    }
}
