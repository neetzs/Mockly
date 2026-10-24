// backend/src/Domain/Common/BaseEntity.cs
namespace Mockly.Domain.Common;

/// Clase base abstracta que aporta identificador único global (GUID)
/// y marcas de tiempo en formato UTC para auditoría en base de datos.

public abstract class BaseEntity
{
    /// Identificador único universal (Primary Key).
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// Fecha y hora UTC en la que se persistió el registro por primera vez.
    public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;

    /// Fecha y hora UTC de la última modificación; nulo si nunca fue editado.
    public DateTime? UpdatedAtUtc { get; protected set; }
}