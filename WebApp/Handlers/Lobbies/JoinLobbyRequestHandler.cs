using MediatR;
using Sqids;
using WebApp.Models;
using WebApp.Services;
using WebApp.Utils;

namespace WebApp.Handlers.Lobbies;

public class JoinLobbyRequestHandler(
    MongoDbService mongoDbService,
    IConnectionsService connectionsService,
    SqidsEncoder<int> sqidsEncoder
    ) : IRequestHandler<JoinLobbyRequest, Unit>
{
    public async Task<Unit> Handle(JoinLobbyRequest request, CancellationToken cancellationToken)
    {
        await using var player = 
            await mongoDbService.Players.FindTrackableEntity(player => player.PlayerId == request.PlayerId);
        
        await using var lobby =
            await mongoDbService.Lobbies.FindTrackableEntity(lobby => lobby.LobbyId == sqidsEncoder.Decode(request.LobbyId)[0]);
        if (lobby.Value == null)
        {
            await connectionsService.SendMessage(request.PlayerId, new WrongLobbyIdResponse());
            return Unit.Value;
        }

        var playersToNotify = lobby.Value.Players.Select(p => p.PlayerId).ToList();
        lobby.Value.Players.Add(new LobbyPlayer()
        {
            PlayerId = request.PlayerId,
            Nickname = player.Value!.Nickname,
            IsReady = false
        });
        player.Value.CurrentLobbyId = lobby.Value.Id!;
        var playerInLobbyInfos = lobby.Value.Players.Select(p => new PlayerInLobbyInfo()
        {
            Nickname = p.Nickname,
            IsReady = p.IsReady
        }).ToList();

        await connectionsService.SendMessage(request.PlayerId, new MoveToLobbyResponse()
        {
            Players = playerInLobbyInfos,
            LobbyId = request.LobbyId
        });
        var response = new LobbyUpdatedResponse()
        {
            PlayerInfos = playerInLobbyInfos,
            CanStartRoom = playerInLobbyInfos.All(p => p.IsReady) && playerInLobbyInfos.Count > 2
        };
        foreach (var playerToNotify in playersToNotify)
        {
            await connectionsService.SendMessage(playerToNotify, response);
            response.CanStartRoom = false;
        }
        return Unit.Value;
    }
}