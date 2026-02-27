using Daibitx.EFCore.Extension.Converters;
using Daibitx.Identity.Session;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using XinMenu.Models;

namespace XinMenu.Entitys
{
    [Table("recipes")]
    public class Recipe
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Column("category_id")]
        public int CategoryId { get; set; }

        [Column("image")]
        [MaxLength(255)]
        public string? Image { get; set; }

        [Column("cook_time")]
        public int CookTime { get; set; }

        [Column("difficulty")]
        public byte Difficulty { get; set; }

        public RecipeStep Steps { get; set; }

        [Column("created_by")]
        public int CreatedBy { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        // Navigation
        public RecipeCategory Category { get; set; }
        public ICollection<RecipeIngredient> Ingredients { get; set; }
    }
    public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
    {
        public void Configure(EntityTypeBuilder<Recipe> builder)
        {
            builder.ToTable("recipes");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Id)
                .ValueGeneratedOnAdd();

            builder.Property(r => r.Name)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(r => r.Steps)
                .HasConversion(new JsonValueConverter<RecipeStep>())
                .HasColumnName("steps")
                .HasMaxLength(1000);

            builder.HasOne(r => r.Category)
                .WithMany(c => c.Recipes)
                .HasForeignKey(r => r.CategoryId);
        }
    }
}
