using MongoDB.Driver;
using WebApp.Models;
using WebApp.Utils;

namespace WebApp.Services.MongoCollections;

public interface IPlayerService
{
    Task<Player> GetPlayerById(string playerId);
    Task<MongoTrackableEntity<Player>> GetTrackablePlayerById(string playerId);
}

public class PlayerService(
    MongoDbService mongoDbService
    ) : IPlayerService
{

    public async Task<Player> GetPlayerById(string playerId)
    {
        return await mongoDbService.Players.Find(p => p.PlayerId == playerId).SingleAsync();
    }

    public Task<MongoTrackableEntity<Player>> GetTrackablePlayerById(string playerId)
    {
        return await mongoDbService.Players.FindTrackableEntity(p => p.PlayerId == playerId);
    }
}