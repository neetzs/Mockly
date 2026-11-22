// backend/src/Application/Features/Recruiter/DTOs/RecruiterDtos.cs
using System.Text.Json;
using Mockly.Application.Features.Interviews.DTOs;
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

/// Reto de la biblioteca para elegir al crear una búsqueda
public sealed record LibraryChallengeResponse(
    Guid Id,
    string Title,
    string Description,
    string Difficulty,
    string Language
);

public sealed record CreateJobOpeningRequest(
    string Title,
    string Description,
    IReadOnlyList<Guid> ChallengeIds
);

public sealed record InviteCandidateRequest(string Email);

/// Detalle de una búsqueda: sus retos y el avance de cada candidato invitado
public sealed record JobOpeningDetailResponse(
    Guid Id,
    string Title,
    string Description,
    bool IsActive,
    IReadOnlyList<OpeningChallengeResponse> Challenges,
    IReadOnlyList<InvitedCandidateResponse> Candidates
);

public sealed record OpeningChallengeResponse(
    Guid Id,
    string Title,
    string Difficulty,
    string Language
);

/// Un candidato invitado, con una fila por entrevista (reto) para ver su progreso
public sealed record InvitedCandidateResponse(
    Guid CandidateId,
    string FullName,
    string Email,
    IReadOnlyList<CandidateSessionResponse> Sessions
);

public sealed record CandidateSessionResponse(
    Guid SessionId,
    string ChallengeTitle,
    string Status,
    int? OverallScore
);

/// Revisión de una entrevista: el código que envió el candidato y la devolución completa de la IA.
/// Le permite al reclutador decidir con criterio propio, no solo mirando el número.
public sealed record SessionReviewResponse(
    Guid SessionId,
    Guid JobOpeningId,
    string JobOpeningTitle,
    string CandidateName,
    string CandidateEmail,
    string ChallengeTitle,
    string ChallengeDescription,
    string Difficulty,
    string Language,
    string Status,
    string? SubmittedCode,
    EvaluationResponse? Evaluation,
    DateTime? CompletedAtUtc
);
