using WebApp.Handlers;
using WebApp.Services.MongoCollections;

namespace WebApp.Services;

public interface IRequestInitializer
{
    void InitializeRequest(Request request, string playerId);
}

public class RequestInitializer(
    IPlayerService playerService,
    ILobbyService lobbyService,
    IGameService gameService
) : IRequestInitializer
{
    public void InitializeRequest(Request request, string playerId)
    {
        request.PlayerId = playerId;
        request.Setup(
            playerService.GetTrackablePlayerByPlayerId!,
            lobbyService.GetTrackableLobbyByPlayerId,
            gameService.GetGameByPlayerId
            );
    }
}