using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class FoodScanConfiguration : IEntityTypeConfiguration<FoodScan>
{
    public void Configure(EntityTypeBuilder<FoodScan> builder)
    {
        builder.ToTable("food_scans", t =>
        {
            // CHECK value from the live prod schema dump (2026-09-07).
            t.HasCheckConstraint("CK_food_scans_confidence",
                "[confidence] IS NULL OR [confidence] IN (N'low', N'medium', N'high')");
        });
        builder.HasKey(s => s.Id).HasName("PK_food_scans");

        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.UserId).HasColumnName("user_id");
        builder.Property(s => s.ImagePath).HasColumnName("image_path").HasMaxLength(500);
        builder.Property(s => s.IsFood).HasColumnName("is_food").HasDefaultValueSql("((1))");
        builder.Property(s => s.Confidence).HasColumnName("confidence").HasMaxLength(50).HasDefaultValueSql("(N'medium')");
        builder.Property(s => s.Notes).HasColumnName("notes");
        builder.Property(s => s.ScannedAt).HasColumnName("scanned_at").IsRequired().HasDefaultValueSql("(SYSDATETIMEOFFSET())");
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .HasConstraintName("FK_food_scans_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.UserId).HasDatabaseName("IX_food_scans_user_id");
    }
}
