namespace KNote.MessageBroker;

public interface IKntMessageBroker
{
    string? PublisherName { get; }
    List<string> QueuesConsume { get; }
    string? ConsumerInfo { get; }
    bool Enabled { get; set; }
    string? StatusInfo { get; set; }

    Task CreateConnectionAsync(string hostName, string virtualHost, int port, string userName, string password);
    Task PublishDeclareAsync(string publisher);
    Task BasicPublishAsync(string body, string routingKey);
    Task QueuesBindAsync(List<string> queuesInfo);
    Task BasicConsumeAsync(string queueName);
    Task CloseConnectionAsync();

    //event EventHandler<MessageBusEventArgs<NoteInfoDto>> ConsumerReceived;
    event EventHandler<MessageBusEventArgs<string>> ConsumerReceived;
}
