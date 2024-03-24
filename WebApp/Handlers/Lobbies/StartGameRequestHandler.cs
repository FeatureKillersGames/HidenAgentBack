using System.Diagnostics;
using MediatR;
using WebApp.Models;
using WebApp.Services;
using WebApp.Services.MongoCollections;

namespace WebApp.Handlers.Lobbies;

public class StartGameRequestHandler(
    IPlayerService playerService,
    IGameService gameService,
    IGameHelperService gameHelperService,
    ILogger<StartGameRequestHandler> logger): IRequestHandler<StartGameRequest, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(StartGameRequest request, CancellationToken cancellationToken)
    {
        var lobby = await request.GetLobby();
        if (lobby == null) return new ErrorResponse(ErrorResponseString.NotInLobby);
        
        var game = await gameService.CreateGame();
        if (game == null) throw new UnreachableException("created game is empty");

        game.Value.PlayersInGame =
        [
            ..lobby.Value.Players.Select(p => new PlayerInGame()
            {
                Nickname = p.Nickname,
                PlayerId = p.PlayerId,
                Score = 0
            })
        ];
        await Task.WhenAll(game.Value.PlayersInGame.Select(async p =>
        {
            var player = await playerService.GetTrackablePlayerByPlayerId(p.PlayerId);
            if (player == null)
            {
                logger.LogWarning("Can not find player with id {Id}, but he is in lobby", p.PlayerId);
                return;
            }

            player.Value.CurrentLobbyId = null;
            player.Value.CurrentGameId = game.Value.Id;

            await player.SaveEntity();
        }));
        await Task.WhenAll(game.SaveEntity(), lobby.DeleteEntity());

        await gameHelperService.PerformGameUpdate(game, sendStartGameResponse: true, swapPlayers: false);
        
        return null;
    }
}