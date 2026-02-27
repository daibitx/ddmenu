using Daibitx.AspNetCore.Utils.Models;
using Microsoft.EntityFrameworkCore;
using XinMenu.Data;
using XinMenu.DTOs;
using XinMenu.Entitys;
using XinMenu.Models;
using XinMenu.Services.Abstractions;

namespace XinMenu.Services.Inplementations;

public class RecipeService : IRecipeService
{
    private readonly AppDbContext _context;
    private readonly ILogger<RecipeService> _logger;

    public RecipeService(AppDbContext context, ILogger<RecipeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<OperateResult<RecipeDetailDto>> GetByIdAsync(int id)
    {
        var recipe = await _context.Recipes
            .AsNoTracking()
            .Include(r => r.Category)
            .Include(r => r.Ingredients)
            .ThenInclude(ri => ri.Ingredient)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recipe == null)
        {
            return OperateResult<RecipeDetailDto>.Fail("菜谱不存在");
        }

        var dto = MapToDetailDto(recipe);
        return OperateResult<RecipeDetailDto>.Succeed(dto);
    }

    public async Task<OperateResult<PagedList<RecipeListItemDto>>> QueryAsync(RecipeQueryRequest request)
    {
        var query = _context.Recipes
            .AsNoTracking()
            .Include(r => r.Category)
            .AsQueryable();

        if (request.CategoryId.HasValue)
        {
            query = query.Where(r => r.CategoryId == request.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Keyword))
        {
            query = query.Where(r => r.Name.Contains(request.Keyword));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new RecipeListItemDto
            {
                Id = r.Id,
                Name = r.Name,
                Category = new CategoryDto
                {
                    Id = r.Category.Id,
                    Name = r.Category.Name,
                    Icon = r.Category.Icon,
                    SortOrder = r.Category.SortOrder
                },
                Image = r.Image,
                CookTime = r.CookTime,
                Difficulty = r.Difficulty,
                IsFavorite = false, // TODO: 实现收藏功能
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        var result = new PagedList<RecipeListItemDto>
        {
            Items = items,
            TotalCount = total,
            PageIndex = request.Page,
            PageSize = request.PageSize
        };

        return OperateResult<PagedList<RecipeListItemDto>>.Succeed(result);
    }

    public async Task<OperateResult<RecipeDetailDto>> CreateAsync(CreateRecipeRequest request, int userId)
    {
        var category = await _context.RecipeCategories.FindAsync(request.CategoryId);
        if (category == null)
        {
            return OperateResult<RecipeDetailDto>.Fail("分类不存在");
        }

        var recipe = new Recipe
        {
            Name = request.Name,
            CategoryId = request.CategoryId,
            Image = request.Image,
            CookTime = request.CookTime,
            Difficulty = request.Difficulty,
            Steps = new RecipeStep { Step = string.Join("|", request.Steps), Description = "" },
            CreatedBy = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();

        // 添加原料
        foreach (var ingredientReq in request.Ingredients)
        {
            var ingredient = new RecipeIngredient
            {
                RecipeId = recipe.Id,
                IngredientId = ingredientReq.IngredientId,
                Name = ingredientReq.Name,
                Amount = ingredientReq.Amount
            };
            _context.RecipeIngredients.Add(ingredient);
        }

        await _context.SaveChangesAsync();

        // 重新加载完整数据
        var createdRecipe = await _context.Recipes
            .Include(r => r.Category)
            .Include(r => r.Ingredients)
            .ThenInclude(ri => ri.Ingredient)
            .FirstAsync(r => r.Id == recipe.Id);

        var dto = MapToDetailDto(createdRecipe);
        return OperateResult<RecipeDetailDto>.Succeed(dto, "创建成功");
    }

    public async Task<OperateResult<RecipeDetailDto>> UpdateAsync(int id, UpdateRecipeRequest request)
    {
        var recipe = await _context.Recipes
            .Include(r => r.Ingredients)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recipe == null)
        {
            return OperateResult<RecipeDetailDto>.Fail("菜谱不存在");
        }

        var category = await _context.RecipeCategories.FindAsync(request.CategoryId);
        if (category == null)
        {
            return OperateResult<RecipeDetailDto>.Fail("分类不存在");
        }

        recipe.Name = request.Name;
        recipe.CategoryId = request.CategoryId;
        recipe.Image = request.Image;
        recipe.CookTime = request.CookTime;
        recipe.Difficulty = request.Difficulty;
        recipe.Steps = new RecipeStep { Step = string.Join("|", request.Steps), Description = "" };
        recipe.UpdatedAt = DateTime.UtcNow;

        // 删除旧原料
        _context.RecipeIngredients.RemoveRange(recipe.Ingredients);

        // 添加新原料
        foreach (var ingredientReq in request.Ingredients)
        {
            var ingredient = new RecipeIngredient
            {
                RecipeId = recipe.Id,
                IngredientId = ingredientReq.IngredientId,
                Name = ingredientReq.Name,
                Amount = ingredientReq.Amount
            };
            _context.RecipeIngredients.Add(ingredient);
        }

        await _context.SaveChangesAsync();

        // 重新加载完整数据
        var updatedRecipe = await _context.Recipes
            .Include(r => r.Category)
            .Include(r => r.Ingredients)
            .ThenInclude(ri => ri.Ingredient)
            .FirstAsync(r => r.Id == id);

        var dto = MapToDetailDto(updatedRecipe);
        return OperateResult<RecipeDetailDto>.Succeed(dto, "更新成功");
    }

    public async Task<OperateResult<bool>> DeleteAsync(int id)
    {
        var recipe = await _context.Recipes.FindAsync(id);
        if (recipe == null)
        {
            return OperateResult<bool>.Fail("菜谱不存在");
        }

        _context.Recipes.Remove(recipe);
        await _context.SaveChangesAsync();

        return OperateResult<bool>.Succeed(true, "删除成功");
    }

    public async Task<OperateResult<bool>> ToggleFavoriteAsync(int recipeId, int userId, bool isFavorite)
    {
        // TODO: 实现收藏功能，需要添加收藏表
        return OperateResult<bool>.Succeed(true);
    }

    private static RecipeDetailDto MapToDetailDto(Recipe recipe)
    {
        var steps = recipe.Steps?.Step?.Split('|', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

        return new RecipeDetailDto
        {
            Id = recipe.Id,
            Name = recipe.Name,
            Category = new CategoryDto
            {
                Id = recipe.Category.Id,
                Name = recipe.Category.Name,
                Icon = recipe.Category.Icon,
                SortOrder = recipe.Category.SortOrder
            },
            Image = recipe.Image,
            CookTime = recipe.CookTime,
            Difficulty = recipe.Difficulty,
            IsFavorite = false,
            Ingredients = recipe.Ingredients?.Select(i => new RecipeIngredientDto
            {
                Id = i.IngredientId,
                Name = i.Name,
                FromLibrary = i.IngredientId.HasValue,
                Amount = i.Amount
            }).ToList() ?? new List<RecipeIngredientDto>(),
            Steps = steps.Select((s, index) => new RecipeStepDto
            {
                StepNumber = index + 1,
                Description = s
            }).ToList(),
            CreatedAt = recipe.CreatedAt,
            UpdatedAt = recipe.UpdatedAt
        };
    }
}
