using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class StreakActivityLogConfiguration : IEntityTypeConfiguration<StreakActivityLog>
{
    public void Configure(EntityTypeBuilder<StreakActivityLog> builder)
    {
        builder.ToTable("streak_activity_log", t =>
        {
            // Documented allowed values from the inventory.
            t.HasCheckConstraint("CK_streak_activity_log_source",
                "[source] IN (N'workout', N'nutrition')");
        });
        builder.HasKey(l => l.Id).HasName("PK_streak_activity_log");

        builder.Property(l => l.Id).HasColumnName("id");
        builder.Property(l => l.UserId).HasColumnName("user_id");
        builder.Property(l => l.ActivityDate).HasColumnName("activity_date").HasColumnType("date");
        builder.Property(l => l.Source).HasColumnName("source").IsRequired().HasMaxLength(50);
        builder.Property(l => l.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        builder.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .HasConstraintName("FK_streak_activity_log_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        // Streak logic queries by user and date.
        builder.HasIndex(l => new { l.UserId, l.ActivityDate })
            .HasDatabaseName("IX_streak_activity_log_user_id_activity_date");
    }
}
