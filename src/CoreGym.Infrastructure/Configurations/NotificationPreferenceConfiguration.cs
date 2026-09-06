using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        // Declares the updated_at trigger so EF avoids the OUTPUT clause on SaveChanges.
        builder.ToTable("notification_preferences", t =>
        {
            t.HasTrigger("trg_notification_preferences_updated_at");
        });
        builder.HasKey(p => p.UserId).HasName("PK_notification_preferences");

        builder.Property(p => p.UserId).HasColumnName("user_id");
        builder.Property(p => p.MealRemindersEnabled).HasColumnName("meal_reminders_enabled").HasDefaultValueSql("((1))");
        builder.Property(p => p.WaterRemindersEnabled).HasColumnName("water_reminders_enabled").HasDefaultValueSql("((1))");
        builder.Property(p => p.CalorieAlertsEnabled).HasColumnName("calorie_alerts_enabled").HasDefaultValueSql("((1))");
        builder.Property(p => p.ChatNotificationsEnabled).HasColumnName("chat_notifications_enabled").HasDefaultValueSql("((1))");
        builder.Property(p => p.QuietHoursStart).HasColumnName("quiet_hours_start").HasColumnType("time");
        builder.Property(p => p.QuietHoursEnd).HasColumnName("quiet_hours_end").HasColumnType("time");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .HasConstraintName("FK_notification_preferences_profiles")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
