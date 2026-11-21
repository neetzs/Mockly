// backend/src/Application/Common/Interfaces/IInterviewNotifier.cs
using Mockly.Application.Features.Recruiter.DTOs;

namespace Mockly.Application.Common.Interfaces;

/// Abstracción para emitir eventos en tiempo real desde los casos de uso.
/// Application no sabe que existe SignalR: solo "avisa". La implementación vive en WebApi
/// (SignalRInterviewNotifier), igual que IApplicationDbContext esconde a EF Core.
public interface IInterviewNotifier
{
    /// Avisa al reclutador dueño de la búsqueda que un candidato terminó su entrevista.
    Task NotifyInterviewCompletedAsync(Guid recruiterId, InterviewCompletedEvent notification, CancellationToken cancellationToken = default);
}
