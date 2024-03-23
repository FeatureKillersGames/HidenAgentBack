using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using MediatR;
using WebApp.Handlers;

namespace WebApp.Services;

public interface IConnectionsService
{
    Task MainLoop(WebSocket webSocket);
    Task SendMessage(string playerId, Response message);
}

public class ConnectionsService(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<ConnectionsService> logger) : IConnectionsService
{
    private const int BufferSize = 4096;
    private readonly Dictionary<string, WebSocket> _connectedPlayers = new();
    public async Task MainLoop(WebSocket webSocket)
    {
        string playerId;
        using (var scope = serviceScopeFactory.CreateScope())
        {
            var playerIdReceiver = scope.ServiceProvider.GetRequiredService<IPlayerIdReceiver>();
            playerId = await playerIdReceiver.ReceivePlayerId(webSocket);
        
            _connectedPlayers.Add(playerId, webSocket);
        
            var playerJoinExitHandlingService =
                scope.ServiceProvider.GetRequiredService<IPlayerJoinExitHandlingService>();
            await playerJoinExitHandlingService.HandleJoin(webSocket, playerId);
        }

        try
        {
            var buffer = new byte[BufferSize];
            var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!receiveResult.CloseStatus.HasValue)
            {
                using (var scope = serviceScopeFactory.CreateScope())
                {
                    var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                    var request =
                        JsonSerializer.Deserialize<Request>(Encoding.UTF8.GetString(buffer, 0, receiveResult.Count));
                    if (request != null)
                    {
                        request.PlayerId = playerId;
                        await mediator.Send(request);
                    }
                }

                receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
            
            using (var scope = serviceScopeFactory.CreateScope())
            {
                _connectedPlayers.Remove(playerId);
            
                var playerJoinExitHandlingService =
                    scope.ServiceProvider.GetRequiredService<IPlayerJoinExitHandlingService>();
                await playerJoinExitHandlingService.HandleExit(playerId);
            }
        }
        catch (WebSocketException)
        {
            using (var scope = serviceScopeFactory.CreateScope())
            {
                _connectedPlayers.Remove(playerId);
            
                var playerJoinExitHandlingService =
                    scope.ServiceProvider.GetRequiredService<IPlayerJoinExitHandlingService>();
                await playerJoinExitHandlingService.HandleExit(playerId);
            }
        }
    }

    public async Task SendMessage(string playerId, Response message)
    {
        if (_connectedPlayers.TryGetValue(playerId, out var player))
        {
            await player.SendAsync(
                Encoding.UTF8.GetBytes(JsonSerializer.Serialize<Response>(message)),
                    WebSocketMessageType.Text,
                    WebSocketMessageFlags.EndOfMessage,
                    CancellationToken.None);
        }
    }
}