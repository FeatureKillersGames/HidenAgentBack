using MongoDB.Driver;
using WebApp.Models;

namespace WebApp.Services.MongoCollections;

public interface ICardService
{
    Task<Card> GetRandomCard();
}

public class CardService(
    MongoDbService mongoDbService
) : ICardService
{
    public async Task<Card> GetRandomCard()
    {
        return await mongoDbService.Cards
            .Find(c => true)
            .FirstAsync();
    }
}