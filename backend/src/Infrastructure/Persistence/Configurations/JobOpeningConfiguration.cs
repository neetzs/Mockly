// backend/src/Infrastructure/Persistence/Configurations/JobOpeningConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mockly.Domain.Entities;

namespace Mockly.Infrastructure.Persistence.Configurations;

public sealed class JobOpeningConfiguration : IEntityTypeConfiguration<JobOpening>
{
    public void Configure(EntityTypeBuilder<JobOpening> builder)
    {
        builder.ToTable("JobOpenings");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(j => j.Description)
            .IsRequired()
            .HasMaxLength(4000);

        // Npgsql mapea un uint marcado como row version a la columna de sistema xmin.
        builder.Property(j => j.RowVersion)
            .IsRowVersion();

        // Relación: Una búsqueda laboral pertenece a un reclutador
        builder.HasOne(j => j.Recruiter)
            .WithMany(u => u.CreatedJobOpenings)
            .HasForeignKey(j => j.RecruiterId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}