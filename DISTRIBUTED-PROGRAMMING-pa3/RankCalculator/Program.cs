using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RankCalculator;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;

var factory = new ConnectionFactory() { HostName = "localhost" };

using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(
    queue: "rank_queue",
    durable: false,
    exclusive: false,
    autoDelete: false,
    arguments: null );

var redis = ConnectionMultiplexer.Connect( "localhost" );
var db = redis.GetDatabase();

var consumer = new EventingBasicConsumer( channel );

consumer.Received += ( model, ea ) =>
{
    var body = ea.Body.ToArray();
    var json = Encoding.UTF8.GetString( body );

    var message = JsonSerializer.Deserialize<Message>( json );

    string id = message.Id;

    string textKey = "TEXT-" + id;
    string rankKey = "RANK-" + id;

    string text = db.StringGet( textKey );

    double rank = CalculateRank( text );

    db.StringSet( rankKey, rank );

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
