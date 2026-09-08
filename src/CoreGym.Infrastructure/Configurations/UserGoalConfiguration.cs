using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class UserGoalConfiguration : IEntityTypeConfiguration<UserGoal>
{
    public void Configure(EntityTypeBuilder<UserGoal> builder)
    {
        // Declares the updated_at trigger so EF avoids the OUTPUT clause on SaveChanges.
        builder.ToTable("user_goals", t =>
        {
            t.HasTrigger("trg_user_goals_updated_at");
        });
        builder.HasKey(g => g.Id).HasName("PK_user_goals");

        builder.Property(g => g.Id).HasColumnName("id");
        builder.Property(g => g.UserId).HasColumnName("user_id");
        builder.Property(g => g.DailyCalories).HasColumnName("daily_calories").HasDefaultValueSql("((2000))");
        builder.Property(g => g.DailyProteinG).HasColumnName("daily_protein_g").HasDefaultValueSql("((150))");
        builder.Property(g => g.DailyCarbsG).HasColumnName("daily_carbs_g").HasDefaultValueSql("((250))");
        builder.Property(g => g.DailyFatG).HasColumnName("daily_fat_g").HasDefaultValueSql("((65))");
        builder.Property(g => g.DailyWaterMl).HasColumnName("daily_water_ml").HasDefaultValueSql("((2500))");
        builder.Property(g => g.DailySteps).HasColumnName("daily_steps").HasDefaultValueSql("((10000))");
        builder.Property(g => g.DailySleepHours).HasColumnName("daily_sleep_hours").HasColumnType("decimal(4,2)").HasDefaultValueSql("((8))");
        builder.Property(g => g.WeeklyWorkouts).HasColumnName("weekly_workouts").HasDefaultValueSql("((4))");
        builder.Property(g => g.TargetWeightKg).HasColumnName("target_weight_kg").HasColumnType("decimal(5,2)");
        builder.Property(g => g.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");
        builder.Property(g => g.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        builder.HasOne(g => g.User)
            .WithMany()
            .HasForeignKey(g => g.UserId)
            .HasConstraintName("FK_user_goals_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(g => g.UserId).IsUnique().HasDatabaseName("IX_user_goals_user_id");
    }
}
