using System.Text.Json.Serialization;
using MediatR;
using WebApp.Models;
using WebApp.Utils;

namespace WebApp.Handlers;

[JsonDerivedType(typeof(CreateLobbyRequest), "create_lobby")]
[JsonDerivedType(typeof(ReadyStateRequest), "ready_state")]
[JsonDerivedType(typeof(ExitLobbyRequest), "exit_lobby")]
[JsonDerivedType(typeof(JoinLobbyRequest), "join_lobby")]
[JsonDerivedType(typeof(StartGameRequest), "start_game")]
[JsonDerivedType(typeof(CaughtRequest), "caught")]
[JsonDerivedType(typeof(GotItRequest), "got_it")]
public abstract class Request
{
    [JsonIgnore] public string PlayerId { get; set; } = default!;
    
    [JsonIgnore] private Func<string, Task<MongoTrackableEntity<Player>>> _playerTrackableGetFunc = default!;
    [JsonIgnore] private Func<string, Task<MongoTrackableEntity<Lobby>?>> _lobbyTrackableGetFunc = default!;
    [JsonIgnore] private Func<string, Task<MongoTrackableEntity<Game>?>> _gameTrackableGetFunc = default!;

    [JsonIgnore] private MongoTrackableEntity<Player>? _player;
    [JsonIgnore] private MongoTrackableEntity<Lobby>? _lobby;
    [JsonIgnore] private MongoTrackableEntity<Game>? _game;

    public async Task<MongoTrackableEntity<Player>> GetPlayer() => _player ??= await _playerTrackableGetFunc.Invoke(PlayerId);
    public async Task<MongoTrackableEntity<Lobby>?> GetLobby() => _lobby ??= await _lobbyTrackableGetFunc.Invoke(PlayerId);
    public async Task<MongoTrackableEntity<Game>?> GetGame() => _game ??= await _gameTrackableGetFunc.Invoke(PlayerId);

    public void Setup(
        Func<string, Task<MongoTrackableEntity<Player>>> playerTrackableGetFunc, 
        Func<string, Task<MongoTrackableEntity<Lobby>?>> lobbyTrackableGetFunc, 
        Func<string, Task<MongoTrackableEntity<Game>?>> gameTrackableGetFunc)
    {
        _playerTrackableGetFunc = playerTrackableGetFunc;
        _lobbyTrackableGetFunc = lobbyTrackableGetFunc;
        _gameTrackableGetFunc = gameTrackableGetFunc;
    }
}

public class ExitLobbyRequest : Request, IRequest<ErrorResponse?>;
public class CreateLobbyRequest: Request, IRequest<ErrorResponse?>;
public class ReadyStateRequest: Request, IRequest<ErrorResponse?>
{
    [JsonPropertyName("is_ready")] public bool IsReady { get; set; }
}

public class JoinLobbyRequest : Request, IRequest<ErrorResponse?>
{
    [JsonPropertyName("lobby_id")] public string LobbyId { get; set; } = default!;
}

public class StartGameRequest : Request, IRequest<ErrorResponse?>;
public class CaughtRequest: Request, IRequest<ErrorResponse?>;

public class GotItRequest : Request, IRequest<ErrorResponse?>
{
    [JsonPropertyName("player_id")] public string TargetPlayerId { get; set; } = default!;
}