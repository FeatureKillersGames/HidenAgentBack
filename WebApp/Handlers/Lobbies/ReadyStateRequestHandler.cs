using MediatR;
using MongoDB.Driver;
using WebApp.Services;
using WebApp.Utils;

namespace WebApp.Handlers.Lobbies;

public class ReadyStateRequestHandler (
    MongoDbService mongoDbService,
    IConnectionsService connectionsService
    ): Request, IRequestHandler<ReadyStateRequest, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(ReadyStateRequest request, CancellationToken cancellationToken)
    {
        var lobby = await request.GetLobby();
        if (lobby == null)
        {
            return new ErrorResponse(ErrorResponseString.NotInLobby);
        }
        
        lobby.Value.Players.Single(lobbyPlayer => lobbyPlayer.PlayerId == request.PlayerId).IsReady = request.IsReady;
        var lobbyPlayers = lobby.Value.Players
            .Select(lobbyPlayer => new PlayerInLobbyInfo()
                {
                    Nickname = lobbyPlayer.Nickname, IsReady = lobbyPlayer.IsReady
                })
            .ToList();
        var canStartRoom = lobby.Value.Players.All(p => p.IsReady) && lobby.Value.Players.Count > 2;

        await Task.WhenAll(
            Task.WhenAll(lobby.Value.Players.Select((p, i) => connectionsService.SendMessage(p.PlayerId,
                new LobbyUpdatedResponse()
                {
                    CanStartRoom = canStartRoom && i == 0,
                    PlayerInfos = lobbyPlayers
                }))),
            lobby.SaveEntity()
        );

        return null;
    }
}