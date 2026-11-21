// backend/src/Application/Features/Recruiter/DTOs/RecruiterDtos.cs
using System.Text.Json;
using Mockly.Domain.Entities;
using Mockly.Domain.Interfaces;

namespace Mockly.Application.Features.Recruiter.DTOs;

/// Búsqueda laboral del reclutador, para el selector del dashboard
public sealed record JobOpeningSummaryResponse(
    Guid Id,
    string Title,
    bool IsActive,
    int CompletedInterviews
);

/// Fila del ranking: una entrevista completada con su desglose de puntajes (para el radar)
public sealed record CandidateRankingResponse(
    Guid SessionId,
    Guid CandidateId,
    string CandidateName,
    string CandidateEmail,
    string ChallengeTitle,
    string Difficulty,
    string Language,
    int OverallScore,
    int CodeQualityScore,
    int ProblemSolvingScore,
    int ArchitectureScore,
    DateTime CompletedAtUtc
)
{
    /// Arma la fila a partir de una sesión completada (con Candidate y TechnicalChallenge cargados).
    /// El desglose se lee del JSON que guardó el motor de IA.
    public static CandidateRankingResponse FromSession(InterviewSession session)
    {
        var scores = session.AiFeedbackJson is { } json
            ? JsonSerializer.Deserialize<TechnicalScoreResult>(json)
            : null;

        return new CandidateRankingResponse(
            session.Id,
            session.CandidateId,
            session.Candidate.FullName,
            session.Candidate.Email,
            session.TechnicalChallenge.Title,
            session.TechnicalChallenge.Difficulty.ToString(),
            session.TechnicalChallenge.Language.ToString(),
            session.OverallScore ?? 0,
            scores?.CodeQualityScore ?? 0,
            scores?.ProblemSolvingScore ?? 0,
            scores?.ArchitectureScore ?? 0,
            session.CompletedAtUtc ?? session.UpdatedAtUtc ?? session.CreatedAtUtc);
    }
}

/// Payload del evento en tiempo real "InterviewCompleted"
public sealed record InterviewCompletedEvent(
    Guid JobOpeningId,
    CandidateRankingResponse Entry
);
