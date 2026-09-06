using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class ExerciseProgressConfiguration : IEntityTypeConfiguration<ExerciseProgress>
{
    public void Configure(EntityTypeBuilder<ExerciseProgress> builder)
    {
        builder.ToTable("exercise_progress");
        builder.HasKey(p => p.Id).HasName("PK_exercise_progress");

        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.UserId).HasColumnName("user_id");
        builder.Property(p => p.ExerciseId).HasColumnName("exercise_id");
        builder.Property(p => p.SessionDate).HasColumnName("session_date").HasColumnType("date").HasDefaultValueSql("(CAST(SYSUTCDATETIME() AS date))");
        builder.Property(p => p.BestSetWeight).HasColumnName("best_set_weight").HasColumnType("decimal(5,2)");
        builder.Property(p => p.BestSetReps).HasColumnName("best_set_reps");
        builder.Property(p => p.TotalVolume).HasColumnName("total_volume").HasColumnType("decimal(10,2)");
        builder.Property(p => p.OneRmEstimate).HasColumnName("one_rm_estimate").HasColumnType("decimal(6,2)");
        builder.Property(p => p.SessionId).HasColumnName("session_id");
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .HasConstraintName("FK_exercise_progress_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Exercise)
            .WithMany()
            .HasForeignKey(p => p.ExerciseId)
            .HasConstraintName("FK_exercise_progress_exercises")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Session)
            .WithMany()
            .HasForeignKey(p => p.SessionId)
            .HasConstraintName("FK_exercise_progress_workout_sessions")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => new { p.UserId, p.SessionDate })
            .HasDatabaseName("IX_exercise_progress_user_id_session_date");
        builder.HasIndex(p => p.ExerciseId).HasDatabaseName("IX_exercise_progress_exercise_id");
        builder.HasIndex(p => p.SessionId).HasDatabaseName("IX_exercise_progress_session_id");
    }
}
