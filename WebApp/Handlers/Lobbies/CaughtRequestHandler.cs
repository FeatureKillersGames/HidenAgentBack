using MediatR;
using WebApp.Services;

namespace WebApp.Handlers.Lobbies;

public class CaughtRequestHandler(
    IGameHelperService gameHelperService
): IRequestHandler<CaughtRequest, ErrorResponse?>
{
    public async Task<ErrorResponse?> Handle(CaughtRequest request, CancellationToken cancellationToken)
    {
        var game = await request.GetGame();
        if (game == null) return new ErrorResponse(ErrorResponseString.NotInGame);

        var playerSenderIdx = game.Value.PlayersInGame.FindIndex(p => p.PlayerId == request.PlayerId);
        if((game.Value.CurrentPlayerIdx + 1) % game.Value.PlayersInGame.Count != playerSenderIdx) return new ErrorResponse(ErrorResponseString.WrongPlayer);

        game.Value.PlayersInGame.First(p => p.PlayerId == request.PlayerId).Score += 1;

        await gameHelperService.PerformGameUpdate(game);
        return null;
    }
}