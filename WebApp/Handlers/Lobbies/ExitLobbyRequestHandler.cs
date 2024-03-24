using MediatR;
using WebApp.Services;
using WebApp.Utils;

namespace WebApp.Handlers.Lobbies;


public class ExitLobbyRequestHandler(
    IConnectionsService connectionsService
    ) : IRequestHandler<ExitLobbyRequest, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(ExitLobbyRequest request, CancellationToken cancellationToken)
    {
        var player = await request.GetPlayer();
        var lobby = await request.GetLobby();
        
        if (lobby == null)
        {
            return new ErrorResponse(ErrorResponseString.NotInLobby);
        }

        lobby.Value.Players = [.. lobby.Value.Players.Where(lobbyPlayer => lobbyPlayer.PlayerId != request.PlayerId)];
        player.Value.CurrentLobbyId = null;
        
        await connectionsService.SendMessage(request.PlayerId, new RemoveFromLobbyResponse());
        
        if (lobby.Value.Players.Count == 0)
        {
            await lobby.DeleteEntity();
            return null;
        }
        
        var playerInfos = lobby.Value.Players.Select(lobbyPlayer => new PlayerInLobbyInfo()
            { Nickname = lobbyPlayer.Nickname, IsReady = lobbyPlayer.IsReady }).ToList();
        var canStartRoom = lobby.Value.Players.All(p => p.IsReady) && lobby.Value.Players.Count > 2;

        await Task.WhenAll(
            lobby.Value.Players.Select(
                (p, i) => connectionsService.SendMessage(
                    p.PlayerId, 
                    new LobbyUpdatedResponse()
                    {
                        PlayerInfos = playerInfos, CanStartRoom = canStartRoom && i == 0
                    }
                )
            )
        );

        await Task.WhenAll(player.SaveEntity(), lobby.SaveEntity());
        return null;
    }
}