using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class OnboardingConfiguration : IEntityTypeConfiguration<Onboarding>
{
    public void Configure(EntityTypeBuilder<Onboarding> builder)
    {
        // Declares the updated_at trigger so EF avoids the OUTPUT clause on SaveChanges.
        builder.ToTable("onboarding", t =>
        {
            t.HasTrigger("trg_onboarding_updated_at");
            // CHECK values from the live prod schema dump (2026-09-07).
            t.HasCheckConstraint("CK_onboarding_gender",
                "[gender] IS NULL OR [gender] IN (N'male', N'female')");
            t.HasCheckConstraint("CK_onboarding_goal",
                "[goal] IS NULL OR [goal] IN (N'muscle_gain', N'weight_loss', N'endurance', N'flexibility', N'general_fitness')");
            t.HasCheckConstraint("CK_onboarding_activity_level",
                "[activity_level] IS NULL OR [activity_level] IN (N'sedentary', N'lightly_active', N'moderately_active', N'very_active', N'extra_active')");
            t.HasCheckConstraint("CK_onboarding_weekly_workouts",
                "[weekly_workouts] IS NULL OR ([weekly_workouts] >= 1 AND [weekly_workouts] <= 7)");
        });
        builder.HasKey(o => o.Id).HasName("PK_onboarding");

        builder.Property(o => o.Id).HasColumnName("id");
        builder.Property(o => o.UserId).HasColumnName("user_id");
        builder.Property(o => o.Age).HasColumnName("age");
        builder.Property(o => o.Gender).HasColumnName("gender").HasMaxLength(50);
        builder.Property(o => o.HeightCm).HasColumnName("height_cm").HasColumnType("decimal(5,1)");
        builder.Property(o => o.WeightKg).HasColumnName("weight_kg").HasColumnType("decimal(5,2)");
        builder.Property(o => o.Goal).HasColumnName("goal").HasMaxLength(50);
        builder.Property(o => o.ActivityLevel).HasColumnName("activity_level").HasMaxLength(50);
        builder.Property(o => o.TargetWeight).HasColumnName("target_weight").HasColumnType("decimal(5,2)");
        builder.Property(o => o.WeeklyWorkouts).HasColumnName("weekly_workouts");
        builder.Property(o => o.Completed).HasColumnName("completed").HasDefaultValue(false);
        builder.Property(o => o.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");
        builder.Property(o => o.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        builder.HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .HasConstraintName("FK_onboarding_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => o.UserId).IsUnique().HasDatabaseName("IX_onboarding_user_id");
    }
}
