using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class CoachContentConfiguration : IEntityTypeConfiguration<CoachContent>
{
    public void Configure(EntityTypeBuilder<CoachContent> builder)
    {
        builder.ToTable("coach_content");
        builder.HasKey(cc => cc.Id).HasName("PK_coach_content");

        builder.Property(cc => cc.Id).HasColumnName("id");
        builder.Property(cc => cc.CoachId).HasColumnName("coach_id");
        builder.Property(cc => cc.Title).HasColumnName("title").IsRequired().HasMaxLength(200);
        builder.Property(cc => cc.Description).HasColumnName("description");
        builder.Property(cc => cc.Type).HasColumnName("type").IsRequired().HasMaxLength(50);
        builder.Property(cc => cc.FileUrl).HasColumnName("file_url").IsRequired().HasMaxLength(500);
        builder.Property(cc => cc.IsPublic).HasColumnName("is_public").HasDefaultValue(false);
        builder.Property(cc => cc.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");
        builder.Property(cc => cc.ThumbnailUrl).HasColumnName("thumbnail_url").HasMaxLength(500);
        builder.Property(cc => cc.FileSizeKb).HasColumnName("file_size_kb");
        builder.Property(cc => cc.SortOrder).HasColumnName("sort_order").HasDefaultValue(0);

        builder.HasOne(cc => cc.Coach)
            .WithMany()
            .HasForeignKey(cc => cc.CoachId)
            .HasConstraintName("FK_coach_content_coaches")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cc => cc.CoachId).HasDatabaseName("IX_coach_content_coach_id");
    }
}
