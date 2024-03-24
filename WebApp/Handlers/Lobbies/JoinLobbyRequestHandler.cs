using MediatR;
using Sqids;
using WebApp.Models;
using WebApp.Services;
using WebApp.Services.MongoCollections;
using WebApp.Utils;

namespace WebApp.Handlers.Lobbies;

public class JoinLobbyRequestHandler(
    ILobbyService lobbyService,
    IConnectionsService connectionsService,
    SqidsEncoder<int> sqidsEncoder
    ) : IRequestHandler<JoinLobbyRequest, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(JoinLobbyRequest request, CancellationToken cancellationToken)
    {
        var player = await request.GetPlayer();
        var lobby = await lobbyService.GetTrackableLobbyByLobbyId(sqidsEncoder.Decode(request.LobbyId).First());
        
        if (lobby == null)
        {
            await connectionsService.SendMessage(request.PlayerId, new WrongLobbyIdResponse());
            return null;
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
        var canStartRoom = playerInLobbyInfos.All(p => p.IsReady) && playerInLobbyInfos.Count > 2;

        await Task.WhenAll(
            connectionsService.SendMessage(request.PlayerId, new MoveToLobbyResponse()
            {
                Players = playerInLobbyInfos,
                LobbyId = request.LobbyId
            }),
            Task.WhenAll(
                playersToNotify.Select((p, i) => 
                    connectionsService.SendMessage(p, new LobbyUpdatedResponse()
                    {
                        CanStartRoom = canStartRoom && i == 0,
                        PlayerInfos = playerInLobbyInfos
                    })
                )
            )
        );
        
        await Task.WhenAll(player.SaveEntity(), lobby.SaveEntity());
        return null;
    }
}