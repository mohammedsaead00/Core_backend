using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class VoiceFoodLogConfiguration : IEntityTypeConfiguration<VoiceFoodLog>
{
    public void Configure(EntityTypeBuilder<VoiceFoodLog> builder)
    {
        builder.ToTable("voice_food_logs", t =>
        {
            // CHECK value from the live prod schema dump (2026-09-07).
            t.HasCheckConstraint("CK_voice_food_logs_confidence",
                "[confidence] IS NULL OR [confidence] IN (N'low', N'medium', N'high')");
        });
        builder.HasKey(l => l.Id).HasName("PK_voice_food_logs");

        builder.Property(l => l.Id).HasColumnName("id");
        builder.Property(l => l.UserId).HasColumnName("user_id");
        builder.Property(l => l.AudioPath).HasColumnName("audio_path").HasMaxLength(500);
        builder.Property(l => l.Transcript).HasColumnName("transcript");
        builder.Property(l => l.IsFood).HasColumnName("is_food").HasDefaultValueSql("((1))");
        builder.Property(l => l.Confidence).HasColumnName("confidence").HasMaxLength(50).HasDefaultValueSql("(N'medium')");
        builder.Property(l => l.Notes).HasColumnName("notes");
        builder.Property(l => l.LoggedAt).HasColumnName("logged_at").IsRequired().HasDefaultValueSql("(SYSDATETIMEOFFSET())");
        builder.Property(l => l.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        builder.HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .HasConstraintName("FK_voice_food_logs_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.UserId).HasDatabaseName("IX_voice_food_logs_user_id");
    }
}
