using ProtoKey.Models;

namespace ProtoKey.Services;

public class StorageWorker : BackgroundService
{
    private readonly StorageService _storage;

    public StorageWorker( StorageService storage )
    {
        _storage = storage;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken )
    {
        await foreach ( var command in
            _storage.Channel.Reader.ReadAllAsync( stoppingToken ) )
        {
            switch ( command.Type )
            {
                case "set":
                    {
                        _storage.Data[ command.Key ] =
                            command.Value;

                        command.Completion.SetResult( 1 );

                        break;
                    }

                case "get":
                    {
                        if ( _storage.Data.TryGetValue(
                            command.Key,
                            out var value ) )
                        {
                            command.Completion.SetResult( value );
                        }
                        else
                        {
                            command.Completion.SetResult( 0 );
                        }

                        break;
                    }

                case "keys":
                    {
                        var result = _storage.Data.Keys
                            .Where( k => k.StartsWith( command.Key ) )
                            .ToList();

                        command.KeysCompletion
                            .SetResult( result );

                        break;
                    }
            }
        }
    }
}