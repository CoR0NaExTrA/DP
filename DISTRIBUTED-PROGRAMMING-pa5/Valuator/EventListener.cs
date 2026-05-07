using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

public class EventListener : BackgroundService
{
    private readonly IHubContext<SummaryHub> _hubContext;

    public EventListener( IHubContext<SummaryHub> hubContext )
    {
        _hubContext = hubContext;
    }

    protected override Task ExecuteAsync( CancellationToken stoppingToken )
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.ExchangeDeclare( "events_exchange", ExchangeType.Fanout );

        var queueName = channel.QueueDeclare().QueueName;

        channel.QueueBind( queueName, "events_exchange", "" );

        var consumer = new EventingBasicConsumer( channel );

        consumer.Received += async ( model, ea ) =>
        {
            var json = Encoding.UTF8.GetString( ea.Body.ToArray() );

            Console.WriteLine( "EVENT RECEIVED IN VALUATOR: " + json ); // 👈

            using var doc = JsonDocument.Parse( json );
            var root = doc.RootElement;

            string id = root.GetProperty( "Id" ).GetString();

            await _hubContext.Clients.Group( id )
                .SendAsync( "ReceiveUpdate", json );
        };

        channel.BasicConsume( queueName, true, consumer );

        return Task.CompletedTask;
    }
}