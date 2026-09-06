using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class WeeklyActivityConfiguration : IEntityTypeConfiguration<WeeklyActivity>
{
    public void Configure(EntityTypeBuilder<WeeklyActivity> builder)
    {
        builder.ToTable("weekly_activity");
        builder.HasKey(w => w.Id).HasName("PK_weekly_activity");

        builder.Property(w => w.Id).HasColumnName("id");
        builder.Property(w => w.UserId).HasColumnName("user_id");
        builder.Property(w => w.WeekStart).HasColumnName("week_start").HasColumnType("date");
        builder.Property(w => w.DayIndex).HasColumnName("day_index");
        builder.Property(w => w.ActualPct).HasColumnName("actual_pct").HasColumnType("decimal(5,2)");
        builder.Property(w => w.GoalPct).HasColumnName("goal_pct").HasColumnType("decimal(5,2)").HasDefaultValue(0m);

        builder.HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .HasConstraintName("FK_weekly_activity_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(w => new { w.UserId, w.WeekStart })
            .HasDatabaseName("IX_weekly_activity_user_id_week_start");
    }
}
