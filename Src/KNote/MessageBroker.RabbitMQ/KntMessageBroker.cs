
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KNote.MessageBroker.RabbitMQ;

public class KntMessageBroker : IKntMessageBroker, IDisposable
{
    #region Private fields 

    private IConnection? _conn;
    private IModel? _channel;
    EventingBasicConsumer? _consumer = null!;
    private string? _consummerTag;

    #endregion

    #region Constructor

    public KntMessageBroker()
    {
        
    }

    public KntMessageBroker(string hostName, string virtualHost, int port, string userName, string password)
    {
        CreateConnection(hostName, virtualHost, port, userName, password);
    }

    #endregion 

    #region IKntMessageBroker implementation

    public void CreateConnection(string hostName, string virtualHost, int port, string userName, string password)
    {
        if((_conn != null && _channel != null) && (_conn.IsOpen && _channel.IsOpen))
        {
            CloseConnection();
        }

        ConnectionFactory factory = new ConnectionFactory
        {
            HostName = hostName,
            VirtualHost = virtualHost,
            Port = port,
            UserName = userName,
            Password = password
        };

        _conn = factory.CreateConnection();
        _channel = _conn.CreateModel();

        _consumer = new EventingBasicConsumer(_channel);

        _consumer.Received += Consumer_Received;
    }

    public event EventHandler<MessageBusEventArgs<string>>? ConsumerReceived;
    private void Consumer_Received(object? sender, BasicDeliverEventArgs e)
    {
        string message = Encoding.UTF8.GetString(e.Body.Span);
        //Debug.WriteLine($"Message: {message}");
        ConsumerReceived?.Invoke(this, new MessageBusEventArgs<string>(message));
    }

    public void BasicConsume(string queueName)
    {
        _consummerTag = _channel.BasicConsume(queueName, true, _consumer);
    }

    public void ExchangeDeclare(string exchange, string type, IDictionary<string, object>? arguments = null)
    {
        _channel?.ExchangeDeclare(exchange, type, true, false, arguments);
    }

    public void QueueDeclare(string queue, IDictionary<string, object>? arguments = null)
    {
        _channel?.QueueDeclare(queue, true, false, false, arguments);
    }

    public void QueueBind(string queue, string exchange, string routingKey, IDictionary<string, object>? arguments = null)
    {
        _channel?.QueueBind(queue, exchange, routingKey, arguments);
    }

    public void BasicPublish(string exchange, string routingKey, string body = "")
    {        
        _channel?.BasicPublish(exchange, routingKey, null, Encoding.UTF8.GetBytes(body));
    }

    public void CloseConnection()
    {
        if(_consumer != null)
            _consumer.Received -= Consumer_Received;
        _channel?.Close();
        _conn?.Close();
    }

    #endregion

    #region IDisposable 

    public void Dispose()
    {
        _channel?.Close();
        _conn?.Close();
    }

    #endregion 
}
