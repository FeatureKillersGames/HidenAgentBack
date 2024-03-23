using System.Linq.Expressions;
using MongoDB.Driver;

namespace WebApp.Utils;

public class MongoTrackableEntity<T> : IAsyncDisposable
{
    public T? Value => _value;
    private bool _saveAble = true;
    private readonly IMongoCollection<T> _mongoCollection;
    private readonly T _value;
    private readonly Expression<Func<T,bool>> _filter;

    public MongoTrackableEntity(IMongoCollection<T> mongoCollection,
        T value,
        Expression<Func<T,bool>> filter)
    {
        _mongoCollection = mongoCollection;
        _value = value;
        _filter = filter;
    }

    public async ValueTask DisposeAsync()
    {
        if (_saveAble && _value != null)
        {
            await _mongoCollection.ReplaceOneAsync(_filter, _value);
        }
    }

    public async Task DeleteEntity()
    {
        await _mongoCollection.DeleteOneAsync(_filter);
        _saveAble = false;
    }
}

public static class MongoTrackableEntityExtensions
{
    public static async Task<MongoTrackableEntity<T>> FindTrackableEntity<T>(
        this IMongoCollection<T> collection,
        Expression<Func<T, bool>> filter)
    {
        var value = await collection.Find(filter).FirstOrDefaultAsync();
        return new MongoTrackableEntity<T>(collection, value, filter);
    }
}