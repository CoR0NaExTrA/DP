using StackExchange.Redis;

namespace ShardLibrary;

public class RedisShardManager
{
    private readonly IDatabase _main;
    private readonly Dictionary<string, IDatabase> _shards;

    public RedisShardManager(
        IConnectionMultiplexer main,
        IConnectionMultiplexer ru,
        IConnectionMultiplexer eu,
        IConnectionMultiplexer asia )
    {
        _main = main.GetDatabase();

        _shards = new()
        {
            [ "RU" ] = ru.GetDatabase(),
            [ "EU" ] = eu.GetDatabase(),
            [ "ASIA" ] = asia.GetDatabase()
        };
    }

    public void SaveShardKey( string id, string region )
    {
        _main.StringSet( "MAP-" + id, region );
    }

    public string GetShardKey( string id )
    {
        return _main.StringGet( "MAP-" + id );
    }

    public IDatabase GetShard( string region )
    {
        return _shards[ region ];
    }

    public IDatabase GetShardById( string id )
    {
        var region = GetShardKey( id );

        Console.WriteLine( $"LOOKUP: {id}, {region}" );

        return GetShard( region );
    }
}