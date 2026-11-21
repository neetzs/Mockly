// backend/src/Domain/Exceptions/DomainException.cs
namespace Mockly.Domain.Exceptions;

/// Se lanza cuando una operación viola una regla de negocio del dominio
/// (por ejemplo, enviar código en una entrevista que ya terminó).
/// La WebApi la traduce a HTTP 409 Conflict.
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
