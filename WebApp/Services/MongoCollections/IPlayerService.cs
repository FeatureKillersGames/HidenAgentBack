using Microsoft.Extensions.Caching.Distributed;
using MongoDB.Driver;
using WebApp.Models;
using WebApp.Utils;

namespace WebApp.Services.MongoCollections;

public interface IPlayerService
{
    Task<Player> GetPlayerByPlayerId(string playerId);
    Task<MongoTrackableEntity<Player>?> GetTrackablePlayerByPlayerId(string playerId);
    Task<Player> CreatePlayer(string playerId, string nickname);
    Task<Player> DeletePlayerByPlayerId(string playerId);
}

public class PlayerService(
    MongoDbService mongoDbService,
    IDistributedCache cache
    ) : IPlayerService
{

    public async Task<Player> GetPlayerByPlayerId(string playerId)
    {
        var id = await cache.GetStringAsync(DistributedCacheKeys.PlayerIdPrefix(playerId));
        return await mongoDbService.Players.Find(p => p.Id == id).SingleAsync();
    }

    public async Task<MongoTrackableEntity<Player>?> GetTrackablePlayerByPlayerId(string playerId)
    {
        var id = await cache.GetStringAsync(DistributedCacheKeys.PlayerIdPrefix(playerId));
        return await mongoDbService.Players.FindTrackableEntity(p => p.Id == id);
    }

    public async Task<Player> CreatePlayer(string playerId, string nickname)
    {
        var player = new Player()
        {
            PlayerId = playerId,
            Nickname = nickname
        };
        await mongoDbService.Players.InsertOneAsync(player);
        await cache.SetStringAsync(DistributedCacheKeys.PlayerIdPrefix(playerId), player.Id!);
        return player;
    }

    public async Task<Player> DeletePlayerByPlayerId(string playerId)
    {
        var id = await cache.GetStringAsync(DistributedCacheKeys.PlayerIdPrefix(playerId));
        var player = await mongoDbService.Players.Find(p => p.Id == id).SingleAsync();
        await mongoDbService.Players.DeleteOneAsync(p => p.Id == id);
        return player;
    }
}