using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XinMenu.Entitys
{
    [Table("recipe_categories")]
    public class RecipeCategory
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        [MaxLength(50)]
        public string Name { get; set; }

        [Column("icon")]
        [MaxLength(50)]
        public string? Icon { get; set; }

        [Column("sort_order")]
        public int SortOrder { get; set; }

        [Column("created_by")]
        public int CreatedBy { get; set; }

        public ICollection<Recipe> Recipes { get; set; }
    }
    public class RecipeCategoryConfiguration : IEntityTypeConfiguration<RecipeCategory>
    {
        public void Configure(EntityTypeBuilder<RecipeCategory> builder)
        {
            builder.ToTable("recipe_categories");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .ValueGeneratedOnAdd();

            builder.Property(c => c.Name).HasMaxLength(50);
            builder.Property(c => c.Icon).HasMaxLength(50);
        }
    }
}
