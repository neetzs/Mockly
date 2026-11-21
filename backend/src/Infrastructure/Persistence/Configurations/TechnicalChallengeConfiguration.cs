// backend/src/Infrastructure/Persistence/Configurations/TechnicalChallengeConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mockly.Domain.Entities;
using Mockly.Domain.Enums;

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

        // Los retos que existían antes de esta columna eran todos de C#: ese es el default.
        builder.Property(t => t.Language)
            .IsRequired()
            .HasConversion<int>()
            .HasDefaultValue(ProgrammingLanguage.CSharp);

        // FK opcional: los retos de la biblioteca de práctica no tienen búsqueda.
        // Si se elimina la búsqueda laboral, se eliminan sus retos en cascada.
        builder.HasOne(t => t.JobOpening)
            .WithMany(j => j.Challenges)
            .HasForeignKey(t => t.JobOpeningId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}