using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> builder)
    {
        builder.ToTable("notification_log", t =>
        {
            // Postgres jsonb → JSON text with validation.
            t.HasCheckConstraint("CK_notification_log_data_json",
                "[data] IS NULL OR ISJSON([data]) = 1");
        });
        builder.HasKey(l => l.Id).HasName("PK_notification_log");

        builder.Property(l => l.Id).HasColumnName("id");
        builder.Property(l => l.UserId).HasColumnName("user_id");
        builder.Property(l => l.Type).HasColumnName("type").IsRequired().HasMaxLength(50);
        builder.Property(l => l.Title).HasColumnName("title").IsRequired().HasMaxLength(200);
        builder.Property(l => l.Body).HasColumnName("body").IsRequired();
        builder.Property(l => l.Data).HasColumnName("data");
        builder.Property(l => l.SentAt).HasColumnName("sent_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");
        builder.Property(l => l.ReadAt).HasColumnName("read_at");

        builder.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .HasConstraintName("FK_notification_log_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        // History listing per user, newest first.
        builder.HasIndex(l => new { l.UserId, l.SentAt })
            .HasDatabaseName("IX_notification_log_user_id_sent_at");
    }
}
