using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class BarcodeScanHistoryConfiguration : IEntityTypeConfiguration<BarcodeScanHistory>
{
    public void Configure(EntityTypeBuilder<BarcodeScanHistory> builder)
    {
        builder.ToTable("barcode_scan_history");
        builder.HasKey(h => h.Id).HasName("PK_barcode_scan_history");

        builder.Property(h => h.Id).HasColumnName("id");
        builder.Property(h => h.UserId).HasColumnName("user_id");
        builder.Property(h => h.Barcode).HasColumnName("barcode").IsRequired().HasMaxLength(50);
        builder.Property(h => h.QuantityG).HasColumnName("quantity_g").HasColumnType("decimal(8,2)");
        builder.Property(h => h.NutritionLogId).HasColumnName("nutrition_log_id");
        builder.Property(h => h.ScannedAt).HasColumnName("scanned_at");

        builder.HasOne(h => h.User)
            .WithMany()
            .HasForeignKey(h => h.UserId)
            .HasConstraintName("FK_barcode_scan_history_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.Product)
            .WithMany()
            .HasForeignKey(h => h.Barcode)
            .HasConstraintName("FK_barcode_scan_history_barcode_products")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(h => h.NutritionLog)
            .WithMany()
            .HasForeignKey(h => h.NutritionLogId)
            .HasConstraintName("FK_barcode_scan_history_nutrition_logs")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(h => h.UserId).HasDatabaseName("IX_barcode_scan_history_user_id");
        builder.HasIndex(h => h.Barcode).HasDatabaseName("IX_barcode_scan_history_barcode");
        builder.HasIndex(h => h.NutritionLogId).HasDatabaseName("IX_barcode_scan_history_nutrition_log_id");
    }
}
