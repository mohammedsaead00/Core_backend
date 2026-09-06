using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class CoachProfileConfiguration : IEntityTypeConfiguration<CoachProfile>
{
    public void Configure(EntityTypeBuilder<CoachProfile> builder)
    {
        // Declares the updated_at trigger so EF avoids the OUTPUT clause on SaveChanges.
        builder.ToTable("coach_profiles", t =>
        {
            t.HasCheckConstraint("CK_coach_profiles_specialties_json",
                "[specialties] IS NULL OR ISJSON([specialties]) = 1");
            t.HasCheckConstraint("CK_coach_profiles_certifications_json",
                "[certifications] IS NULL OR ISJSON([certifications]) = 1");
            t.HasTrigger("trg_coach_profiles_updated_at");
        });
        builder.HasKey(cp => cp.Id).HasName("PK_coach_profiles");

        // PK IS the auth user id (per the original schema).
        builder.Property(cp => cp.Id).HasColumnName("id");
        builder.Property(cp => cp.Bio).HasColumnName("bio");
        builder.Property(cp => cp.BioAr).HasColumnName("bio_ar");
        builder.Property(cp => cp.Specialties).HasColumnName("specialties").AsJsonArray();
        builder.Property(cp => cp.Certifications).HasColumnName("certifications").AsJsonArray();
        builder.Property(cp => cp.ExperienceYears).HasColumnName("experience_years").HasDefaultValue(0);
        builder.Property(cp => cp.PricePerMonth).HasColumnName("price_per_month").HasColumnType("decimal(10,2)").HasDefaultValue(0m);
        builder.Property(cp => cp.Currency).HasColumnName("currency").HasMaxLength(10).HasDefaultValueSql("(N'EGP')");
        builder.Property(cp => cp.Rating).HasColumnName("rating").HasColumnType("decimal(3,2)").HasDefaultValue(0m);
        builder.Property(cp => cp.ReviewsCount).HasColumnName("reviews_count").HasDefaultValue(0);
        builder.Property(cp => cp.IsAvailable).HasColumnName("is_available").HasDefaultValueSql("((1))");
        builder.Property(cp => cp.IsVerified).HasColumnName("is_verified").HasDefaultValue(false);
        builder.Property(cp => cp.CoverImageUrl).HasColumnName("cover_image_url").HasMaxLength(500);
        builder.Property(cp => cp.InstagramUrl).HasColumnName("instagram_url").HasMaxLength(500);
        builder.Property(cp => cp.YoutubeUrl).HasColumnName("youtube_url").HasMaxLength(500);
        builder.Property(cp => cp.MaxClients).HasColumnName("max_clients").HasDefaultValueSql("((20))");
        builder.Property(cp => cp.CurrentClients).HasColumnName("current_clients").HasDefaultValue(0);
        builder.Property(cp => cp.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");
        builder.Property(cp => cp.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        builder.HasOne(cp => cp.User)
            .WithMany()
            .HasForeignKey(cp => cp.Id)
            .HasConstraintName("FK_coach_profiles_profiles")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
