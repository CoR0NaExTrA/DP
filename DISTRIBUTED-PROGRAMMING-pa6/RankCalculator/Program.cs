using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RankCalculator;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory() { HostName = "localhost" };

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

var mainConn = Environment.GetEnvironmentVariable( "DB_MAIN" ) ?? "localhost:6000";
var ruConn = Environment.GetEnvironmentVariable( "DB_RU" ) ?? "localhost:6001";
var euConn = Environment.GetEnvironmentVariable( "DB_EU" ) ?? "localhost:6002";
var asiaConn = Environment.GetEnvironmentVariable( "DB_ASIA" ) ?? "localhost:6003";

var main = ConnectionMultiplexer.Connect( mainConn );
var ru = ConnectionMultiplexer.Connect( ruConn );
var eu = ConnectionMultiplexer.Connect( euConn );
var asia = ConnectionMultiplexer.Connect( asiaConn );

var mainDb = main.GetDatabase();

IDatabase GetShard( string region ) => region switch
{
    "RU" => ru.GetDatabase(),
    "EU" => eu.GetDatabase(),
    "ASIA" => asia.GetDatabase(),
    _ => throw new Exception( "Unknown region" )
};

var consumer = new EventingBasicConsumer( channel );

consumer.Received += ( model, ea ) =>
{
    var body = ea.Body.ToArray();
    var json = Encoding.UTF8.GetString( body );

    var message = JsonSerializer.Deserialize<Message>( json );

    string id = message.Id;

    string textKey = "TEXT-" + id;
    string rankKey = "RANK-" + id;

    string region = mainDb.StringGet( "MAP-" + id );

    Console.WriteLine( $"LOOKUP: {id}, {region}" );

    var db = GetShard( region );

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

bool IsLatin( char c ) =>
    ( c >= 'a' && c <= 'z' ) || ( c >= 'A' && c <= 'Z' );

bool IsCyrillic( char c ) =>
    ( c >= 'а' && c <= 'я' ) ||
    ( c >= 'А' && c <= 'Я' ) ||
    c == 'ё' || c == 'Ё';

Console.WriteLine( "RankCalculator started" );
Console.ReadLine();
