﻿namespace WebApp.Option;

public class MongoDbOptions
{
    public string ConnectionString { get; set; } = default!;
    public string DataBaseName { get; set; } = default!;
    
    public string LobbiesCollectionName { get; set; } = default!;
    public string PlayersCollectionName { get; set; } = default!;
    public string GamesCollectionName { get; set; } = default!;
    public string CardsCollectionName { get; set; } = default!;
    public string TimerCallbacksCollectionName { get; set; } = default!;
}