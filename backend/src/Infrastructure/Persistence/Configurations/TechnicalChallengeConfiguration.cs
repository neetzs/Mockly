// backend/src/Infrastructure/Persistence/Configurations/TechnicalChallengeConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mockly.Domain.Entities;

namespace Mockly.Infrastructure.Persistence.Configurations;

public sealed class TechnicalChallengeConfiguration : IEntityTypeConfiguration<TechnicalChallenge>
{
    public void Configure(EntityTypeBuilder<TechnicalChallenge> builder)
    {
        builder.ToTable("TechnicalChallenges");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(5000);

        // Mapeo a columna 'text' ilimitada en PostgreSQL para soportar plantillas de código extensas
        builder.Property(t => t.InitialCodeTemplate)
            .IsRequired()
            .HasColumnType("text");

        builder.Property(t => t.Difficulty)
            .IsRequired()
            .HasConversion<int>();

        // Si se elimina la búsqueda laboral, se eliminan sus retos en cascada
        builder.HasOne(t => t.JobOpening)
            .WithMany(j => j.Challenges)
            .HasForeignKey(t => t.JobOpeningId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}