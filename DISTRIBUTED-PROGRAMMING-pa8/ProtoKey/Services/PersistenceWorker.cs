using ProtoKey.Models;

namespace ProtoKey.Services;

public class PersistenceWorker : BackgroundService
{
    private readonly PersistenceService _persistence;

    private readonly List<StorageCommand> _buffer
        = new();

    public PersistenceWorker(
        PersistenceService persistence )
    {
        _persistence = persistence;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken )
    {
        _ = Task.Run( async () =>
        {
            while ( !stoppingToken.IsCancellationRequested )
            {
                await Task.Delay( 1000 );

                await Flush();
            }
        } );

        await foreach ( var command in
            _persistence.Channel.Reader
                .ReadAllAsync( stoppingToken ) )
        {
            _buffer.Add( command );
        }
    }

    private async Task Flush()
    {
        if ( _buffer.Count == 0 )
        {
            return;
        }

        var lines = _buffer.Select( c =>
            $"set {c.Key} {c.Value}" );

        await File.AppendAllLinesAsync(
            "ProtoKey.data",
            lines );

        _buffer.Clear();
    }
}