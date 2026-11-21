// backend/src/Application/Common/Exceptions/ForbiddenAccessException.cs
namespace Mockly.Application.Common.Exceptions;

/// El usuario está autenticado (sabemos quién es) pero no tiene permiso sobre el recurso.
/// Diferencia clave: 401 = "no sé quién sos"; 403 = "sé quién sos, pero esto no es tuyo".
public sealed class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message = "No tenés permiso para acceder a este recurso.")
        : base(message) { }
}
