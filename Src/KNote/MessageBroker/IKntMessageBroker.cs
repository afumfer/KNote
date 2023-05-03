namespace KNote.MessageBroker;

public interface IKntMessageBroker 
{
    string? PublisherName { get; }
    List<string> QueuesConsume { get; }
    string? ConsumerInfo { get; }
    bool Enabled { get; set; }
    string? StatusInfo { get; set; }

    void CreateConnection(string hostName, string virtualHost, int port, string userName, string password);
    void PublishDeclare(string publisher);
    void BasicPublish(string body, string routingKey);
    void QueuesBind(List<string> queuesInfo);
    void BasicConsume(string queueName);

    //event EventHandler<MessageBusEventArgs<NoteInfoDto>> ConsumerReceived;
    event EventHandler<MessageBusEventArgs<string>> ConsumerReceived;
}
