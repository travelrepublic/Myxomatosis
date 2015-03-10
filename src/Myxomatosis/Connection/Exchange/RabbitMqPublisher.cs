using RabbitMQ.Client;
using System.Collections.Generic;
using System.Linq;

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

        #region IRabbitPublisher Members

        public void Publish(byte[] payload, string exchange)
        {
            PrivatePublish(payload, exchange);
        }

        public void Publish(byte[] payload, string exchange, string routingKey)
        {
            PrivatePublish(payload, exchange, routingKey);
        }

        public void Publish(byte[] payload, string exchange, IDictionary<string, byte[]> headers)
        {
            PrivatePublish(payload, exchange, "", headers);
        }

        public void Publish(byte[] payload, string exchange, string routingKey, IDictionary<string, byte[]> headers)
        {
            PrivatePublish(payload, exchange, routingKey, headers);
        }

        public void Publish(byte[] payload, string exchange, string routingKey, IDictionary<string, byte[]> headers, ExchangeType exchangeType)
        {
            PrivatePublish(payload, exchange, routingKey, headers, exchangeType);
        }

        #endregion IRabbitPublisher Members

        private void PrivatePublish(byte[] payload, string exchange, string routingKey = "", IDictionary<string, byte[]> headers = null, ExchangeType exchangeType = ExchangeType.Topic)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    model.DeclareExchange(exchange, exchangeType.ToRabbitExchange());

                    var basicProperties = model.CreateBasicProperties();
                    basicProperties.Headers = (headers ?? new Dictionary<string, byte[]>()).ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
                    model.BasicPublish(exchange, routingKey, basicProperties, payload);
                }
            }
        }
    }
}