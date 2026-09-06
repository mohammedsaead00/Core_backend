using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class VoiceFoodLogItemConfiguration : IEntityTypeConfiguration<VoiceFoodLogItem>
{
    public void Configure(EntityTypeBuilder<VoiceFoodLogItem> builder)
    {
        builder.ToTable("voice_food_log_items");
        builder.HasKey(i => i.Id).HasName("PK_voice_food_log_items");

        builder.Property(i => i.Id).HasColumnName("id");
        builder.Property(i => i.LogId).HasColumnName("log_id");
        builder.Property(i => i.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(i => i.NameAr).HasColumnName("name_ar").HasMaxLength(200);
        builder.Property(i => i.EstimatedWeightG).HasColumnName("estimated_weight_g").HasColumnType("decimal(8,2)").HasDefaultValue(0m);
        builder.Property(i => i.Calories).HasColumnName("calories").HasColumnType("decimal(8,2)").HasDefaultValue(0m);
        builder.Property(i => i.ProteinG).HasColumnName("protein_g").HasColumnType("decimal(8,2)").HasDefaultValue(0m);
        builder.Property(i => i.CarbsG).HasColumnName("carbs_g").HasColumnType("decimal(8,2)").HasDefaultValue(0m);
        builder.Property(i => i.FatG).HasColumnName("fat_g").HasColumnType("decimal(8,2)").HasDefaultValue(0m);
        builder.Property(i => i.NutritionLogId).HasColumnName("nutrition_log_id");
        builder.Property(i => i.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        builder.HasOne(i => i.Log)
            .WithMany()
            .HasForeignKey(i => i.LogId)
            .HasConstraintName("FK_voice_food_log_items_voice_food_logs")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.NutritionLog)
            .WithMany()
            .HasForeignKey(i => i.NutritionLogId)
            .HasConstraintName("FK_voice_food_log_items_nutrition_logs")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => i.LogId).HasDatabaseName("IX_voice_food_log_items_log_id");
        builder.HasIndex(i => i.NutritionLogId).HasDatabaseName("IX_voice_food_log_items_nutrition_log_id");
    }
}
