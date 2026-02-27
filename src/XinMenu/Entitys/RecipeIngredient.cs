using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XinMenu.Entitys
{
    [Table("recipe_ingredients")]
    public class RecipeIngredient
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("recipe_id")]
        public int RecipeId { get; set; }

        [Column("ingredient_id")]
        public int? IngredientId { get; set; }

        [Column("name")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Column("amount")]
        [MaxLength(50)]
        public string Amount { get; set; }

        public Recipe Recipe { get; set; }
        public Ingredient? Ingredient { get; set; }
    }

    public class RecipeIngredientConfiguration : IEntityTypeConfiguration<RecipeIngredient>
    {
        public void Configure(EntityTypeBuilder<RecipeIngredient> builder)
        {
            builder.ToTable("recipe_ingredients");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();

            builder.HasOne(x => x.Recipe)
                .WithMany(r => r.Ingredients)
                .HasForeignKey(x => x.RecipeId);

            builder.HasOne(x => x.Ingredient)
                .WithMany()
                .HasForeignKey(x => x.IngredientId);
        }
    }
}
