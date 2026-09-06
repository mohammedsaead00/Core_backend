using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("conversations");
        builder.HasKey(c => c.Id).HasName("PK_conversations");

        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.ClientId).HasColumnName("client_id");
        builder.Property(c => c.CoachId).HasColumnName("coach_id");
        builder.Property(c => c.SubscriptionId).HasColumnName("subscription_id");
        builder.Property(c => c.LastMessage).HasColumnName("last_message");
        builder.Property(c => c.LastMessageAt).HasColumnName("last_message_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");
        builder.Property(c => c.ClientUnread).HasColumnName("client_unread").HasDefaultValue(0);
        builder.Property(c => c.CoachUnread).HasColumnName("coach_unread").HasDefaultValue(0);
        builder.Property(c => c.IsActive).HasColumnName("is_active").HasDefaultValueSql("((1))");
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        // Both participants are keyed by AUTH USER id (original RLS compares
        // both to auth.uid()) — hence two FKs to profiles.
        builder.HasOne(c => c.Client)
            .WithMany()
            .HasForeignKey(c => c.ClientId)
            .HasConstraintName("FK_conversations_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Coach)
            .WithMany()
            .HasForeignKey(c => c.CoachId)
            .HasConstraintName("FK_conversations_coach_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Subscription)
            .WithMany()
            .HasForeignKey(c => c.SubscriptionId)
            .HasConstraintName("FK_conversations_subscriptions")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.ClientId).HasDatabaseName("IX_conversations_client_id");
        builder.HasIndex(c => c.CoachId).HasDatabaseName("IX_conversations_coach_id");
        builder.HasIndex(c => c.SubscriptionId).HasDatabaseName("IX_conversations_subscription_id");
    }
}
