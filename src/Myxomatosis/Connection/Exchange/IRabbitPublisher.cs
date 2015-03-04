using System.Collections.Generic;

namespace Myxomatosis.Connection.Exchange
{
    public interface IRabbitPublisher
    {
        void Publish(byte[] payload, string queueName);

        void Publish(byte[] payload, string queueName, IDictionary<string, byte[]> headers);
    }
}