using Daibitx.AspNetCore.Utils.Models;
using Microsoft.EntityFrameworkCore;
using XinMenu.Data;
using XinMenu.DTOs;
using XinMenu.Entitys;
using XinMenu.Services.Abstractions;

namespace XinMenu.Services.Inplementations;

public class IngredientService : IIngredientService
{
    private readonly AppDbContext _context;
    private readonly ILogger<IngredientService> _logger;

    public IngredientService(AppDbContext context, ILogger<IngredientService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OperateResult<List<IngredientGroupDto>>> GetGroupedAsync(IngredientQueryRequest request)
    {
        var query = _context.Ingredients.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            query = query.Where(i => i.Type == request.Type);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            query = query.Where(i => i.Name.Contains(request.Keyword));
        }

        var ingredients = await query
            .OrderBy(i => i.Type)
            .ThenBy(i => i.Name)
            .ToListAsync();

        var grouped = ingredients
            .GroupBy(i => i.Type)
            .Select(g => new IngredientGroupDto
            {
                Type = g.Key,
                Items = g.Select(i => new IngredientDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Type = i.Type,
                    Description = i.Description
                }).ToList()
            })
            .ToList();

        return OperateResult<List<IngredientGroupDto>>.Succeed(grouped);
    }

    public async Task<OperateResult<IngredientDto>> GetByIdAsync(int id)
    {
        var ingredient = await _context.Ingredients
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id);

        if (ingredient == null)
        {
            return OperateResult<IngredientDto>.Fail("原料不存在");
        }

        var dto = new IngredientDto
        {
            Id = ingredient.Id,
            Name = ingredient.Name,
            Type = ingredient.Type,
            Description = ingredient.Description
        };

        return OperateResult<IngredientDto>.Succeed(dto);
    }

    public async Task<OperateResult<IngredientDto>> CreateAsync(CreateIngredientRequest request, int userId)
    {
        var ingredient = new Ingredient
        {
            Name = request.Name,
            Type = request.Type,
            Description = request.Description,
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Ingredients.Add(ingredient);
        await _context.SaveChangesAsync();

        var dto = new IngredientDto
        {
            Id = ingredient.Id,
            Name = ingredient.Name,
            Type = ingredient.Type,
            Description = ingredient.Description
        };

        return OperateResult<IngredientDto>.Succeed(dto, "创建成功");
    }

    public async Task<OperateResult<IngredientDto>> UpdateAsync(int id, UpdateIngredientRequest request)
    {
        var ingredient = await _context.Ingredients.FindAsync(id);
        if (ingredient == null)
        {
            return OperateResult<IngredientDto>.Fail("原料不存在");
        }

        ingredient.Name = request.Name;
        ingredient.Type = request.Type;
        ingredient.Description = request.Description;

        await _context.SaveChangesAsync();

        var dto = new IngredientDto
        {
            Id = ingredient.Id,
            Name = ingredient.Name,
            Type = ingredient.Type,
            Description = ingredient.Description
        };

        return OperateResult<IngredientDto>.Succeed(dto, "更新成功");
    }

    public async Task<OperateResult<bool>> DeleteAsync(int id)
    {
        var ingredient = await _context.Ingredients.FindAsync(id);
        if (ingredient == null)
        {
            return OperateResult<bool>.Fail("原料不存在");
        }

        // 检查是否有关联的菜谱
        var hasRecipes = await _context.RecipeIngredients.AnyAsync(ri => ri.IngredientId == id);
        if (hasRecipes)
        {
            return OperateResult<bool>.Fail("该原料已被菜谱使用，无法删除");
        }

        _context.Ingredients.Remove(ingredient);
        await _context.SaveChangesAsync();

        return OperateResult<bool>.Succeed(true, "删除成功");
    }
}
