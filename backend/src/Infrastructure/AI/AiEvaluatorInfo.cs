// backend/src/Infrastructure/AI/AiEvaluatorInfo.cs
namespace Mockly.Infrastructure.AI;

/// Qué motor de evaluación quedó activo al arrancar. Program.cs lo loguea para que en cada ambiente
/// (local, Docker, AWS) se vea de un vistazo si la API key se tomó o si se cayó al evaluador simulado.
public sealed record AiEvaluatorInfo(string Provider, string? Model)
{
    public bool IsSimulated => Model is null;

    public override string ToString() => IsSimulated
        ? $"{Provider} (sin OpenAI:ApiKey configurada)"
        : $"{Provider}, modelo {Model}";
}
