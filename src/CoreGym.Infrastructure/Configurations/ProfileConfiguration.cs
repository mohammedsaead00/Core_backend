using CoreGym.Domain.Entities;
using CoreGym.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class ProfileConfiguration : IEntityTypeConfiguration<Profile>
{
    public void Configure(EntityTypeBuilder<Profile> builder)
    {
        // Declares the updated_at trigger so EF avoids the OUTPUT clause on SaveChanges.
        // The role CHECK is NEW (original column is unconstrained text): values
        // confirmed from the app (client/coach) plus the legacy 'user' default.
        builder.ToTable("profiles", t =>
        {
            t.HasTrigger("trg_profiles_updated_at");
            t.HasCheckConstraint("CK_profiles_role",
                "[role] IN (N'client', N'coach', N'user')");
            // CHECK values from the live prod schema dump (2026-09-07).
            t.HasCheckConstraint("CK_profiles_gender",
                "[gender] IS NULL OR [gender] IN (N'male', N'female')");
            t.HasCheckConstraint("CK_profiles_fitness_goal",
                "[fitness_goal] IS NULL OR [fitness_goal] IN (N'muscle_gain', N'weight_loss', N'endurance', N'flexibility')");
        });
        builder.HasKey(p => p.Id).HasName("PK_profiles");

        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(200).HasDefaultValueSql("(N'')");
        builder.Property(p => p.Email).HasColumnName("email").HasMaxLength(320).HasDefaultValueSql("(N'')");
        builder.Property(p => p.Gender).HasColumnName("gender").HasMaxLength(50);
        builder.Property(p => p.Age).HasColumnName("age");
        builder.Property(p => p.WeightKg).HasColumnName("weight_kg").HasColumnType("decimal(5,2)");
        builder.Property(p => p.HeightCm).HasColumnName("height_cm").HasColumnType("decimal(5,1)");
        builder.Property(p => p.FitnessGoal).HasColumnName("fitness_goal").HasMaxLength(200);
        builder.Property(p => p.AvatarUrl).HasColumnName("avatar_url").HasMaxLength(500);
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");
        builder.Property(p => p.Role)
            .HasColumnName("role")
            .HasColumnType("nvarchar(20)")
            .HasConversion(
                role => role.ToString().ToLowerInvariant(),
                value => Enum.Parse<UserRole>(value, ignoreCase: true))
            .HasDefaultValueSql("(N'client')");
        builder.Property(p => p.FullName).HasColumnName("full_name").HasMaxLength(200);
    }
}
