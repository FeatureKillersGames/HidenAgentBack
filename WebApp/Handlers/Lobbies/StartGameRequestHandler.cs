using MediatR;
using MongoDB.Driver;
using WebApp.Models;
using WebApp.Services;
using WebApp.Utils;

namespace WebApp.Handlers.Lobbies;

public class StartGameRequestHandler(
    MongoDbService mongoDbService,
    IConnectionsService connectionsService
    ): IRequestHandler<StartGameRequest, Unit>
{
    public async Task<Unit> Handle(StartGameRequest request, CancellationToken cancellationToken)
    {
        
        var player = await mongoDbService.Players
            .Find(p => p.PlayerId == request.PlayerId)
            .SingleAsync(cancellationToken);

        // Get all lobby players
        var lobby = await mongoDbService.Lobbies.Find(l => l.Id == player.CurrentLobbyId).SingleOrDefaultAsync();
        if (lobby == null)
        {
            await connectionsService.SendMessage(request.PlayerId, new ErrorResponse(ErrorResponseString.NotInLobby));
            return Unit.Value;
        }
        var players = new List<MongoTrackableEntity<Player>>();
        foreach (var lobbyPlayerId in lobby.Players.Select(p => p.PlayerId))
        {
            players.Add(await mongoDbService.Players.FindTrackableEntity(p => p.PlayerId == lobbyPlayerId));
        }
        
        var game = new Game()
        {
            PlayersInGame = players.Select(p => new PlayerInGame()
            {
                Nickname = p.Value!.Nickname,
                PlayerId = p.Value.PlayerId,
                Score = 0
            }).ToList(),
            CurrentPlayerIdx = 0
        };
        await mongoDbService.Games.InsertOneAsync(game);
        var card = mongoDbService.Cards
            .Find(c => true)
            .SortBy(c => Random.Shared.Next())
            .SingleAsync();
        
        
        
        
        foreach (var p in players)
        {
            p.Value!.CurrentGameId = game.Id!;
            await connectionsService.SendMessage(p.Value.PlayerId, new GameStartedResponse());
            await p.DisposeAsync();
        }
        return Unit.Value;
    }
}