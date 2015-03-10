using System.Collections.Generic;

namespace Myxomatosis.Connection.Exchange
{
    public interface IRabbitPublisher
    {
        void Publish(byte[] payload, string exchange);

        void Publish(byte[] payload, string exchange, string routingKey);

        void Publish(byte[] payload, string exchange, IDictionary<string, byte[]> headers);

        void Publish(byte[] payload, string exchange, string routingKey, IDictionary<string, byte[]> dictionary);

        void Publish(byte[] payload, string exchange, string routingKey, IDictionary<string, byte[]> dictionary, ExchangeType exchangeType);
    }
}