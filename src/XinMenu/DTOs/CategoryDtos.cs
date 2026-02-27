namespace XinMenu.DTOs;

#region Category DTOs

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
}

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
}

#endregion
