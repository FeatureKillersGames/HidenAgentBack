using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApp.Models;

public class Game
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public List<PlayerInGame> PlayersInGame { get; set; } = [];
    public int CurrentPlayerIdx { get; set; }
    public string? TimerId { get; set; }

}

public class PlayerInGame
{
    public string PlayerId { get; set; } = default!;
    public string Nickname { get; set; } = default!;
    public int Score { get; set; }
}