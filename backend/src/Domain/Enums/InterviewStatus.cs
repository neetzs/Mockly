// backend/src/Domain/Enums/InterviewStatus.cs
namespace Mockly.Domain.Enums;

/// Ciclo de vida por el cual transita una sesión de entrevista interactiva.
public enum InterviewStatus
{
    NotStarted = 1,
    InProgress = 2,
    Completed = 3,
    Failed = 4
}