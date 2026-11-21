// backend/src/Application/Features/Interviews/DTOs/InterviewDtos.cs
namespace Mockly.Application.Features.Interviews.DTOs;

/// Datos de la sala que se cargan en el editor Monaco
public sealed record InterviewDetailResponse(
    Guid Id,
    string Status,
    string ChallengeTitle,
    string ChallengeDescription,
    string InitialCodeTemplate,
    string Difficulty,
    string? SubmittedCode,
    EvaluationResponse? Evaluation // solo viene cuando la entrevista está Completed
);

/// Fila del listado "Mis entrevistas" del candidato
public sealed record InterviewSummaryResponse(
    Guid Id,
    string Status,
    string ChallengeTitle,
    string Difficulty,
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
);

/// Archivo PDF del certificado listo para devolver como descarga
public sealed record CertificateFileResponse(
    byte[] Content,
    string FileName
);
