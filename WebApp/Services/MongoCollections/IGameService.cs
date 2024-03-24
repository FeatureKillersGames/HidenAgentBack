using WebApp.Models;
using WebApp.Utils;

namespace WebApp.Services.MongoCollections;

public interface IGameService
{
    Task<MongoTrackableEntity<Game>?> GetTrackableGameById(string id);
    Task<MongoTrackableEntity<Game>?> GetGameByPlayerId(string playerId);
    Task<MongoTrackableEntity<Game>?> CreateGame();
}

public class GameService(
    MongoDbService mongoDbService,
    IPlayerService playerService
) : IGameService
{
    public Task<MongoTrackableEntity<Game>?> GetTrackableGameById(string id) =>
        mongoDbService.Games.FindTrackableEntity(g => g.Id == id);

    public async Task<MongoTrackableEntity<Game>?> GetGameByPlayerId(string playerId)
    {
        var player = await playerService.GetTrackablePlayerByPlayerId(playerId);
        if (player?.Value.CurrentGameId == null) return null;
        return await GetTrackableGameById(player.Value!.CurrentGameId);
    }

    public async Task<MongoTrackableEntity<Game>?> CreateGame()
    {
        var game = new Game();
        await mongoDbService.Games.InsertOneAsync(game);
        return await mongoDbService.Games.FindTrackableEntity(g => g.Id == game.Id);
    }
}