using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class UserProgramConfiguration : IEntityTypeConfiguration<UserProgram>
{
    public void Configure(EntityTypeBuilder<UserProgram> builder)
    {
        builder.ToTable("user_programs", t =>
        {
            // CHECK values from the live prod schema dump (2026-09-07).
            t.HasCheckConstraint("CK_user_programs_muscle_group",
                "[muscle_group] IN (N'chest', N'arms', N'legs', N'core')");
        });
        builder.HasKey(p => p.Id).HasName("PK_user_programs");

        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.UserId).HasColumnName("user_id");
        builder.Property(p => p.ProgramName).HasColumnName("program_name").IsRequired().HasMaxLength(200);
        builder.Property(p => p.MuscleGroup).HasColumnName("muscle_group").IsRequired().HasMaxLength(50);
        builder.Property(p => p.IsActive).HasColumnName("is_active").HasDefaultValueSql("((1))");
        builder.Property(p => p.StartedAt).HasColumnName("started_at");

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .HasConstraintName("FK_user_programs_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.UserId).HasDatabaseName("IX_user_programs_user_id");
    }
}
