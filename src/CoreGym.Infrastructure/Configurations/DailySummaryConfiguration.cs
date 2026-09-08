using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class DailySummaryConfiguration : IEntityTypeConfiguration<DailySummary>
{
    public void Configure(EntityTypeBuilder<DailySummary> builder)
    {
        // Declares the updated_at trigger so EF avoids the OUTPUT clause on SaveChanges.
        builder.ToTable("daily_summary", t =>
        {
            t.HasTrigger("trg_daily_summary_updated_at");
            // CHECK value from the live prod schema dump (2026-09-07).
            t.HasCheckConstraint("CK_daily_summary_mood", "[mood] IS NULL OR ([mood] >= 1 AND [mood] <= 5)");
        });
        builder.HasKey(s => s.Id).HasName("PK_daily_summary");

        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.UserId).HasColumnName("user_id");
        builder.Property(s => s.SummaryDate).HasColumnName("summary_date").HasColumnType("date").HasDefaultValueSql("(CAST(SYSUTCDATETIME() AS date))");
        builder.Property(s => s.Steps).HasColumnName("steps").HasDefaultValue(0);
        builder.Property(s => s.ActiveMinutes).HasColumnName("active_minutes").HasDefaultValue(0);
        builder.Property(s => s.CaloriesBurned).HasColumnName("calories_burned").HasDefaultValue(0);
        builder.Property(s => s.WaterMl).HasColumnName("water_ml").HasDefaultValue(0);
        builder.Property(s => s.SleepHours).HasColumnName("sleep_hours").HasColumnType("decimal(4,2)");
        builder.Property(s => s.CaloriesConsumed).HasColumnName("calories_consumed").HasColumnType("decimal(8,2)").HasDefaultValue(0m);
        builder.Property(s => s.ProteinG).HasColumnName("protein_g").HasColumnType("decimal(8,2)").HasDefaultValue(0m);
        builder.Property(s => s.CarbsG).HasColumnName("carbs_g").HasColumnType("decimal(8,2)").HasDefaultValue(0m);
        builder.Property(s => s.FatG).HasColumnName("fat_g").HasColumnType("decimal(8,2)").HasDefaultValue(0m);
        builder.Property(s => s.WorkoutDone).HasColumnName("workout_done").HasDefaultValue(false);
        builder.Property(s => s.WorkoutDuration).HasColumnName("workout_duration").HasDefaultValue(0);
        builder.Property(s => s.Mood).HasColumnName("mood");
        builder.Property(s => s.Notes).HasColumnName("notes");
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .HasConstraintName("FK_daily_summary_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.UserId, s.SummaryDate })
            .HasDatabaseName("IX_daily_summary_user_id_summary_date");
    }
}
