using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class CoachConfiguration : IEntityTypeConfiguration<Coach>
{
    public void Configure(EntityTypeBuilder<Coach> builder)
    {
        // Declares the updated_at trigger so EF avoids the OUTPUT clause on SaveChanges.
        builder.ToTable("coaches", t =>
        {
            t.HasCheckConstraint("CK_coaches_specialization_json",
                "[specialization] IS NULL OR ISJSON([specialization]) = 1");
            t.HasTrigger("trg_coaches_updated_at");
        });
        builder.HasKey(c => c.Id).HasName("PK_coaches");

        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.UserId).HasColumnName("user_id");
        builder.Property(c => c.Bio).HasColumnName("bio").IsRequired().HasDefaultValueSql("(N'')");
        builder.Property(c => c.PriceMonthly).HasColumnName("price_monthly").HasColumnType("decimal(10,2)").HasDefaultValue(0m);
        builder.Property(c => c.Specialization).HasColumnName("specialization").AsJsonArray();
        builder.Property(c => c.Rating).HasColumnName("rating").HasColumnType("decimal(3,2)").HasDefaultValue(0m);
        builder.Property(c => c.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValueSql("((1))");
        builder.Property(c => c.StripeAccountId).HasColumnName("stripe_account_id").HasMaxLength(100);
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        builder.HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .HasConstraintName("FK_coaches_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.UserId).IsUnique().HasDatabaseName("IX_coaches_user_id");
    }
}
