using System;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices {
  public class MessageBusClient : IMessageBusClient {
    private readonly IConfiguration _configuration;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public MessageBusClient (IConfiguration configuration) {
      _configuration = configuration;

      //the factory used to create a new instance to establish a connection with rabbitmq message bus
      var factory = new ConnectionFactory () {
        HostName = _configuration["RabbitMQHost"],
        Port = int.Parse (_configuration["RabbitMQPort"])
      };
      try {

        //we create the connection
        _connection = factory.CreateConnection ();
        //we create the channel
        _channel = _connection.CreateModel ();
        //we set the type of exchange, there are 4 types, but we keep it simple by using fanout, it broadcast message
        //to all the subscribers without specifying a route
        //so exhange type is used to define how we are seinding the messages, in which way (broadcast,unicast,multicast..)
        _channel.ExchangeDeclare (exchange: "trigger", type : ExchangeType.Fanout);

        //we subscribe to this event, and we also add to it the function we are writing, so we can do more
        //stuff when shutting down the cnx
        _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

        Console.WriteLine ($"--> Connected to the Message Bus");

      } catch (Exception ex) {
        Console.WriteLine ($"--> Could not connect to the Message Bus: {ex.Message}");
      }
    }

    public void PublishNewPlatform (PlatformPublishedDto platformPublishedDto) {
      var message = JsonSerializer.Serialize (platformPublishedDto);
      if (_connection.IsOpen) {
        Console.WriteLine ("--> RabbitMQ Connection is open, sending message ...");
        SendMessage (message);
      } else {
        Console.WriteLine ("--> RabbitMQ Connection is closed, not sending message ...");
      }
    }

    private void SendMessage (string message) {
      var body = Encoding.UTF8.GetBytes (message);

      _channel.BasicPublish (exchange: "trigger",
        routingKey: "",
        basicProperties : null,
        body : body);

      Console.WriteLine ($"--> We Have Sent {message}");
    }
    //this func is when the class dies, we clean everything behind
    public void Dispose () {
      Console.WriteLine ("--> Message Bus Disposed");
      if(_channel.IsOpen){
        _channel.Close();
        _connection.Close();
      }
    }
    private void RabbitMQ_ConnectionShutdown (object sender, ShutdownEventArgs e) {
      Console.WriteLine ($"--> RabbitMQ Connection Shutdown");
    }
  }
}