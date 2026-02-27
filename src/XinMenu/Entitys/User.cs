using Daibitx.Identity.Core.Aggregates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace XinMenu.Entitys;

[Table("users")]
public class User : DxUser
{
    public string? AvaterUrl { get; set; }
    //public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    //public virtual ICollection<FoodLog> FoodLogs { get; set; } = new List<FoodLog>();
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
    }
}

