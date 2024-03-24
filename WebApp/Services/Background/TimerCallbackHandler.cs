namespace WebApp.Services.Background;

public class TimerCallbackHandler(
    IConnectionsService connectionsService,
    MongoDbService mongoDbService
): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            
        }
    }
}