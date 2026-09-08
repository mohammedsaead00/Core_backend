using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class StripeCustomerConfiguration : IEntityTypeConfiguration<StripeCustomer>
{
    public void Configure(EntityTypeBuilder<StripeCustomer> builder)
    {
        builder.ToTable("stripe_customers");
        builder.HasKey(sc => sc.Id).HasName("PK_stripe_customers");

        builder.Property(sc => sc.Id).HasColumnName("id");
        builder.Property(sc => sc.UserId).HasColumnName("user_id");
        builder.Property(sc => sc.StripeCustomerId).HasColumnName("stripe_customer_id").IsRequired().HasMaxLength(100);
        builder.Property(sc => sc.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        builder.HasOne(sc => sc.User)
            .WithMany()
            .HasForeignKey(sc => sc.UserId)
            .HasConstraintName("FK_stripe_customers_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(sc => sc.UserId).IsUnique().HasDatabaseName("IX_stripe_customers_user_id");
        // Checkout flow looks up the customer id per user (UNIQUE in the live prod dump).
        builder.HasIndex(sc => sc.StripeCustomerId).IsUnique().HasDatabaseName("IX_stripe_customers_stripe_customer_id");
    }
}
