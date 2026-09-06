using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class WorkoutSetConfiguration : IEntityTypeConfiguration<WorkoutSet>
{
    public void Configure(EntityTypeBuilder<WorkoutSet> builder)
    {
        builder.ToTable("workout_sets");
        builder.HasKey(s => s.Id).HasName("PK_workout_sets");

        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.SessionId).HasColumnName("session_id");
        builder.Property(s => s.UserId).HasColumnName("user_id");
        builder.Property(s => s.ExerciseName).HasColumnName("exercise_name").IsRequired().HasMaxLength(200);
        builder.Property(s => s.SetNumber).HasColumnName("set_number");
        builder.Property(s => s.Reps).HasColumnName("reps");
        builder.Property(s => s.WeightKg).HasColumnName("weight_kg").HasColumnType("decimal(5,2)");
        builder.Property(s => s.DurationSec).HasColumnName("duration_sec");
        builder.Property(s => s.RestSec).HasColumnName("rest_sec").HasDefaultValueSql("((60))");
        builder.Property(s => s.IsWarmup).HasColumnName("is_warmup").HasDefaultValue(false);
        builder.Property(s => s.LoggedAt).HasColumnName("logged_at");

        builder.HasOne(s => s.Session)
            .WithMany()
            .HasForeignKey(s => s.SessionId)
            .HasConstraintName("FK_workout_sets_workout_sessions")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .HasConstraintName("FK_workout_sets_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        // Sets of a session; user + exercise name serves the personal-records computation.
        builder.HasIndex(s => s.SessionId).HasDatabaseName("IX_workout_sets_session_id");
        builder.HasIndex(s => new { s.UserId, s.ExerciseName })
            .HasDatabaseName("IX_workout_sets_user_id_exercise_name");
    }
}
