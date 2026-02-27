using Daibitx.Identity.EFCore.Context;
using Microsoft.EntityFrameworkCore;
using XinMenu.Entitys;

namespace XinMenu.Data;

public class AppDbContext : DxIdentityDbContext<User, Role>
{
    public DbSet<UserRefreshToken> RefreshTokens { get; set; }
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<RecipeIngredient> RecipeIngredients { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<RecipeCategory> RecipeCategories { get; set; }
    public DbSet<FoodLog> FoodLogs { get; set; }
    public AppDbContext(DbContextOptions<DxIdentityDbContext<User, Role>> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserRefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new RecipeConfiguration());
        modelBuilder.ApplyConfiguration(new RecipeIngredientConfiguration());
        modelBuilder.ApplyConfiguration(new IngredientConfiguration());
        modelBuilder.ApplyConfiguration(new RecipeCategoryConfiguration());
        modelBuilder.ApplyConfiguration(new FoodLogConfiguration());

    }


}
