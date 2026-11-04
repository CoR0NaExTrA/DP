using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory() { HostName = "localhost" };

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.ExchangeDeclare(
    exchange: "events_exchange",
    type: ExchangeType.Fanout );

var queueName = channel.QueueDeclare().QueueName;

channel.QueueBind(
    queue: queueName,
    exchange: "events_exchange",
    routingKey: "" );

var consumer = new EventingBasicConsumer( channel );

consumer.Received += ( model, ea ) =>
{
    var json = Encoding.UTF8.GetString( ea.Body.ToArray() );

    using var doc = JsonDocument.Parse( json );
    var root = doc.RootElement;

    Console.WriteLine( "=== EVENT ===" );

    if ( root.TryGetProperty( "Rank", out var rankProp ) )
    {
        Console.WriteLine( "Type: RankCalculated" );
        Console.WriteLine( $"Id: {root.GetProperty( "Id" ).GetString()}" );
        Console.WriteLine( $"Rank: {rankProp.GetDouble()}" );
    }
    else if ( root.TryGetProperty( "Similarity", out var simProp ) )
    {
        Console.WriteLine( "Type: SimilarityCalculated" );
        Console.WriteLine( $"Id: {root.GetProperty( "Id" ).GetString()}" );
        Console.WriteLine( $"Similarity: {simProp.GetDouble()}" );
    }

    Console.WriteLine();
};

channel.BasicConsume( queueName, true, consumer );

Console.WriteLine( "EventsLogger started..." );
Console.ReadLine();
