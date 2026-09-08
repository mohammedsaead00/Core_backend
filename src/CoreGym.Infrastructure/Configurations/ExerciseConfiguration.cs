using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("exercises", t =>
        {
            t.HasCheckConstraint("CK_exercises_secondary_muscles_json",
                "[secondary_muscles] IS NULL OR ISJSON([secondary_muscles]) = 1");
            // CHECK values from the live prod schema dump (2026-09-07).
            t.HasCheckConstraint("CK_exercises_muscle_group",
                "[muscle_group] IS NULL OR [muscle_group] IN (N'chest', N'back', N'shoulders', N'arms', N'legs', N'core', N'full_body', N'cardio')");
            t.HasCheckConstraint("CK_exercises_category",
                "[category] IS NULL OR [category] IN (N'compound', N'isolation', N'cardio', N'stretching')");
        });
        builder.HasKey(e => e.Id).HasName("PK_exercises");

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Name).HasColumnName("name").IsRequired().HasMaxLength(200);
        builder.Property(e => e.NameAr).HasColumnName("name_ar").HasMaxLength(200);
        builder.Property(e => e.MuscleGroup).HasColumnName("muscle_group").HasMaxLength(50);
        builder.Property(e => e.SecondaryMuscles).HasColumnName("secondary_muscles").AsJsonArray();
        builder.Property(e => e.Equipment).HasColumnName("equipment").HasMaxLength(50);
        builder.Property(e => e.Category).HasColumnName("category").HasMaxLength(50);
        builder.Property(e => e.Instructions).HasColumnName("instructions");
        builder.Property(e => e.InstructionsAr).HasColumnName("instructions_ar");
        builder.Property(e => e.Tips).HasColumnName("tips");
        builder.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");
        builder.Property(e => e.ImageUrl).HasColumnName("image_url").HasMaxLength(500);
        builder.Property(e => e.YoutubeVideoId).HasColumnName("youtube_video_id").HasMaxLength(50);
        builder.Property(e => e.GifUrl).HasColumnName("gif_url").HasMaxLength(500);
    }
}
