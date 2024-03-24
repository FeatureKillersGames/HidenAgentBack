namespace WebApp;

public static class DistributedCacheKeys
{
    public static string PlayerIdPrefix(string playerId) => $"player-id-{playerId}";
}