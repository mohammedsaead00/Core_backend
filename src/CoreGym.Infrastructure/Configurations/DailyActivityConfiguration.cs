using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class DailyActivityConfiguration : IEntityTypeConfiguration<DailyActivity>
{
    public void Configure(EntityTypeBuilder<DailyActivity> builder)
    {
        builder.ToTable("daily_activity");
        builder.HasKey(a => a.Id).HasName("PK_daily_activity");

        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.UserId).HasColumnName("user_id");
        builder.Property(a => a.ActivityDate).HasColumnName("activity_date").IsRequired().HasColumnType("date").HasDefaultValueSql("(CAST(SYSUTCDATETIME() AS date))");
        builder.Property(a => a.Steps).HasColumnName("steps").HasDefaultValue(0);
        builder.Property(a => a.ActiveCaloriesBurned).HasColumnName("active_calories_burned").HasColumnType("decimal(8,2)").HasDefaultValue(0m);
        // Live prod dump: heart rate and exercise minutes are numeric, not int.
        builder.Property(a => a.HeartRateAvg).HasColumnName("heart_rate_avg").HasColumnType("decimal(6,2)");
        builder.Property(a => a.ExerciseMinutes).HasColumnName("exercise_minutes").HasColumnType("decimal(6,2)");
        builder.Property(a => a.Source).HasColumnName("source").HasMaxLength(50).HasDefaultValueSql("(N'health_connect')");
        builder.Property(a => a.SyncedAt).HasColumnName("synced_at").IsRequired().HasDefaultValueSql("(SYSDATETIMEOFFSET())");
        builder.Property(a => a.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        builder.HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .HasConstraintName("FK_daily_activity_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(a => new { a.UserId, a.ActivityDate })
            .HasDatabaseName("IX_daily_activity_user_id_activity_date");
    }
}
