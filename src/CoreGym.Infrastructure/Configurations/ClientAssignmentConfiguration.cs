using CoreGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoreGym.Infrastructure.Configurations;

public class ClientAssignmentConfiguration : IEntityTypeConfiguration<ClientAssignment>
{
    public void Configure(EntityTypeBuilder<ClientAssignment> builder)
    {
        builder.ToTable("client_assignments");
        builder.HasKey(ca => ca.Id).HasName("PK_client_assignments");

        builder.Property(ca => ca.Id).HasColumnName("id");
        builder.Property(ca => ca.CoachId).HasColumnName("coach_id");
        builder.Property(ca => ca.ClientId).HasColumnName("client_id");
        builder.Property(ca => ca.ContentId).HasColumnName("content_id");
        builder.Property(ca => ca.Note).HasColumnName("note");
        builder.Property(ca => ca.AssignedAt).HasColumnName("assigned_at");

        builder.HasOne(ca => ca.Coach)
            .WithMany()
            .HasForeignKey(ca => ca.CoachId)
            .HasConstraintName("FK_client_assignments_coaches")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ca => ca.Client)
            .WithMany()
            .HasForeignKey(ca => ca.ClientId)
            .HasConstraintName("FK_client_assignments_profiles")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(ca => ca.Content)
            .WithMany()
            .HasForeignKey(ca => ca.ContentId)
            .HasConstraintName("FK_client_assignments_coach_content")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ca => ca.ClientId).HasDatabaseName("IX_client_assignments_client_id");
        builder.HasIndex(ca => ca.CoachId).HasDatabaseName("IX_client_assignments_coach_id");
        builder.HasIndex(ca => ca.ContentId).HasDatabaseName("IX_client_assignments_content_id");
    }
}
