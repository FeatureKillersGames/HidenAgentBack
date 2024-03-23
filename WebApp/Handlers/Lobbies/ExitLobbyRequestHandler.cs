using MediatR;
using WebApp.Services;
using WebApp.Utils;

namespace WebApp.Handlers.Lobbies;


public class ExitLobbyRequestHandler(
    MongoDbService mongoDbService,
    IConnectionsService connectionsService
    ) : IRequestHandler<ExitLobbyRequest, Unit>
{
    public async Task<Unit> Handle(ExitLobbyRequest request, CancellationToken cancellationToken)
    {
        await using var player = 
            await mongoDbService.Players.FindTrackableEntity(player => player.PlayerId == request.PlayerId);
        if (string.IsNullOrEmpty(player.Value!.CurrentLobbyId))
        {
            await connectionsService.SendMessage(request.PlayerId, new ErrorResponse(ErrorResponseString.NotInLobby));
            return Unit.Value;
        }
        
        await using var lobby = await mongoDbService.Lobbies
            .FindTrackableEntity(lobby => lobby.Id == player.Value.CurrentLobbyId);
        if (lobby.Value == null)
        {
            await connectionsService.SendMessage(request.PlayerId, new ErrorResponse(ErrorResponseString.NotInLobby));
            return Unit.Value;
        }

        lobby.Value.Players = [.. lobby.Value.Players.Where(lobbyPlayer => lobbyPlayer.PlayerId != request.PlayerId)];
        await connectionsService.SendMessage(request.PlayerId, new RemoveFromLobbyResponse());
        
        if (lobby.Value.Players.Count == 0)
        {
            await lobby.DeleteEntity();
            return Unit.Value;
        }
        
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