using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApp.Models;

public class Player
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string PlayerId { get; set; } = default!;
    public string Nickname { get; set; } = default!;
    public string CurrentLobbyId { get; set; } = default!;
    public string CurrentGameId { get; set; } = default!;
}