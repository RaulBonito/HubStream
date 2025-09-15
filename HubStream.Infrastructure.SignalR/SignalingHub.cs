using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

// He movido el Hub fuera de un namespace para simplificar, ajústalo al tuyo.
public class SignalingHub : Hub
{
    // NUEVO: Un diccionario estático para rastrear los hosts activos.
    // La clave es el ConnectionId del host, el valor es el nombre que eligió.
    private static readonly ConcurrentDictionary<string, string> ActiveHosts = new();

    // El Host inicia la transmisión.
    // MODIFICADO: Ahora acepta un 'hostName' para mostrar en el lobby.
    public async Task StartStream(string hostName)
    {
        var hostConnectionId = Context.ConnectionId;
        ActiveHosts[hostConnectionId] = hostName;

        await Groups.AddToGroupAsync(hostConnectionId, hostConnectionId);
        await Clients.Caller.SendAsync("StreamStarted", hostConnectionId);

        // NUEVO: Notificar a todos los clientes en el lobby sobre la nueva sala.
        await Clients.All.SendAsync("UpdateHostList", ActiveHosts);
    }

    // Un participante se une a la transmisión del Host.
    public async Task WatchStream(string hostConnectionId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, hostConnectionId);
        await Clients.Client(hostConnectionId).SendAsync("ViewerJoined", Context.ConnectionId);
    }

    // NUEVO: Un método para que los nuevos clientes obtengan la lista de hosts.
    public async Task GetActiveHosts()
    {
        await Clients.Caller.SendAsync("UpdateHostList", ActiveHosts);
    }

    // El Host envía una oferta a un espectador específico.
    public async Task SendOffer(string toUser, string offerSdp)
    {
        await Clients.Client(toUser).SendAsync("ReceiveOffer", Context.ConnectionId, offerSdp);
    }

    // El espectador envía una respuesta al Host.
    public async Task SendAnswer(string toUser, string answerSdp)
    {
        await Clients.Client(toUser).SendAsync("ReceiveAnswer", Context.ConnectionId, answerSdp);
    }

    // Se envían candidatos ICE.
    public async Task SendIceCandidate(string toUser, object candidate)
    {
        await Clients.Client(toUser).SendAsync("ReceiveIceCandidate", Context.ConnectionId, candidate);
    }

    // Maneja la desconexión
    public override async Task OnDisconnectedAsync(System.Exception? exception)
    {
        var connectionId = Context.ConnectionId;

        // NUEVO: Si el que se desconecta era un host, lo eliminamos de la lista
        // y notificamos al lobby.
        if (ActiveHosts.TryRemove(connectionId, out _))
        {
            // Notificar a los espectadores de esa sala que el host se ha ido.
            await Clients.Group(connectionId).SendAsync("HostDisconnected");
            // Actualizar la lista para todos en el lobby.
            await Clients.All.SendAsync("UpdateHostList", ActiveHosts);
        }

        await base.OnDisconnectedAsync(exception);
    }
}