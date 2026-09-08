using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class CoachOnboardingConfiguration : IEntityTypeConfiguration<CoachOnboarding>
{
    public void Configure(EntityTypeBuilder<CoachOnboarding> builder)
    {
        // Declares the updated_at trigger so EF avoids the OUTPUT clause on SaveChanges.
        builder.ToTable("coach_onboarding", t =>
        {
            t.HasCheckConstraint("CK_coach_onboarding_certifications_json",
                "[certifications] IS NULL OR ISJSON([certifications]) = 1");
            t.HasCheckConstraint("CK_coach_onboarding_specialization_json",
                "[specialization] IS NULL OR ISJSON([specialization]) = 1");
            t.HasCheckConstraint("CK_coach_onboarding_languages_json",
                "[languages] IS NULL OR ISJSON([languages]) = 1");
            t.HasCheckConstraint("CK_coach_onboarding_gallery_images_json",
                "[gallery_images] IS NULL OR ISJSON([gallery_images]) = 1");
            t.HasCheckConstraint("CK_coach_onboarding_pdf_urls_json",
                "[pdf_urls] IS NULL OR ISJSON([pdf_urls]) = 1");
            t.HasCheckConstraint("CK_coach_onboarding_certificate_files_json",
                "[certificate_files] IS NULL OR ISJSON([certificate_files]) = 1");
            t.HasCheckConstraint("CK_coach_onboarding_transformation_images_json",
                "[transformation_images] IS NULL OR ISJSON([transformation_images]) = 1");
            t.HasTrigger("trg_coach_onboarding_updated_at");
        });
        builder.HasKey(co => co.Id).HasName("PK_coach_onboarding");

        builder.Property(co => co.Id).HasColumnName("id");
        builder.Property(co => co.UserId).HasColumnName("user_id");
        builder.Property(co => co.DisplayName).HasColumnName("display_name").IsRequired().HasMaxLength(200).HasDefaultValueSql("(N'')");
        builder.Property(co => co.YearsExperience).HasColumnName("years_experience").HasDefaultValue(0);
        builder.Property(co => co.Certifications).HasColumnName("certifications").IsRequired().AsJsonArray();
        builder.Property(co => co.Specialization).HasColumnName("specialization").IsRequired().AsJsonArray();
        builder.Property(co => co.Bio).HasColumnName("bio").IsRequired().HasDefaultValueSql("(N'')");
        builder.Property(co => co.PriceMonthly).HasColumnName("price_monthly").IsRequired().HasColumnType("decimal(10,2)").HasDefaultValue(0m);
        builder.Property(co => co.PricePremium).HasColumnName("price_premium").IsRequired().HasColumnType("decimal(10,2)").HasDefaultValue(0m);
        builder.Property(co => co.Languages).HasColumnName("languages").IsRequired().AsJsonArray()
            .HasDefaultValueSql("(N'[\"Arabic\",\"English\"]')");
        builder.Property(co => co.MaxClients).HasColumnName("max_clients").IsRequired().HasDefaultValueSql("((10))");
        builder.Property(co => co.ProfileImageUrl).HasColumnName("profile_image_url").HasMaxLength(500);
        builder.Property(co => co.IntroVideoUrl).HasColumnName("intro_video_url").HasMaxLength(500);
        builder.Property(co => co.IsCompleted).HasColumnName("is_completed").HasDefaultValue(false);
        builder.Property(co => co.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");
        builder.Property(co => co.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");
        builder.Property(co => co.PhoneNumber).HasColumnName("phone_number").HasMaxLength(30);
        builder.Property(co => co.City).HasColumnName("city").HasMaxLength(100);
        builder.Property(co => co.Gender).HasColumnName("gender").HasMaxLength(50);
        builder.Property(co => co.GalleryImages).HasColumnName("gallery_images").AsJsonArray();
        builder.Property(co => co.PdfUrls).HasColumnName("pdf_urls").AsJsonArray();
        builder.Property(co => co.CertificateFiles).HasColumnName("certificate_files").AsJsonArray();
        builder.Property(co => co.TransformationImages).HasColumnName("transformation_images").AsJsonArray();

        builder.HasOne(co => co.User)
            .WithMany()
            .HasForeignKey(co => co.UserId)
            .HasConstraintName("FK_coach_onboarding_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(co => co.UserId).HasDatabaseName("IX_coach_onboarding_user_id");
    }
}
