using System.Collections.Generic;

namespace TravelRepublic.RxRabbitMQClient.Connection.Exchange
{
    public interface IRabbitPublisher
    {
        void Publish(byte[] payload, string queueName);

        void Publish(byte[] payload, string queueName, IDictionary<string, byte[]> headers);
    }
}