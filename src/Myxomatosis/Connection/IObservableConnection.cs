using Myxomatosis.Connection.Queue;

namespace Myxomatosis.Connection
{
    public interface IObservableConnection
    {
        IRabbitQueue GetQueue(string exchange, string queueName);

        IRabbitQueue<T> GetQueue<T>(string exchange, string queueName);
    }
}