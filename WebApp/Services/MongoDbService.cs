using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApp.Models;
using WebApp.Option;
using TimerCallback = WebApp.Models.TimerCallback;

namespace WebApp.Services;


public class MongoDbService
{
    public IMongoCollection<Lobby> Lobbies { get; }
    public IMongoCollection<Player> Players { get; }
    public IMongoCollection<Game> Games { get; }
    public IMongoCollection<Card> Cards { get; }
    public IMongoCollection<TimerCallback> TimerCallbacks { get; }
    
    public MongoDbService(IOptions<MongoDbOptions> options)
    {
        var mongoClient = new MongoClient(options.Value.ConnectionString);
        var mongoDb = mongoClient.GetDatabase(options.Value.DataBaseName);
        
        Lobbies = mongoDb.GetCollection<Lobby>(options.Value.LobbiesCollectionName);
        Players = mongoDb.GetCollection<Player>(options.Value.PlayersCollectionName);
        Games = mongoDb.GetCollection<Game>(options.Value.GamesCollectionName);
        Cards = mongoDb.GetCollection<Card>(options.Value.CardsCollectionName);
        TimerCallbacks = mongoDb.GetCollection<TimerCallback>(options.Value.TimerCallbacksCollectionName);
    }
}