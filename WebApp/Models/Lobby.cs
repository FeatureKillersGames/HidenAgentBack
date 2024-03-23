using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApp.Models;

public class Lobby
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public int LobbyId { get; set; }
    public List<LobbyPlayer> Players { get; set; } = [];
}

public class LobbyPlayer
{
    public string PlayerId { get; set; } = default!;
    public string Nickname { get; set; } = default!;
    public bool IsReady { get; set; }
}
