// backend/src/Domain/Entities/User.cs
using Mockly.Domain.Common;
using Mockly.Domain.Enums;

namespace Mockly.Domain.Entities;

/// Representa a cualquier usuario registrado en Mockly (Candidato o Reclutador).
public class User : BaseEntity
{
    
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }

    // Propiedades de navegación de Entity Framework Core
    public ICollection<InterviewSession> CandidateInterviews { get; private set; } = new List<InterviewSession>();
    public ICollection<JobOpening> CreatedJobOpenings { get; private set; } = new List<JobOpening>();

    // Constructor privado requerido por los mecanismos de reflexión de EF Core
    private User() { }

    public User(string email, string passwordHash, string fullName, UserRole role)
    {
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        FullName = fullName.Trim();
        Role = role;
    }
}