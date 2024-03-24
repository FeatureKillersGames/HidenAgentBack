using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using MongoDB.Driver;
using WebApp.Handlers;
using WebApp.Models;
using WebApp.Services.MongoCollections;

namespace WebApp.Services;

public interface IPlayerJoinExitHandlingService
{
    Task HandleJoin(WebSocket webSocket, string playerId);
    Task HandleExit(string playerId);
}

public class PlayerJoinExitHandlingService(
    IPlayerService playerService,
    IMediator mediator,
    IRequestInitializer requestInitializer
    ): IPlayerJoinExitHandlingService
{
    public async Task HandleJoin(WebSocket webSocket, string playerId)
    {
        var buffer = new byte[1024];
        var received = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
        var request = JsonSerializer.Deserialize<JoinRequest>(Encoding.UTF8.GetString(buffer, 0, received.Count))!;
        await playerService.CreatePlayer(playerId, request.Nickname);
    }

    public async Task HandleExit(string playerId)
    {
        var exitLobbyRequest = new ExitLobbyRequest();
        requestInitializer.InitializeRequest(exitLobbyRequest, playerId);
        await mediator.Send(exitLobbyRequest);
        await playerService.DeletePlayerByPlayerId(playerId);
    }

    private class JoinRequest
    {
        [JsonIgnore]
        public string PlayerId { get; set; } = default!;
        [JsonPropertyName("nickname")]
        public string Nickname { get; set; } = default!;
    }
}

