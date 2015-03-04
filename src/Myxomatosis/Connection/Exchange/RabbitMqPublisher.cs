using System.Collections.Generic;
using System.Linq;
using RabbitMQ.Client;

namespace Myxomatosis.Connection.Exchange
{
    internal class RabbitMqPublisher : IRabbitPublisher
    {
        private readonly ConnectionFactory _connectionFactory;

        #region Constructors

        public RabbitMqPublisher(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        #endregion Constructors

        public void Publish(byte[] payload, string exchange)
        {
            PrivatePublish(payload, exchange);
        }

        public void Publish(byte[] payload, string exchange, IDictionary<string, byte[]> headers)
        {
            PrivatePublish(payload, exchange, headers);
        }

        private void PrivatePublish(byte[] payload, string exchange, IDictionary<string, byte[]> headers = null)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    model.DeclareExchange(exchange, "fanout");

                    var basicProperties = model.CreateBasicProperties();
                    basicProperties.Headers = (headers ?? new Dictionary<string, byte[]>()).ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
                    model.BasicPublish(exchange: exchange, routingKey: "", basicProperties: basicProperties, body: payload);
                }
            }
        }
    }
}