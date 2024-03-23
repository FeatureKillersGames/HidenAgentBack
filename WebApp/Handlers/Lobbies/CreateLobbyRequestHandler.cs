using MediatR;
using MongoDB.Driver;
using Sqids;
using WebApp.Models;
using WebApp.Services;
using WebApp.Utils;

namespace WebApp.Handlers.Lobbies;

public class CreateLobbyRequestHandler(
    IConnectionsService connectionsService,
    MongoDbService mongoDbService,
    SqidsEncoder<int> sqidsEncoder): IRequestHandler<CreateLobbyRequest, Unit>
{
    public async Task<Unit> Handle(CreateLobbyRequest request, CancellationToken cancellationToken)
    {
        await using var player = 
            await mongoDbService.Players.FindTrackableEntity(player => player.PlayerId == request.PlayerId);

        var lobbyWithMaxLobbyId = await mongoDbService.Lobbies
            .Find(_ => true)
            .SortByDescending(lobby => lobby.LobbyId)
            .FirstOrDefaultAsync(cancellationToken);
        var lobby = new Lobby(){LobbyId = lobbyWithMaxLobbyId?.LobbyId + 1 ?? 0};
        lobby.Players.Add(new LobbyPlayer(){PlayerId = player.Value!.PlayerId, Nickname = player.Value.Nickname});
        await mongoDbService.Lobbies.InsertOneAsync(lobby,cancellationToken: cancellationToken);
        
        player.Value.CurrentLobbyId = lobby.Id!;
        await connectionsService.SendMessage(request.PlayerId, 
            new MoveToLobbyResponse()
            {
                LobbyId = sqidsEncoder.Encode(lobby.LobbyId),
                Players =
                [
                    .. lobby.Players.Select(lobbyPlayer => new PlayerInLobbyInfo()
                        { Nickname = lobbyPlayer.Nickname, IsReady = lobbyPlayer.IsReady })
                ]
            });
        return Unit.Value;
    }
}