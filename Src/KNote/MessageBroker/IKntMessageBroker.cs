using KNote.Model.Dto;

namespace KNote.MessageBroker;

public interface IKntMessageBroker 
{
    void CreateConnection(string hostName, string virtualHost, int port, string userName, string password);
    void ExchangeDeclare(string exchange, string type, IDictionary<string, object>? arguments = null);
    void QueueDeclare(string queue, IDictionary<string, object>? arguments = null);
    void QueueBind(string queue, string exchange, string routingKey, IDictionary<string, object>? arguments = null);
    void BasicPublish(string exchange, string routingKey, string body = "");
    void BasicConsume(string queueName);

    //event EventHandler<MessageBusEventArgs<NoteInfoDto>> ConsumerReceived;
    event EventHandler<MessageBusEventArgs<string>> ConsumerReceived;
}
