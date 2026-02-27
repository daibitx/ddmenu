using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XinMenu.Entitys
{
    [Table("food_logs")]
    public class FoodLog
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        [Column("time")]
        public TimeSpan Time { get; set; }

        [Column("meal_type")]
        public string MealType { get; set; }

        [Column("name")]
        [MaxLength(100)]
        public string Name { get; set; }

        [Column("recipe_id")]
        public int? RecipeId { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        public Recipe? Recipe { get; set; }
    }

    public class FoodLogConfiguration : IEntityTypeConfiguration<FoodLog>
    {
        public void Configure(EntityTypeBuilder<FoodLog> builder)
        {
            builder.ToTable("food_logs");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();
            builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(100);

            builder.HasOne(x => x.Recipe)
                .WithMany()
                .HasForeignKey(x => x.RecipeId);
        }
    }
}
