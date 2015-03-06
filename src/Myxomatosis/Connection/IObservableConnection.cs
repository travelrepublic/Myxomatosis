using Myxomatosis.Connection.Queue;

namespace Myxomatosis.Connection
{
    public interface IObservableConnection
    {
        IRabbitQueue GetQueue(string exchange, string queueName);

        IRabbitQueue GetQueue(string exchange, string queueName, string routingKey);

        IRabbitQueue<T> GetQueue<T>(string exchange, string queueName);

        IRabbitQueue<T> GetQueue<T>(string exchange, string queueName, string routingKey);

        IRabbitExchange GetExchange(string exchange);

        IRabbitExchange<T> GetExchange<T>(string exchange);
    }
}