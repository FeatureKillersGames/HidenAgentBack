using System.Net.WebSockets;

namespace WebApp.Services;

public interface IPlayerIdReceiver
{
    Task<string> ReceivePlayerId(WebSocket webSocket);
}

public class RandomPlayerIdReceiver : IPlayerIdReceiver
{
    public Task<string> ReceivePlayerId(WebSocket webSocket)
    {
        return Task.FromResult(Guid.NewGuid().ToString());
    }
}