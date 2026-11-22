// backend/src/Application/Features/Interviews/DTOs/InterviewDtos.cs
using System.Text.Json;
using Mockly.Domain.Interfaces;

namespace Mockly.Application.Features.Interviews.DTOs;

/// Datos de la sala que se cargan en el editor Monaco
public sealed record InterviewDetailResponse(
    Guid Id,
    string Status,
    Guid ChallengeId,
    string? JobOpeningTitle, // null = intento de práctica
    string ChallengeTitle,
    string ChallengeDescription,
    string InitialCodeTemplate,
    string Difficulty,
    string Language, // nombre del enum (CSharp, Python, Sql...): el frontend lo mapea al editor
    string? SubmittedCode,
    EvaluationResponse? Evaluation // solo viene cuando la entrevista está Completed
);

/// Fila del listado "Mis entrevistas" del candidato
public sealed record InterviewSummaryResponse(
    Guid Id,
    string Status,
    Guid ChallengeId,
    string? JobOpeningTitle, // null = intento de práctica
    string ChallengeTitle,
    string Difficulty,
    string Language,
    int? OverallScore,
    DateTime? CompletedAtUtc
);

/// Código enviado por el candidato para ser evaluado
public sealed record SubmitCodeRequest(
    string Code
);

/// Resultado estructurado de la evaluación por IA
public sealed record EvaluationResponse(
    int OverallScore,
    int CodeQualityScore,
    int ProblemSolvingScore,
    int ArchitectureScore,
    string Summary,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Improvements
)
{
    public static EvaluationResponse FromResult(TechnicalScoreResult result) =>
        new(
            result.OverallScore,
            result.CodeQualityScore,
            result.ProblemSolvingScore,
            result.ArchitectureScore,
            result.Summary,
            result.Strengths,
            result.Improvements);

    /// Reconstruye la evaluación desde el JSON guardado en la sesión (null si todavía no fue evaluada).
    public static EvaluationResponse? FromJson(string? aiFeedbackJson) =>
        aiFeedbackJson is { } json && JsonSerializer.Deserialize<TechnicalScoreResult>(json) is { } stored
            ? FromResult(stored)
            : null;
}

/// Archivo PDF del certificado listo para devolver como descarga
public sealed record CertificateFileResponse(
    byte[] Content,
    string FileName
);
