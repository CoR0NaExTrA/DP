using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace ShardLibrary;

public static class RedisConnectionsFactory
{
    public static RedisShardManager Create( IConfiguration config )
    {
        string password = config[ "Redis:Password" ]!;

        string mainHost = config[ "Redis:Main" ]!;
        string ruHost = config[ "Redis:Ru" ]!;
        string euHost = config[ "Redis:Eu" ]!;
        string asiaHost = config[ "Redis:Asia" ]!;

        var main = ConnectionMultiplexer.Connect(
            $"{mainHost},password={password}" );

        var ru = ConnectionMultiplexer.Connect(
            $"{ruHost},password={password}" );

        var eu = ConnectionMultiplexer.Connect(
            $"{euHost},password={password}" );

        var asia = ConnectionMultiplexer.Connect(
            $"{asiaHost},password={password}" );

        return new RedisShardManager( main, ru, eu, asia );
    }
}