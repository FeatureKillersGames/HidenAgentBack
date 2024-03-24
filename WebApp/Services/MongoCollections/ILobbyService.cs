using MongoDB.Driver;
using WebApp.Models;
using WebApp.Utils;

namespace WebApp.Services.MongoCollections;

public interface ILobbyService
{
    Task<MongoTrackableEntity<Lobby>?> GetTrackableLobbyById(string id);
    Task<MongoTrackableEntity<Lobby>?> GetTrackableLobbyByLobbyId(int lobbyId);
    Task<MongoTrackableEntity<Lobby>?> GetTrackableLobbyByPlayerId(string playerId);
    Task<MongoTrackableEntity<Lobby>?> CreateLobby();
}

public class LobbyService(
    MongoDbService mongoDbService,
    IPlayerService playerService
) : ILobbyService
{
    public Task<MongoTrackableEntity<Lobby>?> GetTrackableLobbyById(string id) => 
        mongoDbService.Lobbies.FindTrackableEntity(l => l.Id == id);

    public Task<MongoTrackableEntity<Lobby>?> GetTrackableLobbyByLobbyId(int lobbyId) =>
        mongoDbService.Lobbies.FindTrackableEntity(l => l.LobbyId == lobbyId);

    public async Task<MongoTrackableEntity<Lobby>?> GetTrackableLobbyByPlayerId(string playerId)
    {
        var player = await playerService.GetTrackablePlayerByPlayerId(playerId);
        if (player.Value?.CurrentLobbyId == null) return null;
        return await GetTrackableLobbyById(player.Value.CurrentLobbyId);
    }

    public async Task<MongoTrackableEntity<Lobby>?> CreateLobby()
    {
        var lobbyWithMaxLobbyId = await mongoDbService.Lobbies
            .Find(_ => true)
            .SortByDescending(lobby => lobby.LobbyId)
            .FirstOrDefaultAsync();
        var lobby = new Lobby(){LobbyId = lobbyWithMaxLobbyId?.LobbyId + 1 ?? 0};
        await mongoDbService.Lobbies.InsertOneAsync(lobby);
        return await mongoDbService.Lobbies.FindTrackableEntity(l => l.Id == lobby.Id);
    }
}