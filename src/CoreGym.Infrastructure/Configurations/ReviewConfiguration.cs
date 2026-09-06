using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");
        builder.HasKey(r => r.Id).HasName("PK_reviews");

        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.ClientId).HasColumnName("client_id");
        builder.Property(r => r.CoachId).HasColumnName("coach_id");
        builder.Property(r => r.Rating).HasColumnName("rating");
        builder.Property(r => r.Comment).HasColumnName("comment");
        builder.Property(r => r.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        builder.HasOne(r => r.Client)
            .WithMany()
            .HasForeignKey(r => r.ClientId)
            .HasConstraintName("FK_reviews_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Coach)
            .WithMany()
            .HasForeignKey(r => r.CoachId)
            .HasConstraintName("FK_reviews_coaches")
            .OnDelete(DeleteBehavior.Restrict);

        // Rating aggregates (coach detail/dashboard) and a client's own reviews.
        builder.HasIndex(r => r.CoachId).HasDatabaseName("IX_reviews_coach_id");
        builder.HasIndex(r => r.ClientId).HasDatabaseName("IX_reviews_client_id");
    }
}
