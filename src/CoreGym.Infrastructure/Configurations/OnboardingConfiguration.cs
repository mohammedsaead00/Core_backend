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

        builder.HasIndex(o => o.UserId).HasDatabaseName("IX_onboarding_user_id");
    }
}
