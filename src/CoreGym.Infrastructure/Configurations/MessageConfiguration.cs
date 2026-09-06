using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages", t =>
        {
            // Documented allowed values from the inventory (consolidates the
            // duplicated conversation/message RLS era of the original schema).
            t.HasCheckConstraint("CK_messages_type",
                "[type] IN (N'text', N'voice', N'image', N'file')");
        });
        builder.HasKey(m => m.Id).HasName("PK_messages");

        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.ConversationId).HasColumnName("conversation_id");
        builder.Property(m => m.SenderId).HasColumnName("sender_id");
        builder.Property(m => m.Content).HasColumnName("content").IsRequired();
        builder.Property(m => m.Type).HasColumnName("type").HasMaxLength(20).HasDefaultValueSql("(N'text')");
        builder.Property(m => m.FileUrl).HasColumnName("file_url").HasMaxLength(500);
        builder.Property(m => m.IsRead).HasColumnName("is_read").HasDefaultValue(false);
        builder.Property(m => m.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
        builder.Property(m => m.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        builder.HasOne(m => m.Conversation)
            .WithMany()
            .HasForeignKey(m => m.ConversationId)
            .HasConstraintName("FK_messages_conversations")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .HasConstraintName("FK_messages_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        // Chat history fetch: messages of a conversation in order.
        builder.HasIndex(m => new { m.ConversationId, m.CreatedAt })
            .HasDatabaseName("IX_messages_conversation_id_created_at");
        builder.HasIndex(m => m.SenderId).HasDatabaseName("IX_messages_sender_id");
    }
}
