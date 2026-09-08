using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class TrainingProgramConfiguration : IEntityTypeConfiguration<TrainingProgram>
{
    public void Configure(EntityTypeBuilder<TrainingProgram> builder)
    {
        builder.ToTable("training_programs", t =>
        {
            // CHECK values from the live prod schema dump (2026-09-07).
            t.HasCheckConstraint("CK_training_programs_level",
                "[level] IS NULL OR [level] IN (N'beginner', N'intermediate', N'advanced')");
            t.HasCheckConstraint("CK_training_programs_goal",
                "[goal] IS NULL OR [goal] IN (N'strength', N'muscle_gain', N'weight_loss', N'endurance', N'general_fitness')");
        });
        builder.HasKey(p => p.Id).HasName("PK_training_programs");

        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(p => p.NameAr).HasColumnName("name_ar").HasMaxLength(200);
        builder.Property(p => p.Description).HasColumnName("description");
        builder.Property(p => p.DescriptionAr).HasColumnName("description_ar");
        builder.Property(p => p.Level).HasColumnName("level").HasMaxLength(50);
        builder.Property(p => p.Goal).HasColumnName("goal").HasMaxLength(50);
        builder.Property(p => p.DaysPerWeek).HasColumnName("days_per_week");
        builder.Property(p => p.DurationWeeks).HasColumnName("duration_weeks");
        builder.Property(p => p.SplitType).HasColumnName("split_type").HasMaxLength(50);
        builder.Property(p => p.IsActive).HasColumnName("is_active").HasDefaultValueSql("((1))");
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");
    }
}
