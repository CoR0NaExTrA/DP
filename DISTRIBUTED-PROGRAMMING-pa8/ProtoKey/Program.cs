using ProtoKey.Services;

var builder = WebApplication.CreateBuilder( args );

builder.Services.AddControllers();

builder.Services.AddSingleton<StorageService>();

builder.Services.AddSingleton<PersistenceService>();

builder.Services.AddHostedService<StorageWorker>();

builder.Services.AddHostedService<PersistenceWorker>();

var app = builder.Build();

var storage =
    app.Services.GetRequiredService<StorageService>();

if ( File.Exists( "ProtoKey.data" ) )
{
    var lines =
        await File.ReadAllLinesAsync(
            "ProtoKey.data" );

    foreach ( var line in lines )
    {
        var parts = line.Split( ' ' );

        if ( parts.Length == 3 &&
            parts[ 0 ] == "set" )
        {
            var key = parts[ 1 ];

            var value = int.Parse( parts[ 2 ] );

            storage.Data[ key ] = value;
        }
    }
}

app.MapControllers();

app.Run( "http://localhost:7777" );
