using System.Reflection;
using Sqids;
using WebApp.Option;
using WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbOptions>(builder.Configuration.GetSection(nameof(MongoDbOptions)));


builder.Services.AddMediatR(o =>
{
    o.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
});

builder.Services.AddSingleton<IConnectionsService, ConnectionsService>();
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddSingleton<SqidsEncoder<int>>(_ => new SqidsEncoder<int>(new SqidsOptions() { MinLength = 5 }));
builder.Services.AddScoped<IPlayerJoinExitHandlingService, PlayerJoinExitHandlingService>();
builder.Services.AddScoped<IPlayerIdReceiver, RandomPlayerIdReceiver>();
builder.Services.AddDistributedMemoryCache();

var app = builder.Build();
app.UseWebSockets();

app.MapGet("/ws", async (HttpContext context, IConnectionsService connectionsService) =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await connectionsService.MainLoop(webSocket);
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status200OK;
    }
});

app.Run();