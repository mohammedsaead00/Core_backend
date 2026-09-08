using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class WorkoutSessionConfiguration : IEntityTypeConfiguration<WorkoutSession>
{
    public void Configure(EntityTypeBuilder<WorkoutSession> builder)
    {
        builder.ToTable("workout_sessions", t =>
        {
            // CHECK value from the live prod schema dump (2026-09-07).
            t.HasCheckConstraint("CK_workout_sessions_muscle_group",
                "[muscle_group] IN (N'chest', N'arms', N'legs', N'core', N'back', N'shoulders', N'full_body')");
        });
        builder.HasKey(s => s.Id).HasName("PK_workout_sessions");

        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.UserId).HasColumnName("user_id");
        builder.Property(s => s.MuscleGroup).HasColumnName("muscle_group").IsRequired().HasMaxLength(50);
        builder.Property(s => s.SessionName).HasColumnName("session_name").HasMaxLength(200);
        builder.Property(s => s.DurationMin).HasColumnName("duration_min").HasDefaultValue(0);
        builder.Property(s => s.Notes).HasColumnName("notes");
        builder.Property(s => s.SessionDate).HasColumnName("session_date").IsRequired().HasColumnType("date").HasDefaultValueSql("(CAST(SYSUTCDATETIME() AS date))");
        builder.Property(s => s.StartedAt).HasColumnName("started_at").IsRequired().HasDefaultValueSql("(SYSDATETIMEOFFSET())");
        builder.Property(s => s.EndedAt).HasColumnName("ended_at");

        builder.HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .HasConstraintName("FK_workout_sessions_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.UserId, s.SessionDate })
            .HasDatabaseName("IX_workout_sessions_user_id_session_date");
    }
}
