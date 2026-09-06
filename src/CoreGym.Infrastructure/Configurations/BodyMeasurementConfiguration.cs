using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class BodyMeasurementConfiguration : IEntityTypeConfiguration<BodyMeasurement>
{
    public void Configure(EntityTypeBuilder<BodyMeasurement> builder)
    {
        builder.ToTable("body_measurements");
        builder.HasKey(m => m.Id).HasName("PK_body_measurements");

        builder.Property(m => m.Id).HasColumnName("id");
        builder.Property(m => m.UserId).HasColumnName("user_id");
        builder.Property(m => m.WeightKg).HasColumnName("weight_kg").HasColumnType("decimal(5,2)");
        builder.Property(m => m.BodyFatPct).HasColumnName("body_fat_pct").HasColumnType("decimal(5,2)");
        builder.Property(m => m.MuscleMass).HasColumnName("muscle_mass").HasColumnType("decimal(5,2)");
        builder.Property(m => m.ChestCm).HasColumnName("chest_cm").HasColumnType("decimal(5,1)");
        builder.Property(m => m.WaistCm).HasColumnName("waist_cm").HasColumnType("decimal(5,1)");
        builder.Property(m => m.HipsCm).HasColumnName("hips_cm").HasColumnType("decimal(5,1)");
        builder.Property(m => m.ArmsCm).HasColumnName("arms_cm").HasColumnType("decimal(5,1)");
        builder.Property(m => m.ThighsCm).HasColumnName("thighs_cm").HasColumnType("decimal(5,1)");
        builder.Property(m => m.MeasuredDate).HasColumnName("measured_date").HasColumnType("date").HasDefaultValueSql("(CAST(SYSUTCDATETIME() AS date))");
        builder.Property(m => m.Notes).HasColumnName("notes");
        builder.Property(m => m.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .HasConstraintName("FK_body_measurements_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        // Typical query: a user's measurement history over time.
        builder.HasIndex(m => new { m.UserId, m.MeasuredDate })
            .HasDatabaseName("IX_body_measurements_user_id_measured_date");
    }
}
