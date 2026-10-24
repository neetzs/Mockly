// backend/src/Domain/Interfaces/IInterviewEvaluationService.cs
namespace Mockly.Domain.Interfaces;

/// Objeto inmutable que modela el resultado de la evaluación estructurada del LLM.
public sealed record TechnicalScoreResult(
    int OverallScore,
    int CodeQualityScore,
    int ProblemSolvingScore,
    int ArchitectureScore,
    string Summary,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> Improvements
);

/// Contrato para el motor de evaluación de código mediante IA.
public interface IInterviewEvaluationService
{
    Task<TechnicalScoreResult> EvaluateSubmissionAsync(
        string challengeDescription,
        string submittedCode,
        CancellationToken cancellationToken = default);
}