//because this class will be working in the background , it will be listenning to the event bus, so thats 
//why we don't create an interface for it so it is a background service

using System.Text;
using CommandsService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommandsService.AsyncDataServices {
  public class MessageBusSubscriber : BackgroundService {
    private readonly IConfiguration _configuration;
    private readonly IEventProcessor _eventProcessor;
    private IConnection _connection;
    private IModel _channel;
    private string _queueName;

    public MessageBusSubscriber (IConfiguration configuration, IEventProcessor eventProcessor) {
      _configuration = configuration;
      _eventProcessor = eventProcessor;
      InitializeRabbitMQ ();
    }

    private void InitializeRabbitMQ () {
      var factory = new ConnectionFactory () {
        HostName = _configuration["RabbitMQHost"],
        Port = int.Parse (_configuration["RabbitMQPort"])
      };

      _connection = factory.CreateConnection ();
      _channel = _connection.CreateModel ();
      _channel.ExchangeDeclare (exchange: "trigger", type : ExchangeType.Fanout);
      _queueName = _channel.QueueDeclare ().QueueName;
      _channel.QueueBind (queue: _queueName,
        exchange: "trigger",
        routingKey: "");

      Console.WriteLine ("--> Listenning on the message bus ..");

      _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

    }

    private void RabbitMQ_ConnectionShutdown (object sender, ShutdownEventArgs e) {
      Console.WriteLine ("--> Connection Shutdown");
    }

    public override void Dispose () {
      if (_channel.IsOpen) {
        _channel.Close ();
        _connection.Close ();
      }

      base.Dispose ();
    }
    protected override Task ExecuteAsync (CancellationToken stoppingToken) {
      stoppingToken.ThrowIfCancellationRequested ();
      //so we subscribe to the channel
      var consumer = new EventingBasicConsumer (_channel);
      //on received of notification we get the string and then we call the process event to process it
      //and later it checks if it is platform published it will insert to db
      consumer.Received += (ModuleHandle, ea) => {
        Console.WriteLine ("--> Event Received");
        var body = ea.Body;
        var notificationMessage = Encoding.UTF8.GetString (body.ToArray ());
        _eventProcessor.ProcessEvent (notificationMessage);
      };

      _channel.BasicConsume(queue:_queueName,autoAck:true,consumer:consumer);
      return Task.CompletedTask;
    }
  }
}