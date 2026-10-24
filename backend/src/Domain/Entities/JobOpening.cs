// backend/src/Domain/Entities/JobOpening.cs
using Mockly.Domain.Common;

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

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}