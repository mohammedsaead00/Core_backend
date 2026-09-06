using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class FoodConfiguration : IEntityTypeConfiguration<Food>
{
    public void Configure(EntityTypeBuilder<Food> builder)
    {
        builder.ToTable("foods");
        builder.HasKey(f => f.Id).HasName("PK_foods");

        builder.Property(f => f.Id).HasColumnName("id");
        builder.Property(f => f.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(f => f.NameAr).HasColumnName("name_ar").HasMaxLength(200);
        builder.Property(f => f.Calories).HasColumnName("calories").HasColumnType("decimal(8,2)");
        builder.Property(f => f.ProteinG).HasColumnName("protein_g").HasColumnType("decimal(8,2)").HasDefaultValue(0m);
        builder.Property(f => f.CarbsG).HasColumnName("carbs_g").HasColumnType("decimal(8,2)").HasDefaultValue(0m);
        builder.Property(f => f.FatG).HasColumnName("fat_g").HasColumnType("decimal(8,2)").HasDefaultValue(0m);
        builder.Property(f => f.FiberG).HasColumnName("fiber_g").HasColumnType("decimal(8,2)").HasDefaultValue(0m);
        builder.Property(f => f.ServingSize).HasColumnName("serving_size").HasColumnType("decimal(6,2)").HasDefaultValueSql("((100))");
        builder.Property(f => f.ServingUnit).HasColumnName("serving_unit").HasMaxLength(50).HasDefaultValueSql("(N'g')");
        builder.Property(f => f.IsCustom).HasColumnName("is_custom").HasDefaultValue(false);
        builder.Property(f => f.CreatedBy).HasColumnName("created_by");
        builder.Property(f => f.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");
        builder.Property(f => f.Category).HasColumnName("category").HasMaxLength(50).HasDefaultValueSql("(N'other')");
        builder.Property(f => f.ImageUrl).HasColumnName("image_url").HasMaxLength(500);

        builder.HasOne(f => f.Creator)
            .WithMany()
            .HasForeignKey(f => f.CreatedBy)
            .HasConstraintName("FK_foods_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(f => f.CreatedBy).HasDatabaseName("IX_foods_created_by");
    }
}
