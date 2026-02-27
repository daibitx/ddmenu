using Daibitx.AspNetCore.Utils.Models;
using XinMenu.DTOs;

namespace XinMenu.Services.Abstractions;

public interface IIngredientService
{
    Task<OperateResult<List<IngredientGroupDto>>> GetGroupedAsync(IngredientQueryRequest request);
    Task<OperateResult<IngredientDto>> GetByIdAsync(int id);
    Task<OperateResult<IngredientDto>> CreateAsync(CreateIngredientRequest request, int userId);
    Task<OperateResult<IngredientDto>> UpdateAsync(int id, UpdateIngredientRequest request);
    Task<OperateResult<bool>> DeleteAsync(int id);
}
