using StackExchange.Redis;

namespace ShardLibrary;

public static class RedisConnectionsFactory
{
    public static RedisShardManager Create()
    {
        var main = ConnectionMultiplexer.Connect(
            Environment.GetEnvironmentVariable( "DB_MAIN" )
            ?? "localhost:6000" );

        var ru = ConnectionMultiplexer.Connect(
            Environment.GetEnvironmentVariable( "DB_RU" )
            ?? "localhost:6001" );

        var eu = ConnectionMultiplexer.Connect(
            Environment.GetEnvironmentVariable( "DB_EU" )
            ?? "localhost:6002" );

        var asia = ConnectionMultiplexer.Connect(
            Environment.GetEnvironmentVariable( "DB_ASIA" )
            ?? "localhost:6003" );

        return new RedisShardManager( main, ru, eu, asia );
    }
}