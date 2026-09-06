using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class BarcodeProductConfiguration : IEntityTypeConfiguration<BarcodeProduct>
{
    public void Configure(EntityTypeBuilder<BarcodeProduct> builder)
    {
        // Declares the updated_at trigger so EF avoids the OUTPUT clause on SaveChanges.
        builder.ToTable("barcode_products", t =>
        {
            // Documented allowed values from the inventory.
            t.HasCheckConstraint("CK_barcode_products_source",
                "[source] IN (N'openfoodfacts', N'gemini_estimate')");
            t.HasTrigger("trg_barcode_products_updated_at");
        });
        builder.HasKey(b => b.Barcode).HasName("PK_barcode_products");

        builder.Property(b => b.Barcode).HasColumnName("barcode").HasMaxLength(50);
        builder.Property(b => b.ProductName).HasColumnName("product_name").IsRequired().HasMaxLength(200);
        builder.Property(b => b.ProductNameAr).HasColumnName("product_name_ar").HasMaxLength(200);
        builder.Property(b => b.Brand).HasColumnName("brand").HasMaxLength(200);
        builder.Property(b => b.ServingSizeG).HasColumnName("serving_size_g").HasColumnType("decimal(6,2)");
        builder.Property(b => b.Calories).HasColumnName("calories").HasColumnType("decimal(8,2)").HasDefaultValue(0m);
        builder.Property(b => b.ProteinG).HasColumnName("protein_g").HasColumnType("decimal(8,2)").HasDefaultValue(0m);
        builder.Property(b => b.CarbsG).HasColumnName("carbs_g").HasColumnType("decimal(8,2)").HasDefaultValue(0m);
        builder.Property(b => b.FatG).HasColumnName("fat_g").HasColumnType("decimal(8,2)").HasDefaultValue(0m);
        builder.Property(b => b.Source).HasColumnName("source").IsRequired().HasMaxLength(50);
        builder.Property(b => b.Confidence).HasColumnName("confidence").HasMaxLength(50).HasDefaultValueSql("(N'high')");
        builder.Property(b => b.LookupCount).HasColumnName("lookup_count").HasDefaultValueSql("((1))");
        builder.Property(b => b.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");
        builder.Property(b => b.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");
    }
}
