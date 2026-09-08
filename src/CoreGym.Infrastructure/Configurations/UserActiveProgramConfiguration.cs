using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class UserActiveProgramConfiguration : IEntityTypeConfiguration<UserActiveProgram>
{
    public void Configure(EntityTypeBuilder<UserActiveProgram> builder)
    {
        // Declares the updated_at trigger so EF avoids the OUTPUT clause on SaveChanges.
        builder.ToTable("user_active_program", t =>
        {
            t.HasTrigger("trg_user_active_program_updated_at");
        });
        builder.HasKey(p => p.Id).HasName("PK_user_active_program");

        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.UserId).HasColumnName("user_id");
        builder.Property(p => p.ProgramId).HasColumnName("program_id");
        builder.Property(p => p.StartedAt).HasColumnName("started_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");
        builder.Property(p => p.CurrentWeek).HasColumnName("current_week").HasDefaultValueSql("((1))");
        builder.Property(p => p.CurrentDay).HasColumnName("current_day").HasDefaultValueSql("((1))");
        builder.Property(p => p.Notes).HasColumnName("notes");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        builder.HasOne(p => p.User)
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .HasConstraintName("FK_user_active_program_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Program)
            .WithMany()
            .HasForeignKey(p => p.ProgramId)
            .HasConstraintName("FK_user_active_program_training_programs")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.UserId).IsUnique().HasDatabaseName("IX_user_active_program_user_id");
    }
}
