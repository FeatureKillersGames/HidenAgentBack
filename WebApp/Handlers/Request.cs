using System.Text.Json.Serialization;
using MediatR;

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
    public string PlayerId { get; set; } = default!;
}

public class ExitLobbyRequest : Request, IRequest<Unit>;
public class CreateLobbyRequest: Request, IRequest<Unit>;
public class ReadyStateRequest: Request, IRequest<Unit>
{
    [JsonPropertyName("is_ready")] public bool IsReady { get; set; }
}

public class JoinLobbyRequest : Request, IRequest<Unit>
{
    [JsonPropertyName("lobby_id")] public string LobbyId { get; set; } = default!;
}

public class StartGameRequest : Request, IRequest<Unit>;
public class CaughtRequest: Request, IRequest<Unit>;

public class GotItRequest : Request, IRequest<Unit>
{
    [JsonPropertyName("player_idx")] public int PlayerIdx { get; set; }
}