namespace WebApp;

public static class DistributedCacheKeys
{
    public static string PlayersLobby(string playerId)
    {
        return $"lobby_player_{playerId}";
    }
}