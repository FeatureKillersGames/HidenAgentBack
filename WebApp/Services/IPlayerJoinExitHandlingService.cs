using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediatR;
using MongoDB.Driver;
using WebApp.Handlers;
using WebApp.Models;

namespace WebApp.Services;

public interface IPlayerJoinExitHandlingService
{
    Task HandleJoin(WebSocket webSocket, string playerId);
    Task HandleExit(string playerId);
}

public class PlayerJoinExitHandlingService(
    MongoDbService mongoDbService,
    IMediator mediator
    ): IPlayerJoinExitHandlingService
{
    public async Task HandleJoin(WebSocket webSocket, string playerId)
    {
        var buffer = new byte[1024];
        var received = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
        var request = JsonSerializer.Deserialize<JoinRequest>(Encoding.UTF8.GetString(buffer, 0, received.Count))!;
        var player = new Player()
        {
            PlayerId = playerId,
            Nickname = request.Nickname
        };
        await mongoDbService.Players.InsertOneAsync(player);
    }

    public async Task HandleExit(string playerId)
    {
        await mediator.Send(new ExitLobbyRequest() { PlayerId = playerId });
        await mongoDbService.Players
            .DeleteOneAsync(new ExpressionFilterDefinition<Player>(player => player.PlayerId == playerId));
    }

    private class JoinRequest
    {
        [JsonIgnore]
        public string PlayerId { get; set; } = default!;
        [JsonPropertyName("nickname")]
        public string Nickname { get; set; } = default!;
    }
}

