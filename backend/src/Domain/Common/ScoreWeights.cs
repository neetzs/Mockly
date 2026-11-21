// backend/src/Domain/Common/ScoreWeights.cs
namespace Mockly.Domain.Common;

/// Regla de negocio de Mockly: cómo se arma el puntaje general a partir de los tres criterios.
/// Vive en el dominio (y no en el prompt) para que el resultado sea exacto y siempre el mismo:
/// la IA solo califica cada criterio y la cuenta la hace el backend.
/// La landing muestra esta misma ponderación.
public static class ScoreWeights
{
    // Pesos en porcentaje (suman 100). Enteros a propósito: con double, 82 * 0.3 da 24.5999...
    // y un 81,5 exacto podría redondear para abajo. Con enteros la cuenta es exacta.
    public const int ProblemSolving = 50;
    public const int CodeQuality = 30;
    public const int Architecture = 20;

    /// Promedio ponderado redondeado al entero más cercano (x,5 redondea hacia arriba).
    /// Ej.: (85, 82, 72) → 4250 + 2460 + 1440 = 8150 centésimos → 81,5 → 82.
    public static int CalculateOverall(int problemSolving, int codeQuality, int architecture)
    {
        var hundredths = problemSolving * ProblemSolving + codeQuality * CodeQuality + architecture * Architecture;
        return (hundredths + 50) / 100; // +50 antes de la división entera = redondeo de la mitad hacia arriba
    }
}
