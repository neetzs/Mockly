// backend/src/Domain/Entities/InterviewSession.cs
using Mockly.Domain.Common;
using Mockly.Domain.Enums;
using Mockly.Domain.Exceptions;

namespace Mockly.Domain.Entities;

/// Instancia concreta donde un candidato resuelve un reto técnico y recibe evaluación por IA.
public class InterviewSession : BaseEntity
{
    public Guid CandidateId { get; private set; }
    public User Candidate { get; private set; } = null!;

    public Guid JobOpeningId { get; private set; }
    public JobOpening JobOpening { get; private set; } = null!;

    public Guid TechnicalChallengeId { get; private set; }
    public TechnicalChallenge TechnicalChallenge { get; private set; } = null!;

    public InterviewStatus Status { get; private set; } = InterviewStatus.NotStarted;

    /// Solución (CODIGO) escrita y enviada por el candidato.
    public string SubmittedCode { get; private set; } = string.Empty;

    /// Calificación general consolidada sobre 100 puntos calculada por el motor LLM
    public int? OverallScore { get; private set; }

    /// Almacena el desglose detallado de métricas (código limpio, algoritmos) en formato JSON nativo de PostgreSQL.
    public string? AiFeedbackJson { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    private InterviewSession() { }

    public InterviewSession(Guid candidateId, Guid jobOpeningId, Guid technicalChallengeId)
    {
        CandidateId = candidateId;
        JobOpeningId = jobOpeningId;
        TechnicalChallengeId = technicalChallengeId;
    }

    /// Pasa la sesión a InProgress. Si ya estaba en curso no hace nada (idempotente),
    /// así el candidato puede recargar la página de la sala sin recibir un error.
    public void Start()
    {
        switch (Status)
        {
            case InterviewStatus.InProgress:
                return;
            case InterviewStatus.Completed or InterviewStatus.Failed:
                throw new DomainException("La entrevista ya finalizó y no se puede volver a iniciar.");
        }

        Status = InterviewStatus.InProgress;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// Registra la solución evaluada. Solo es válido sobre una sesión en curso:
    /// impide reenviar código (y pisar la nota) en una entrevista ya terminada.
    public void Complete(string submittedCode, int overallScore, string aiFeedbackJson)
    {
        EnsureInProgress();

        SubmittedCode = submittedCode;
        OverallScore = Math.Clamp(overallScore, 0, 100);
        AiFeedbackJson = aiFeedbackJson;
        Status = InterviewStatus.Completed;
        CompletedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Fail()
    {
        EnsureInProgress();

        Status = InterviewStatus.Failed;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    /// Guard clause reutilizable: corta la operación si la sesión no está en curso.
    public void EnsureInProgress()
    {
        if (Status != InterviewStatus.InProgress)
        {
            throw new DomainException(Status switch
            {
                InterviewStatus.NotStarted => "La entrevista todavía no fue iniciada.",
                _ => "La entrevista ya finalizó y no admite nuevos envíos."
            });
        }
    }
}