using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RankCalculator;
using ShardLibrary;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory()
{
    HostName = "localhost"
};

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.ExchangeDeclare(
    exchange: "events_exchange",
    type: ExchangeType.Fanout );

channel.QueueDeclare(
    queue: "rank_queue",
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null );

var shardManager = RedisConnectionsFactory.Create();

var consumer = new EventingBasicConsumer( channel );

consumer.Received += ( model, ea ) =>
{
    var body = ea.Body.ToArray();
    var json = Encoding.UTF8.GetString( body );

    var message = JsonSerializer.Deserialize<Message>( json );

    string id = message.Id;

    var db = shardManager.GetShardById( id );

    string text = db.StringGet( "TEXT-" + id );

    double rank = CalculateRank( text );

    db.StringSet( "RANK-" + id, rank );

    var rankEvent = new RankCalculatedEvent
    {
        Id = id,
        Rank = rank
    };

    var eventJson = JsonSerializer.Serialize( rankEvent );
    var eventBody = Encoding.UTF8.GetBytes( eventJson );

    channel.BasicPublish(
        exchange: "events_exchange",
        routingKey: "",
        basicProperties: null,
        body: eventBody );

    Console.WriteLine( $"Processed {id}" );
};

channel.BasicConsume(
    queue: "rank_queue",
    autoAck: true,
    consumer: consumer );

double CalculateRank( string text )
{
    if ( string.IsNullOrEmpty( text ) )
        return 0;

    int nonAlphabetic = text.Count( c =>
        !( char.IsLetter( c ) &&
          ( IsLatin( c ) || IsCyrillic( c ) ) ) );

    return ( double )nonAlphabetic / text.Length;
}

bool IsLatin( char c ) => ( c >= 'a' && c <= 'z' ) || ( c >= 'A' && c <= 'Z' );

bool IsCyrillic( char c ) =>
    ( c >= 'а' && c <= 'я' ) ||
    ( c >= 'А' && c <= 'Я' ) ||
    c == 'ё' || c == 'Ё';

Console.WriteLine( "RankCalculator started" );
Console.ReadLine();
