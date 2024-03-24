using WebApp.Utils;
using TimerCallback = WebApp.Models.TimerCallback;

namespace WebApp.Services.MongoCollections;

public interface ITimerCallbackService
{
    Task<MongoTrackableEntity<TimerCallback>?> GetTrackableTimerCallbackById(string id);
}

public class TimerCallbackService(
    MongoDbService mongoDbService
) : ITimerCallbackService
{
    public Task<MongoTrackableEntity<TimerCallback>?> GetTrackableTimerCallbackById(string id) => 
        mongoDbService.TimerCallbacks.FindTrackableEntity(t => t.Id == id);
}