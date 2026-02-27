namespace XinMenu.DTOs;

#region Recipe DTOs

public class RecipeListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CategoryDto Category { get; set; } = null!;
    public string? Image { get; set; }
    public int CookTime { get; set; }
    public byte Difficulty { get; set; }
    public bool IsFavorite { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RecipeDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CategoryDto Category { get; set; } = null!;
    public string? Image { get; set; }
    public int CookTime { get; set; }
    public byte Difficulty { get; set; }
    public bool IsFavorite { get; set; }
    public List<RecipeIngredientDto> Ingredients { get; set; } = new();
    public List<RecipeStepDto> Steps { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateRecipeRequest
{
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? Image { get; set; }
    public int CookTime { get; set; }
    public byte Difficulty { get; set; }
    public List<CreateRecipeIngredientRequest> Ingredients { get; set; } = new();
    public List<string> Steps { get; set; } = new();
}

public class UpdateRecipeRequest
{
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? Image { get; set; }
    public int CookTime { get; set; }
    public byte Difficulty { get; set; }
    public List<CreateRecipeIngredientRequest> Ingredients { get; set; } = new();
    public List<string> Steps { get; set; } = new();
}

public class RecipeIngredientDto
{
    public int? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool FromLibrary { get; set; }
    public string Amount { get; set; } = string.Empty;
}

public class CreateRecipeIngredientRequest
{
    public int? IngredientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
}

public class RecipeStepDto
{
    public int StepNumber { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class FavoriteRequest
{
    public bool IsFavorite { get; set; }
}

public class RecipeQueryRequest
{
    public int? CategoryId { get; set; }
    public string? Keyword { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}



#endregion
