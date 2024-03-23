using MediatR;
using MongoDB.Driver;
using WebApp.Services;
using WebApp.Utils;

namespace WebApp.Handlers.Lobbies;

public class ReadyStateRequestHandler (
    MongoDbService mongoDbService,
    IConnectionsService connectionsService
    ): Request, IRequestHandler<ReadyStateRequest, Unit>
{
    public async Task<Unit> Handle(ReadyStateRequest request, CancellationToken cancellationToken)
    {
        var player = await mongoDbService.Players
            .Find(player => player.PlayerId == request.PlayerId)
            .SingleAsync(cancellationToken);
        if (string.IsNullOrEmpty(player.CurrentLobbyId))
        {
            await connectionsService.SendMessage(request.PlayerId, new ErrorResponse(ErrorResponseString.NotInLobby));
            return Unit.Value;
        }
        
        await using var lobby = await mongoDbService.Lobbies
            .FindTrackableEntity(lobby => lobby.Id == player.CurrentLobbyId);
        if (lobby.Value == null)
        {
            await connectionsService.SendMessage(request.PlayerId, new ErrorResponse(ErrorResponseString.NotInLobby));
            return Unit.Value;
        }

        lobby.Value.Players.Single(lobbyPlayer => lobbyPlayer.PlayerId == request.PlayerId).IsReady = request.IsReady;
        var lobbyUpdatedResponse = new LobbyUpdatedResponse()
        {
            PlayerInfos =
            [
                .. lobby.Value.Players.Select(lobbyPlayer => new PlayerInLobbyInfo()
                    { Nickname = lobbyPlayer.Nickname, IsReady = lobbyPlayer.IsReady })
            ],
            CanStartRoom = lobby.Value.Players.All(p => p.IsReady) && lobby.Value.Players.Count > 2
        };
        foreach (var playerInLobby in lobby.Value.Players.Select(lobbyPlayer => lobbyPlayer.PlayerId))
        {
            await connectionsService.SendMessage(playerInLobby, lobbyUpdatedResponse);
            lobbyUpdatedResponse.CanStartRoom = false;
        }
        return Unit.Value;
    }
}