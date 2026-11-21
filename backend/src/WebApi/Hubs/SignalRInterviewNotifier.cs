// backend/src/WebApi/Hubs/SignalRInterviewNotifier.cs
using Microsoft.AspNetCore.SignalR;
using Mockly.Application.Common.Interfaces;
using Mockly.Application.Features.Recruiter.DTOs;

namespace Mockly.WebApi.Hubs;

/// Implementación de IInterviewNotifier con SignalR.
/// IHubContext permite enviar mensajes a los clientes del hub desde FUERA del hub
/// (en este caso, desde InterviewService cuando termina una evaluación).
public sealed class SignalRInterviewNotifier : IInterviewNotifier
{
    private readonly IHubContext<InterviewHub> _hubContext;
    private readonly ILogger<SignalRInterviewNotifier> _logger;

    public SignalRInterviewNotifier(IHubContext<InterviewHub> hubContext, ILogger<SignalRInterviewNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task NotifyInterviewCompletedAsync(Guid recruiterId, InterviewCompletedEvent notification, CancellationToken cancellationToken = default)
    {
        try
        {
            await _hubContext.Clients
                .Group(InterviewHub.RecruiterGroup(recruiterId))
                .SendAsync(InterviewHub.InterviewCompletedEvent, notification, cancellationToken);
        }
        catch (Exception ex)
        {
            // La notificación es un "extra": si falla, el candidato igual tiene que recibir su nota
            // (el resultado ya está guardado y el dashboard lo ve al recargar). Por eso logueamos y seguimos.
            _logger.LogWarning(ex, "No se pudo notificar en tiempo real la sesión {SessionId}.", notification.Entry.SessionId);
        }
    }
}
