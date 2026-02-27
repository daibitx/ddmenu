using Daibitx.AspNetCore.Utils.Models;
using XinMenu.DTOs;

namespace XinMenu.Services.Abstractions;

public interface IRecipeService
{
    Task<OperateResult<RecipeDetailDto>> GetByIdAsync(int id);
    Task<OperateResult<PagedList<RecipeListItemDto>>> QueryAsync(RecipeQueryRequest request);
    Task<OperateResult<RecipeDetailDto>> CreateAsync(CreateRecipeRequest request, int userId);
    Task<OperateResult<RecipeDetailDto>> UpdateAsync(int id, UpdateRecipeRequest request);
    Task<OperateResult<bool>> DeleteAsync(int id);
    Task<OperateResult<bool>> ToggleFavoriteAsync(int recipeId, int userId, bool isFavorite);
}
