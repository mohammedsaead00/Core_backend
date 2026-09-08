using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class PaymentIntentConfiguration : IEntityTypeConfiguration<PaymentIntent>
{
    public void Configure(EntityTypeBuilder<PaymentIntent> builder)
    {
        builder.ToTable("payment_intents", t =>
        {
            // CHECK value from the live prod schema dump (2026-09-07).
            t.HasCheckConstraint("CK_payment_intents_status",
                "[status] IN (N'pending', N'succeeded', N'failed', N'refunded')");
        });
        builder.HasKey(pi => pi.Id).HasName("PK_payment_intents");

        builder.Property(pi => pi.Id).HasColumnName("id");
        builder.Property(pi => pi.ClientId).HasColumnName("client_id");
        builder.Property(pi => pi.CoachId).HasColumnName("coach_id");
        builder.Property(pi => pi.StripePaymentId).HasColumnName("stripe_payment_id").IsRequired().HasMaxLength(100);
        builder.Property(pi => pi.StripeCustomerId).HasColumnName("stripe_customer_id").HasMaxLength(100);
        builder.Property(pi => pi.Amount).HasColumnName("amount").HasColumnType("decimal(12,2)");
        builder.Property(pi => pi.Currency).HasColumnName("currency").IsRequired().HasMaxLength(10).HasDefaultValueSql("(N'usd')");
        builder.Property(pi => pi.Status).HasColumnName("status").IsRequired().HasMaxLength(50).HasDefaultValueSql("(N'pending')");
        builder.Property(pi => pi.Tier).HasColumnName("tier").IsRequired().HasMaxLength(50).HasDefaultValueSql("(N'standard')");
        builder.Property(pi => pi.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("(SYSDATETIMEOFFSET())");

        builder.HasOne(pi => pi.Client)
            .WithMany()
            .HasForeignKey(pi => pi.ClientId)
            .HasConstraintName("FK_payment_intents_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(pi => pi.Coach)
            .WithMany()
            .HasForeignKey(pi => pi.CoachId)
            .HasConstraintName("FK_payment_intents_coaches")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(pi => pi.ClientId).HasDatabaseName("IX_payment_intents_client_id");
        builder.HasIndex(pi => pi.CoachId).HasDatabaseName("IX_payment_intents_coach_id");
        // Webhook events arrive by stripe payment id.
        builder.HasIndex(pi => pi.StripePaymentId).HasDatabaseName("IX_payment_intents_stripe_payment_id");
    }
}
