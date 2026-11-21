// backend/src/Infrastructure/AI/FakeInterviewEvaluationService.cs
using Mockly.Domain.Interfaces;

namespace Mockly.Infrastructure.AI;

/// Implementación temporal del motor de evaluación.
/// Devuelve resultados simulados mientras no hay API key disponible.
public sealed class FakeInterviewEvaluationService : IInterviewEvaluationService
{
    public async Task<TechnicalScoreResult> EvaluateSubmissionAsync(
        string challengeDescription,
        string submittedCode,
        CancellationToken cancellationToken = default)
    {
        // Simula la latencia de una llamada real a un modelo de IA
        await Task.Delay(300, cancellationToken);

        // Puntaje simulado que reacciona al código enviado
        // (mismo código → misma nota, así el equipo puede probar)
        var score = 55;
        var code = submittedCode.ToLowerInvariant();

        if (code.Contains("function") || code.Contains("public") || code.Contains("class")) score += 15;
        if (code.Contains("return")) score += 10;
        if (code.Contains("test")) score += 5;
        if (submittedCode.Length > 200) score += 10;

        score = Math.Clamp(score, 0, 100);

        // challengeDescription se usará cuando conectemos la IA real
        return new TechnicalScoreResult(
            OverallScore: score,
            CodeQualityScore: Math.Clamp(score + 5, 0, 100),
            ProblemSolvingScore: Math.Clamp(score - 5, 0, 100),
            ArchitectureScore: Math.Clamp(score - 10, 0, 100),
            Summary: "Evaluación simulada (mock). Reemplazar por OpenAI cuando haya API key.",
            Strengths: new[] { "Escribió código completo.", "Usó estructura de control." },
            Improvements: new[] { "Agregá tests unitarios.", "Documentá las funciones." });
    }
}
