using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RabbitMQ.Client;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;

namespace Valuator.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IDatabase _redis;

    public IndexModel( ILogger<IndexModel> logger, IConnectionMultiplexer redis )
    {
        _logger = logger;
        _redis = redis.GetDatabase();
    }

    public IActionResult OnPost( string text )
    {
        _logger.LogDebug( text );

        string id = Guid.NewGuid().ToString();

        string textKey = "TEXT-" + id;
        string similarityKey = "SIMILARITY-" + id;

        _redis.StringSet( textKey, text );

        const string allTextsKey = "ALL_TEXTS";

        bool exists = _redis.SetContains( allTextsKey, text );
        double similarity = exists ? 1.0 : 0.0;

        _redis.StringSet( similarityKey, similarity );

        if ( !exists )
            _redis.SetAdd( allTextsKey, text );

        SendMessage( id );

        PublishSimilarityEvent( id, similarity );

        return Redirect( $"summary?id={id}" );
    }

    private void SendMessage( string id )
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost"
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: "rank_queue",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null );

        var message = JsonSerializer.Serialize( new { Id = id } );
        var body = Encoding.UTF8.GetBytes( message );

        channel.BasicPublish(
            exchange: "",
            routingKey: "rank_queue",
            basicProperties: null,
            body: body );
    }

    private void PublishSimilarityEvent( string id, double similarity )
    {
        var factory = new ConnectionFactory()
        {
            HostName = "localhost"
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(
            exchange: "events_exchange",
            type: ExchangeType.Fanout );

        var simEvent = new SimilarityCalculatedEvent
        {
            Id = id,
            Similarity = similarity
        };

        var json = JsonSerializer.Serialize( simEvent );
        var body = Encoding.UTF8.GetBytes( json );

        channel.BasicPublish(
            exchange: "events_exchange",
            routingKey: "",
            basicProperties: null,
            body: body );
    }
}