// backend/src/Domain/Enums/UserRole.cs
namespace Mockly.Domain.Enums;


// Define los roles de acceso a la plataforma para el control de autorización (RBAC).
public enum UserRole
{
    /// Candidato que realiza pruebas técnicas y recibe feedback en vivo.
    Candidate = 1,

    /// Reclutador o evaluador que visualiza métricas y rankings en tiempo real.
    Recruiter = 2
}