// backend/src/Infrastructure/Persistence/Configurations/InterviewSessionConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mockly.Domain.Entities;

namespace Mockly.Infrastructure.Persistence.Configurations;

public sealed class InterviewSessionConfiguration : IEntityTypeConfiguration<InterviewSession>
{
    public void Configure(EntityTypeBuilder<InterviewSession> builder)
    {
        builder.ToTable("InterviewSessions");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(s => s.SubmittedCode)
            .HasColumnType("text");

        // Almacenamiento JSON binario optimizado nativo de PostgreSQL para el reporte del LLM
        builder.Property(s => s.AiFeedbackJson)
            .HasColumnType("jsonb");

        // Reglas de integridad referencial para evitar eliminaciones accidentales de historial
        builder.HasOne(s => s.Candidate)
            .WithMany(u => u.CandidateInterviews)
            .HasForeignKey(s => s.CandidateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.JobOpening)
            .WithMany(j => j.InterviewSessions)
            .HasForeignKey(s => s.JobOpeningId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.TechnicalChallenge)
            .WithMany()
            .HasForeignKey(s => s.TechnicalChallengeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}