using Daibitx.AspNetCore.Utils.Models;
using Microsoft.EntityFrameworkCore;
using XinMenu.Data;
using XinMenu.DTOs;
using XinMenu.Entitys;
using XinMenu.Services.Abstractions;

namespace XinMenu.Services.Inplementations;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(AppDbContext context, ILogger<CategoryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OperateResult<List<CategoryDto>>> GetAllAsync()
    {
        var categories = await _context.RecipeCategories
            .AsNoTracking()
            .OrderBy(c => c.SortOrder)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Icon = c.Icon,
                SortOrder = c.SortOrder
            })
            .ToListAsync();

        return OperateResult<List<CategoryDto>>.Succeed(categories);
    }

    public async Task<OperateResult<CategoryDto>> GetByIdAsync(int id)
    {
        var category = await _context.RecipeCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return OperateResult<CategoryDto>.Fail("分类不存在");
        }

        var dto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Icon = category.Icon,
            SortOrder = category.SortOrder
        };

        return OperateResult<CategoryDto>.Succeed(dto);
    }

    public async Task<OperateResult<CategoryDto>> CreateAsync(CreateCategoryRequest request, int userId)
    {
        var category = new RecipeCategory
        {
            Name = request.Name,
            Icon = request.Icon,
            SortOrder = request.SortOrder,
            CreatedBy = userId
        };

        _context.RecipeCategories.Add(category);
        await _context.SaveChangesAsync();

        var dto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Icon = category.Icon,
            SortOrder = category.SortOrder
        };

        return OperateResult<CategoryDto>.Succeed(dto, "创建成功");
    }

    public async Task<OperateResult<CategoryDto>> UpdateAsync(int id, UpdateCategoryRequest request)
    {
        var category = await _context.RecipeCategories.FindAsync(id);
        if (category == null)
        {
            return OperateResult<CategoryDto>.Fail("分类不存在");
        }

        category.Name = request.Name;
        category.Icon = request.Icon;
        category.SortOrder = request.SortOrder;

        await _context.SaveChangesAsync();

        var dto = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Icon = category.Icon,
            SortOrder = category.SortOrder
        };

        return OperateResult<CategoryDto>.Succeed(dto, "更新成功");
    }

    public async Task<OperateResult<bool>> DeleteAsync(int id)
    {
        var category = await _context.RecipeCategories.FindAsync(id);
        if (category == null)
        {
            return OperateResult<bool>.Fail("分类不存在");
        }

        // 检查是否有关联的菜谱
        var hasRecipes = await _context.Recipes.AnyAsync(r => r.CategoryId == id);
        if (hasRecipes)
        {
            return OperateResult<bool>.Fail("该分类下存在菜谱，无法删除");
        }

        _context.RecipeCategories.Remove(category);
        await _context.SaveChangesAsync();

        return OperateResult<bool>.Succeed(true, "删除成功");
    }
}
