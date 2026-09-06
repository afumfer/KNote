
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace KNote.MessageBroker.RabbitMQ;

public class KntMessageBroker : IKntMessageBroker, IAsyncDisposable, IDisposable
{
    #region Private fields

    private IConnection? _conn;
    private IChannel? _channel;
    private AsyncEventingBasicConsumer? _consumer;

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

    #region IKntMessageBroker implementation

    public async Task CreateConnectionAsync(string hostName, string virtualHost, int port, string userName, string password)
    {
        if((_conn != null && _channel != null) && (_conn.IsOpen && _channel.IsOpen))
        {
            await CloseConnectionAsync();
        }

        ConnectionFactory factory = new ConnectionFactory
        {
            HostName = hostName,
            VirtualHost = virtualHost,
            Port = port,
            UserName = userName,
            Password = password
        };

        _conn = await factory.CreateConnectionAsync();
        _channel = await _conn.CreateChannelAsync();

        _consumer = new AsyncEventingBasicConsumer(_channel);

        _consumer.ReceivedAsync += Consumer_ReceivedAsync;
    }

    public async Task PublishDeclareAsync(string publisher)
    {
        var publishValues = publisher.Split(';');
        _publisherName = publishValues[0];
        if (_channel != null)
            await _channel.ExchangeDeclareAsync(_publisherName, publishValues[1], true, false);
    }

    public async Task BasicPublishAsync(string body = "", string routingKey = "")
    {
        if (!string.IsNullOrEmpty(_publisherName) && _channel != null)
            await _channel.BasicPublishAsync(_publisherName, routingKey, Encoding.UTF8.GetBytes(body));
    }

    public async Task QueuesBindAsync(List<string> queuesInfo)
    {
        foreach (var queueInfo in queuesInfo)
        {
            var queueInfoValues = queueInfo.Split(';');
            QueuesConsume.Add(queueInfoValues[0]);

            if (_channel == null)
                continue;

            await _channel.QueueDeclareAsync(queueInfoValues[0], true, false, false);
            await _channel.ExchangeDeclareAsync(queueInfoValues[1], "fanout", true, false);
            await _channel.QueueBindAsync(queueInfoValues[0], queueInfoValues[1], queueInfoValues[2]);
        }
    }

    public event EventHandler<MessageBusEventArgs<string>>? ConsumerReceived;
    private Task Consumer_ReceivedAsync(object sender, BasicDeliverEventArgs e)
    {
        string message = Encoding.UTF8.GetString(e.Body.Span);
        // TODO: Capture aditional info in sender ...
        ConsumerReceived?.Invoke(this, new MessageBusEventArgs<string>(message));
        return Task.CompletedTask;
    }

    public async Task BasicConsumeAsync(string queueName)
    {
        if (_channel == null || _consumer == null)
            return;
        _consumerInfo = await _channel.BasicConsumeAsync(queueName, true, _consumer);
    }

    public async Task CloseConnectionAsync()
    {
        if(_consumer != null)
            _consumer.ReceivedAsync -= Consumer_ReceivedAsync;
        if (_channel != null)
            await _channel.CloseAsync();
        if (_conn != null)
            await _conn.CloseAsync();
    }

    #endregion

    #region IDisposable / IAsyncDisposable

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null)
            await _channel.CloseAsync();
        if (_conn != null)
            await _conn.CloseAsync();
    }

    #endregion
}
