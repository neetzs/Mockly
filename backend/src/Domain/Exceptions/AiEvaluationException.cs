// backend/src/Domain/Exceptions/AiEvaluationException.cs
namespace Mockly.Domain.Exceptions;

/// El motor de evaluación no pudo calificar el código (proveedor caído, cuota agotada,
/// respuesta con formato inválido). No es culpa del candidato: la sesión sigue en curso
/// y puede reintentar. La WebApi la traduce a HTTP 503 Service Unavailable.
public sealed class AiEvaluationException : Exception
{
    public AiEvaluationException(string message) : base(message) { }
}
