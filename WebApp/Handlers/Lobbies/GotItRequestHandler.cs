using MediatR;
using WebApp.Services;
using WebApp.Services.MongoCollections;

namespace WebApp.Handlers.Lobbies;

public class GotItRequestHandler(
    IGameHelperService gameHelperService
): IRequestHandler<GotItRequest, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(GotItRequest request, CancellationToken cancellationToken)
    {
        var game = await request.GetGame();
        if (game == null) return new ErrorResponse(ErrorResponseString.NotInGame);

        var playerSenderIdx = game.Value.PlayersInGame.FindIndex(p => p.PlayerId == request.PlayerId);
        if (playerSenderIdx != game.Value.CurrentPlayerIdx) return new ErrorResponse(ErrorResponseString.WrongPlayer);

        game.Value.PlayersInGame.First(p => p.PlayerId == request.TargetPlayerId).Score += 1;

        await gameHelperService.PerformGameUpdate(game);
        return null;
    }
}