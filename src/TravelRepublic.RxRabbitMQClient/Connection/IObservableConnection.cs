using TravelRepublic.RxRabbitMQClient.Connection.Queue;

namespace TravelRepublic.RxRabbitMQClient.Connection
{
    public interface IObservableConnection
    {
        IRabbitQueue GetQueue(string exchange, string queueName);

        IRabbitQueue<T> GetQueue<T>(string exchange, string queueName);
    }
}