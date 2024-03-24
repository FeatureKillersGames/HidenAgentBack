using System.Diagnostics;
using MediatR;
using MongoDB.Driver;
using Sqids;
using WebApp.Models;
using WebApp.Services;
using WebApp.Services.MongoCollections;
using WebApp.Utils;

namespace WebApp.Handlers.Lobbies;

public class CreateLobbyRequestHandler(
    IConnectionsService connectionsService,
    ILobbyService lobbyService,
    SqidsEncoder<int> sqidsEncoder): IRequestHandler<CreateLobbyRequest, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(CreateLobbyRequest request, CancellationToken cancellationToken)
    {
        var player = await request.GetPlayer();
        var lobby = await lobbyService.CreateLobby();
        if (lobby == null) throw new UnreachableException("created lobby is null");
        
        lobby.Value.Players.Add(new LobbyPlayer(){PlayerId = player.Value.PlayerId, Nickname = player.Value.Nickname});
        player.Value.CurrentLobbyId = lobby.Value.Id!;
        
        await connectionsService.SendMessage(request.PlayerId, 
            new MoveToLobbyResponse()
            {
                LobbyId = sqidsEncoder.Encode(lobby.Value.LobbyId),
                Players =
                [
                    .. lobby.Value.Players.Select(lobbyPlayer => new PlayerInLobbyInfo()
                        { Nickname = lobbyPlayer.Nickname, IsReady = lobbyPlayer.IsReady })
                ]
            });

        await Task.WhenAll(player.SaveEntity(), lobby.SaveEntity());
        
        return null;
    }
}