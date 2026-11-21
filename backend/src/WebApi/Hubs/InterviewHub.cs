// backend/src/WebApi/Hubs/InterviewHub.cs
using Microsoft.AspNetCore.SignalR;
using Mockly.WebApi.Extensions;

namespace Mockly.WebApi.Hubs;

/// Hub de SignalR del dashboard. Un "hub" es el punto de conexión WebSocket:
/// el navegador del reclutador se conecta una vez y el servidor le empuja eventos
/// cuando ocurren, sin que el cliente tenga que preguntar (polling).
///
/// Solo Recruiters (se exige en Program.cs con RequireAuthorization al mapear el hub).
/// El hub no expone métodos para el cliente: la comunicación es solo servidor → cliente.
public sealed class InterviewHub : Hub
{
    /// Nombre del evento que escucha el frontend: connection.on("InterviewCompleted", ...)
    public const string InterviewCompletedEvent = "InterviewCompleted";

    /// Grupo privado de cada reclutador. Así cada uno recibe solo los eventos de SUS búsquedas.
    public static string RecruiterGroup(Guid recruiterId) => $"recruiter:{recruiterId}";

    public override async Task OnConnectedAsync()
    {
        // Al conectarse, metemos la conexión en el grupo del reclutador (el id sale del JWT).
        // Si tiene dos pestañas abiertas, son dos conexiones en el mismo grupo: ambas reciben el evento.
        await Groups.AddToGroupAsync(Context.ConnectionId, RecruiterGroup(Context.User!.GetUserId()));
        await base.OnConnectedAsync();
    }
}
