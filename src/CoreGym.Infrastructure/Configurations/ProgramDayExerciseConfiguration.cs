using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class ProgramDayExerciseConfiguration : IEntityTypeConfiguration<ProgramDayExercise>
{
    public void Configure(EntityTypeBuilder<ProgramDayExercise> builder)
    {
        builder.ToTable("program_day_exercises");
        builder.HasKey(e => e.Id).HasName("PK_program_day_exercises");

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.ProgramDayId).HasColumnName("program_day_id");
        builder.Property(e => e.ExerciseId).HasColumnName("exercise_id");
        builder.Property(e => e.OrderIndex).HasColumnName("order_index");
        builder.Property(e => e.Sets).HasColumnName("sets");
        builder.Property(e => e.RepsMin).HasColumnName("reps_min");
        builder.Property(e => e.RepsMax).HasColumnName("reps_max");
        builder.Property(e => e.RestSeconds).HasColumnName("rest_seconds").HasDefaultValueSql("((90))");
        builder.Property(e => e.Notes).HasColumnName("notes");
        builder.Property(e => e.NotesAr).HasColumnName("notes_ar");
        builder.Property(e => e.IsMainLift).HasColumnName("is_main_lift").HasDefaultValue(false);

        builder.HasOne(e => e.ProgramDay)
            .WithMany()
            .HasForeignKey(e => e.ProgramDayId)
            .HasConstraintName("FK_program_day_exercises_program_days")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Exercise)
            .WithMany()
            .HasForeignKey(e => e.ExerciseId)
            .HasConstraintName("FK_program_day_exercises_exercises")
            .OnDelete(DeleteBehavior.Restrict);

        // Ordered retrieval of a day's exercises; ExerciseId for reverse lookups.
        builder.HasIndex(e => new { e.ProgramDayId, e.OrderIndex })
            .HasDatabaseName("IX_program_day_exercises_program_day_id_order_index");
        builder.HasIndex(e => e.ExerciseId).HasDatabaseName("IX_program_day_exercises_exercise_id");
    }
}
