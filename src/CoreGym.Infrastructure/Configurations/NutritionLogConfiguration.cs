using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class NutritionLogConfiguration : IEntityTypeConfiguration<NutritionLog>
{
    public void Configure(EntityTypeBuilder<NutritionLog> builder)
    {
        builder.ToTable("nutrition_logs", t =>
        {
            // CHECK value from the live prod schema dump (2026-09-07).
            t.HasCheckConstraint("CK_nutrition_logs_meal_type",
                "[meal_type] IS NULL OR [meal_type] IN (N'breakfast', N'lunch', N'dinner', N'snack')");
        });
        builder.HasKey(n => n.Id).HasName("PK_nutrition_logs");

        builder.Property(n => n.Id).HasColumnName("id");
        builder.Property(n => n.UserId).HasColumnName("user_id");
        builder.Property(n => n.FoodId).HasColumnName("food_id");
        builder.Property(n => n.FoodName).HasColumnName("food_name").IsRequired().HasMaxLength(200);
        builder.Property(n => n.MealType).HasColumnName("meal_type").HasMaxLength(50);
        // Explicitly NOT NULL + default 1 in the inventory: IsRequired makes the
        // column NOT NULL while EF still omits it on insert so the default applies.
        builder.Property(n => n.Quantity).HasColumnName("quantity").IsRequired().HasColumnType("decimal(8,2)").HasDefaultValueSql("((1))");
        builder.Property(n => n.ServingUnit).HasColumnName("serving_unit").HasMaxLength(50).HasDefaultValueSql("(N'g')");
        builder.Property(n => n.Calories).HasColumnName("calories").HasColumnType("decimal(8,2)");
        builder.Property(n => n.ProteinG).HasColumnName("protein_g").HasColumnType("decimal(8,2)").HasDefaultValue(0m);
        builder.Property(n => n.CarbsG).HasColumnName("carbs_g").HasColumnType("decimal(8,2)").HasDefaultValue(0m);
        builder.Property(n => n.FatG).HasColumnName("fat_g").HasColumnType("decimal(8,2)").HasDefaultValue(0m);
        builder.Property(n => n.LoggedDate).HasColumnName("logged_date").IsRequired().HasColumnType("date").HasDefaultValueSql("(CAST(SYSUTCDATETIME() AS date))");
        builder.Property(n => n.LoggedAt).HasColumnName("logged_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .HasConstraintName("FK_nutrition_logs_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.Food)
            .WithMany()
            .HasForeignKey(n => n.FoodId)
            .HasConstraintName("FK_nutrition_logs_foods")
            .OnDelete(DeleteBehavior.Restrict);

        // Typical queries: a user's logs for a day (summary sync, daily views).
        builder.HasIndex(n => new { n.UserId, n.LoggedDate })
            .HasDatabaseName("IX_nutrition_logs_user_id_logged_date");
        builder.HasIndex(n => n.FoodId).HasDatabaseName("IX_nutrition_logs_food_id");
    }
}
