namespace XinMenu.DTOs;

#region FoodLog DTOs

public class FoodLogItemDto
{
    public int Id { get; set; }
    public string Time { get; set; } = string.Empty;
    public string MealType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? RecipeId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class FoodLogDayDto
{
    public string Date { get; set; } = string.Empty;
    public List<FoodLogGroupDto> Items { get; set; } = new();
}

public class FoodLogGroupDto
{
    public string MealType { get; set; } = string.Empty;
    public List<FoodLogItemDto> Items { get; set; } = new();
}

public class CreateFoodLogRequest
{
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string MealType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? RecipeId { get; set; }
}

public class UpdateFoodLogRequest
{
    public string Date { get; set; } = string.Empty;
    public string Time { get; set; } = string.Empty;
    public string MealType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? RecipeId { get; set; }
}

public class FoodLogCalendarDto
{
    public Dictionary<string, int> Data { get; set; } = new();
}

public class FoodLogQueryRequest
{
    public string Date { get; set; } = string.Empty;
}

public class FoodLogCalendarQueryRequest
{
    public int Year { get; set; }
    public int Month { get; set; }
}

#endregion
