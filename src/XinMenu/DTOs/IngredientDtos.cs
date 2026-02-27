namespace XinMenu.DTOs;

#region Ingredient DTOs

public class IngredientDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class IngredientGroupDto
{
    public string Type { get; set; } = string.Empty;
    public List<IngredientDto> Items { get; set; } = new();
}

public class CreateIngredientRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateIngredientRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class IngredientQueryRequest
{
    public string? Type { get; set; }
    public string? Keyword { get; set; }
}

#endregion
