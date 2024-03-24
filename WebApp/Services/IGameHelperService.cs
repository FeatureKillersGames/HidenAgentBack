using WebApp.Handlers;
using WebApp.Models;
using WebApp.Services.MongoCollections;
using WebApp.Utils;

namespace WebApp.Services;

public interface IGameHelperService
{
    Task PerformGameUpdate(
        MongoTrackableEntity<Game> game,
        bool sendStartGameResponse = false,
        bool swapPlayers = true);
}

public class GameHelperService(
    ICardService cardService,
    IConnectionsService connectionsService
) : IGameHelperService
{
    public async Task PerformGameUpdate(
        MongoTrackableEntity<Game> game, 
        bool sendStartGameResponse = false,
        bool swapPlayers = true)
    {

        if (swapPlayers)
        {
            game.Value.CurrentPlayerIdx += 1;
            game.Value.CurrentPlayerIdx %= game.Value.PlayersInGame.Count;
        }
        
        var gameStartedResponse = new GameStartedResponse()
        {
            Players = [..game.Value.PlayersInGame.Select(p => p.Nickname)]
        };
        var updateLeaderboardResponse = new UpdateLeaderboardResponse()
        {
            PlayerInLeaderboard =
            [
                .. game.Value.PlayersInGame.Select(p => new PlayerInLeaderboardInfo()
                {
                    Nickname = p.Nickname,
                    Score = p.Score
                })
            ]
        };
        
        var card = await cardService.GetRandomCard();
        var playerToExplain = game.Value.PlayersInGame[game.Value.CurrentPlayerIdx];
        var playerToFindBanned = game.Value.PlayersInGame[(game.Value.CurrentPlayerIdx + 1) % game.Value.PlayersInGame.Count];
        
        await Task.WhenAll(game.Value.PlayersInGame.Select(async p =>
        {
            if (sendStartGameResponse)
            {
                await connectionsService.SendMessage(p.PlayerId, gameStartedResponse);
            }
            
            await connectionsService.SendMessage(p.PlayerId, updateLeaderboardResponse);
            
            if (p == playerToExplain)
            {
                await connectionsService.SendMessage(p.PlayerId, new ShowCardDecryptedResponse()
                {
                    Listeners = [..game.Value.PlayersInGame
                        .Where(pig => pig != playerToFindBanned && pig != playerToExplain)
                        .Select(pig => new ListenerInfo()
                        {
                            Nickname = pig.Nickname,
                            PlayerId = pig.PlayerId
                        })
                    ],
                    Target = card.Target
                });
            }
            else if (p == playerToFindBanned)
            {
                await connectionsService.SendMessage(p.PlayerId, new ShowCardEncryptedResponse()
                {
                    Target = card.Target,
                    Banned = card.Banned
                });
            }
            else
            {
                await connectionsService.SendMessage(p.PlayerId, new ShowRolesResponse()
                {
                    Speaker = playerToExplain.Nickname,
                    Agent = playerToFindBanned.Nickname
                });
            }
        }));
        await game.SaveEntity();
    }
}
