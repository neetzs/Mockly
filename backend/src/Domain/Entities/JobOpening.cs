// backend/src/Domain/Entities/JobOpening.cs
using Mockly.Domain.Common;
using Mockly.Domain.Enums;
using Mockly.Domain.Exceptions;

namespace Mockly.Domain.Entities;

/// Posición o búsqueda laboral creada por un reclutador que agrupa retos técnicos.
public class JobOpening : BaseEntity
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Guid RecruiterId { get; private set; }
    public User Recruiter { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;

    // Colecciones asociadas
    public ICollection<TechnicalChallenge> Challenges { get; private set; } = new List<TechnicalChallenge>();
    public ICollection<InterviewSession> InterviewSessions { get; private set; } = new List<InterviewSession>();

    private JobOpening() { }

    public JobOpening(string title, string description, Guid recruiterId)
    {
        Title = title.Trim();
        Description = description.Trim();
        RecruiterId = recruiterId;
    }

    // Límite razonable para una entrevista: más retos alargan demasiado la evaluación del candidato.
    public const int MaxChallenges = 5;

    /// Agrega a la búsqueda una copia de un reto de la biblioteca.
    public void AddChallengeFromLibrary(TechnicalChallenge libraryChallenge)
    {
        if (Challenges.Count >= MaxChallenges)
        {
            throw new DomainException($"Una búsqueda puede tener como máximo {MaxChallenges} retos.");
        }

        Challenges.Add(libraryChallenge.CopyForJobOpening(Id));
    }

    /// Invita a un candidato: le crea una entrevista pendiente por cada reto de la búsqueda.
    /// Devuelve las sesiones nuevas para que el caso de uso las agregue al DbContext de forma explícita.
    public IReadOnlyList<InterviewSession> Invite(User candidate)
    {
        if (!IsActive)
        {
            throw new DomainException("La búsqueda está cerrada y no admite nuevos candidatos.");
        }

        if (candidate.Role != UserRole.Candidate)
        {
            throw new DomainException("Solo se pueden invitar usuarios con rol de candidato.");
        }

        if (Challenges.Count == 0)
        {
            throw new DomainException("La búsqueda no tiene retos para asignar.");
        }

        if (InterviewSessions.Any(s => s.CandidateId == candidate.Id))
        {
            throw new DomainException("Ese candidato ya está invitado a esta búsqueda.");
        }

        // Las copias se crean en el orden que eligió el reclutador, así que CreatedAtUtc conserva ese orden.
        var sessions = Challenges
            .OrderBy(c => c.CreatedAtUtc)
            .Select(challenge => new InterviewSession(candidate.Id, Id, challenge.Id))
            .ToList();

        foreach (var session in sessions)
        {
            InterviewSessions.Add(session);
        }

        UpdatedAtUtc = DateTime.UtcNow;
        return sessions;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}