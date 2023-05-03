
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
    private EventingBasicConsumer? _consumer = null!;

    #endregion

    #region Properties

    private string? _publisherName;
    public string? PublisherName { get { return _publisherName; } }

    private string? _consumerInfo;
    public string? ConsumerInfo { get { return _consumerInfo; } }

    public List<string> QueuesConsume { get; } = new List<string>();
    
    public bool Enabled { get; set; }
    
    public string? StatusInfo { get; set; }

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

    public void PublishDeclare(string publisher)
    {
        var publishValues = publisher.Split(';');
        _publisherName = publishValues[0];
        _channel?.ExchangeDeclare(_publisherName, publishValues[1], true, false, null);
    }

    public void BasicPublish(string body = "", string routingKey = "")
    {
        _channel?.BasicPublish(_publisherName, routingKey, null, Encoding.UTF8.GetBytes(body));
    }

    public void QueuesBind(List<string> queuesInfo)
    {
        foreach (var queueInfo in queuesInfo)
        {
            var queueInfoValues = queueInfo.Split(';');
            QueuesConsume.Add(queueInfoValues[0]);

            _channel?.QueueDeclare(queueInfoValues[0], true, false, false, null);
            _channel?.ExchangeDeclare(queueInfoValues[1], "fanout", true, false, null);
            _channel?.QueueBind(queueInfoValues[0], queueInfoValues[1], queueInfoValues[2], null);
        }     
    }

    public event EventHandler<MessageBusEventArgs<string>>? ConsumerReceived;
    private void Consumer_Received(object? sender, BasicDeliverEventArgs e)
    {
        string message = Encoding.UTF8.GetString(e.Body.Span);
        // TODO: Capture aditional info in sender ... 
        ConsumerReceived?.Invoke(this, new MessageBusEventArgs<string>(message));
    }

    public void BasicConsume(string queueName)
    {
        _consumerInfo = _channel.BasicConsume(queueName, true, _consumer);
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
