using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class ProgramDayConfiguration : IEntityTypeConfiguration<ProgramDay>
{
    public void Configure(EntityTypeBuilder<ProgramDay> builder)
    {
        builder.ToTable("program_days", t =>
        {
            t.HasCheckConstraint("CK_program_days_muscle_groups_json",
                "[muscle_groups] IS NULL OR ISJSON([muscle_groups]) = 1");
        });
        builder.HasKey(d => d.Id).HasName("PK_program_days");

        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.ProgramId).HasColumnName("program_id");
        builder.Property(d => d.DayNumber).HasColumnName("day_number");
        builder.Property(d => d.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(d => d.NameAr).HasColumnName("name_ar").HasMaxLength(200);
        builder.Property(d => d.MuscleGroups).HasColumnName("muscle_groups").AsJsonArray();
        builder.Property(d => d.Notes).HasColumnName("notes");

        builder.HasOne(d => d.Program)
            .WithMany()
            .HasForeignKey(d => d.ProgramId)
            .HasConstraintName("FK_program_days_training_programs")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => d.ProgramId).HasDatabaseName("IX_program_days_program_id");
    }
}
