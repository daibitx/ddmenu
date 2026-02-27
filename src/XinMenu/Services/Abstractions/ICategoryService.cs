using Daibitx.AspNetCore.Utils.Models;
using XinMenu.DTOs;

namespace XinMenu.Services.Abstractions;

public interface ICategoryService
{
    Task<OperateResult<List<CategoryDto>>> GetAllAsync();
    Task<OperateResult<CategoryDto>> GetByIdAsync(int id);
    Task<OperateResult<CategoryDto>> CreateAsync(CreateCategoryRequest request, int userId);
    Task<OperateResult<CategoryDto>> UpdateAsync(int id, UpdateCategoryRequest request);
    Task<OperateResult<bool>> DeleteAsync(int id);
}
