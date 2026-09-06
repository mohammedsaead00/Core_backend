using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");
        builder.HasKey(n => n.Id).HasName("PK_notifications");

        builder.Property(n => n.Id).HasColumnName("id");
        builder.Property(n => n.UserId).HasColumnName("user_id");
        builder.Property(n => n.Type).HasColumnName("type").IsRequired().HasMaxLength(50);
        builder.Property(n => n.Title).HasColumnName("title").IsRequired().HasMaxLength(200);
        builder.Property(n => n.Body).HasColumnName("body").IsRequired();
        builder.Property(n => n.ConversationId).HasColumnName("conversation_id");
        builder.Property(n => n.PlanId).HasColumnName("plan_id");
        builder.Property(n => n.CoachId).HasColumnName("coach_id");
        builder.Property(n => n.IsRead).HasColumnName("is_read").HasDefaultValue(false);
        builder.Property(n => n.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        builder.HasOne(n => n.User)
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .HasConstraintName("FK_notifications_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.Conversation)
            .WithMany()
            .HasForeignKey(n => n.ConversationId)
            .HasConstraintName("FK_notifications_conversations")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.Plan)
            .WithMany()
            .HasForeignKey(n => n.PlanId)
            .HasConstraintName("FK_notifications_subscription_plans")
            .OnDelete(DeleteBehavior.Restrict);

        // NOTE: coach_id intentionally has NO FK — key space unverified
        // (user id vs coaches.id). See MIGRATION_PROGRESS.md open question 12.

        builder.HasIndex(n => new { n.UserId, n.CreatedAt })
            .HasDatabaseName("IX_notifications_user_id_created_at");
    }
}
