using StackExchange.Redis;

namespace Valuator;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();

        var mainConn = Environment.GetEnvironmentVariable( "DB_MAIN" ) ?? "localhost:6000";
        var ruConn = Environment.GetEnvironmentVariable( "DB_RU" ) ?? "localhost:6001";
        var euConn = Environment.GetEnvironmentVariable( "DB_EU" ) ?? "localhost:6002";
        var asiaConn = Environment.GetEnvironmentVariable( "DB_ASIA" ) ?? "localhost:6003";

        var main = ConnectionMultiplexer.Connect( mainConn );
        var ru = ConnectionMultiplexer.Connect( ruConn );
        var eu = ConnectionMultiplexer.Connect( euConn );
        var asia = ConnectionMultiplexer.Connect( asiaConn );

        builder.Services.AddSingleton( new RedisShardManager( main, ru, eu, asia ) );

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }
}
