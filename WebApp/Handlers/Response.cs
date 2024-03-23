using System.Text.Json.Serialization;

namespace WebApp.Handlers;


[JsonDerivedType(typeof(MoveToLobbyResponse), "move_to_lobby")]
[JsonDerivedType(typeof(ErrorResponse), "error")]
[JsonDerivedType(typeof(LobbyUpdatedResponse), "lobby_update")]
[JsonDerivedType(typeof(RemoveFromLobbyResponse), "remove_from_lobby")]
[JsonDerivedType(typeof(WrongLobbyIdResponse), "wrong_lobby_id")]
[JsonDerivedType(typeof(GameStartedResponse), "game_started")]
[JsonDerivedType(typeof(ShowCardDecryptedResponse), "show_card_decrypted")]
[JsonDerivedType(typeof(ShowCardEncryptedResponse), "show_card_encrypted")]
[JsonDerivedType(typeof(ShowRolesResponse), "show_roles")]
[JsonDerivedType(typeof(UpdateLeaderboardResponse), "update_leaderboard")]
[JsonDerivedType(typeof(GameFinishedResponse), "game_finished")]
public abstract class Response;


public class PlayerInLobbyInfo
{
    [JsonPropertyName("nickname")] public string Nickname { get; set; } = default!;
    [JsonPropertyName("is_ready")] public bool IsReady { get; set; }
}

public class PlayerInLeaderboardInfo
{
    [JsonPropertyName("nickname")] public string Nickname { get; set; } = default!;
    [JsonPropertyName("score")] public int Score { get; set; }
}

public class ErrorResponse(string message) : Response
{
    [JsonPropertyName("message")] public string Message { get; set; } = message;
}
public class LobbyUpdatedResponse: Response
{
    [JsonPropertyName("players")] public List<PlayerInLobbyInfo>? PlayerInfos { get; set; }
    [JsonPropertyName("can_start_room")] public bool CanStartRoom { get; set; }
}
public class RemoveFromLobbyResponse : Response;
public class MoveToLobbyResponse: Response
{
    [JsonPropertyName("players")] public List<PlayerInLobbyInfo> Players { get; set; } = [];
    [JsonPropertyName("lobby_id")] public string LobbyId { get; set; } = default!;
}
public class WrongLobbyIdResponse : Response;

public class GameStartedResponse : Response
{
    [JsonPropertyName("players")] public List<string> Players { get; set; } = [];
}

public class ShowCardEncryptedResponse : Response
{
    [JsonPropertyName("target")] public string Target { get; set; } = default!;
    [JsonPropertyName("banned")] public List<string> Banned { get; set; } = [];
}

public class ShowCardDecryptedResponse : Response
{
    [JsonPropertyName("target")] public string Target { get; set; } = default!;
    [JsonPropertyName("players")] public List<string> Players { get; set; } = [];
}

public class ShowRolesResponse : Response
{
    [JsonPropertyName("speaker")] public string Speaker { get; set; } = default!;
    [JsonPropertyName("agent")] public string Agent { get; set; } = default!;
}

public class UpdateLeaderboardResponse : Response
{
    [JsonPropertyName("leaderboard")] public List<PlayerInLeaderboardInfo> PlayerInLeaderboard { get; set; } = [];
}

public class GameFinishedResponse : Response
{
    [JsonPropertyName("leaderboard")] public List<PlayerInLeaderboardInfo> PlayerInLeaderboard { get; set; } = [];
}