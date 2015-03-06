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

        public IRabbitQueue GetQueue(string exchange, string queueName, string routingKey)
        {
            return OpenConnectionInternal(exchange, queueName, routingKey);
        }

        public IRabbitQueue<T> GetQueue<T>(string exchange, string queueName)
        {
            return new QueueResult<T>(OpenConnectionInternal(exchange, queueName, null), _logger);
        }

        public IRabbitQueue<T> GetQueue<T>(string exchange, string queueName, string routingKey)
        {
            return new QueueResult<T>(OpenConnectionInternal(exchange, queueName, routingKey), _logger);
        }

        public IRabbitExchange GetExchange(string exchange)
        {
            return new Exchange(exchange, _publisher, _serializer);
        }

        public IRabbitExchange<T> GetExchange<T>(string exchange)
        {
            return new Exchange<T>(exchange, _publisher, _serializer);
        }

        #endregion IObservableConnection Members

        private QueueResult OpenConnectionInternal(string exchange, string queueName, string routingKey)
        {
            return new QueueResult(exchange, queueName, routingKey, _subscriptionManager, _subscriberThread);
        }
    }
}