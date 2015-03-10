using Myxomatosis.Connection;
using Myxomatosis.Connection.Exchange;
using Myxomatosis.Connection.Queue;
using Myxomatosis.Connection.Queue.Listen;
using Myxomatosis.Logging;
using Myxomatosis.Serialization;

namespace Myxomatosis.Api
{
    public class Listener : IObservableConnection
    {
        private readonly IRabbitMqClientLogger _logger;
        private readonly IRabbitPublisher _publisher;
        private readonly ISerializer _serializer;
        private readonly IRabbitMqSubscriber _subscriberThread;
        private readonly IQueueSubscriptionManager _subscriptionManager;

        #region Constructors

        public Listener(
            IRabbitMqSubscriber subscriberThread,
            IRabbitPublisher publisher,
            IQueueSubscriptionManager subscriptionManager,
            ISerializer serializer,
            IRabbitMqClientLogger logger)
        {
            _subscriberThread = subscriberThread;
            _publisher = publisher;
            _subscriptionManager = subscriptionManager;
            _serializer = serializer;
            _logger = logger;
        }

        #endregion Constructors

        #region IObservableConnection Members

        public IRabbitQueue GetQueue(string exchange, string queueName)
        {
            return OpenConnectionInternal(exchange, queueName, null);
        }

        public IRabbitQueue GetQueue(string exchange, string queueName, ExchangeType exchangeType)
        {
            return OpenConnectionInternal(exchange, queueName, null, exchangeType);
        }

        public IRabbitQueue GetQueue(string exchange, string queueName, string routingKey)
        {
            return OpenConnectionInternal(exchange, queueName, routingKey);
        }

        public IRabbitQueue GetQueue(string exchange, string queueName, string routingKey, ExchangeType exchangeType)
        {
            return OpenConnectionInternal(exchange, queueName, routingKey, exchangeType);
        }

        public IRabbitQueue<T> GetQueue<T>(string exchange, string queueName)
        {
            return new QueueResult<T>(OpenConnectionInternal(exchange, queueName, null), _logger);
        }

        public IRabbitQueue<T> GetQueue<T>(string exchange, string queueName, ExchangeType exchangeType)
        {
            return new QueueResult<T>(OpenConnectionInternal(exchange, queueName, null, exchangeType), _logger);
        }

        public IRabbitQueue<T> GetQueue<T>(string exchange, string queueName, string routingKey)
        {
            return new QueueResult<T>(OpenConnectionInternal(exchange, queueName, routingKey), _logger);
        }

        public IRabbitQueue<T> GetQueue<T>(string exchange, string queueName, string routingKey, ExchangeType exchangeType)
        {
            return new QueueResult<T>(OpenConnectionInternal(exchange, queueName, routingKey, exchangeType), _logger);
        }

        public IRabbitExchange GetExchange(string exchange)
        {
            return GetExchange(exchange, ExchangeType.Topic);
        }

        public IRabbitExchange<T> GetExchange<T>(string exchange)
        {
            return GetExchange<T>(exchange, ExchangeType.Topic);
        }

        public IRabbitExchange GetExchange(string exchange, ExchangeType exchangeType)
        {
            return new Exchange(exchange, _publisher, _serializer, exchangeType);
        }

        public IRabbitExchange<T> GetExchange<T>(string exchange, ExchangeType exchangeType)
        {
            return new Exchange<T>(exchange, _publisher, _serializer, exchangeType);
        }

        #endregion IObservableConnection Members

        private QueueResult OpenConnectionInternal(string exchange, string queueName, string routingKey, ExchangeType exchangeType = ExchangeType.Topic)
        {
            return new QueueResult(exchange, queueName, routingKey, _subscriptionManager, _subscriberThread, exchangeType);
        }
    }
}