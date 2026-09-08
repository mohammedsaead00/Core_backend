using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications", t =>
        {
            // Values from the live prod schema dump (2026-09-07).
            t.HasCheckConstraint("CK_notifications_type",
                "[type] IN (N'message', N'plan')");
        });
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

        // NOTE: plan_id has NO foreign key in the live prod schema — kept FK-less for fidelity.

        // RESOLVED (open question 12): the prod dump shows notifications.coach_id
        // references profiles(id) — it is a USER id, and it does have an FK.
        builder.HasOne(n => n.Coach)
            .WithMany()
            .HasForeignKey(n => n.CoachId)
            .HasConstraintName("FK_notifications_coach_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(n => new { n.UserId, n.CreatedAt })
            .HasDatabaseName("IX_notifications_user_id_created_at");
    }
}
