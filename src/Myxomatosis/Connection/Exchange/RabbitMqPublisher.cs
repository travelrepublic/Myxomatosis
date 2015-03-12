using Myxomatosis.Connection.Queue;
using Myxomatosis.Serialization;
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
                    basicProperties.Headers = (headers ?? new Dictionary<string, byte[]>()).ToDictionary(kvp => kvp.Key, kvp => (object) kvp.Value);
                    model.BasicPublish(exchange, routingKey, basicProperties, payload);
                }
            }
        }
    }

    internal class Exchange : IExchange
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly string _exchangeName;

        #region Constructors

        public Exchange(ConnectionFactory connectionFactory, string exchangeName)
        {
            _connectionFactory = connectionFactory;
            _exchangeName = exchangeName;
            Serializer = DefaultSerializer.Instance;
        }

        #endregion Constructors

        public ISerializer Serializer { get; set; }

        #region IExchange Members

        public void Publish(byte[] message)
        {
            PublishBytes(message);
        }

        public void Publish(byte[] message, string routingKey)
        {
            PublishBytes(message, routingKey);
        }

        public void Publish(byte[] message, IDictionary<string, byte[]> headers)
        {
            PublishBytes(message, headers: headers);
        }

        public void Publish(byte[] message, IDictionary<string, byte[]> headers, string routingKey)
        {
            PublishBytes(message, routingKey, headers);
        }

        public void Publish<T>(T message)
        {
            PublishMessage(message);
        }

        public void Publish<T>(T message, string routingKey)
        {
            PublishMessage(message, routingKey);
        }

        public void Publish<T>(T message, IDictionary<string, object> headers)
        {
            PublishMessage(message, headers: headers);
        }

        public void Publish<T>(T message, IDictionary<string, object> headers, string routingKey)
        {
            PublishMessage(message, routingKey, headers);
        }

        #endregion IExchange Members

        private void PublishMessage<T>(T message, string routingKey = "", IDictionary<string, object> headers = null)
        {
            PublishBytes(Serializer.Serialize(message), routingKey, headers == null ? new Dictionary<string, byte[]>() : headers.ToDictionary(kvp => kvp.Key, kvp => Serializer.Serialize(kvp.Value)));
        }

        private void PublishBytes(byte[] message, string routingKey = "", IDictionary<string, byte[]> headers = null)
        {
            using (var connection = _connectionFactory.CreateConnection())
            {
                using (var model = connection.CreateModel())
                {
                    var basicProperties = model.CreateBasicProperties();
                    basicProperties.Headers = (headers ?? new Dictionary<string, byte[]>()).ToDictionary(kvp => kvp.Key, kvp => (object) kvp.Value);
                    model.BasicPublish(_exchangeName, routingKey, basicProperties, message);
                }
            }
        }
    }
}