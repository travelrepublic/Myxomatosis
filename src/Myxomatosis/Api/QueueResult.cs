using Myxomatosis.Connection;
using Myxomatosis.Connection.Exchange;
using Myxomatosis.Connection.Queue;
using Myxomatosis.Connection.Queue.Listen;
using Myxomatosis.Logging;
using Myxomatosis.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Myxomatosis.Api
{
    public class Exchange : IRabbitExchange
    {
        private readonly string _exchange;
        private readonly IRabbitPublisher _publisher;
        protected readonly ISerializer _serializer;

        #region Constructors

        public Exchange(string exchange, IRabbitPublisher publisher, ISerializer serializer)
        {
            _exchange = exchange;
            _publisher = publisher;
            _serializer = serializer;
        }

        #endregion Constructors

        #region IRabbitExchange Members

        public void Publish(byte[] payload, string routingKey)
        {
            PublishInternal(payload, routingKey, null);
        }

        public void Publish(byte[] payload, string routingKey, IDictionary<string, object> headers)
        {
            PublishInternal(payload, routingKey, headers.ToDictionary(kvp => kvp.Key, kvp => _serializer.Serialize(kvp.Value)));
        }

        #endregion IRabbitExchange Members

        private void PublishInternal(byte[] payload, string routingKey, IDictionary<string, byte[]> headers)
        {
            _publisher.Publish(payload, _exchange, routingKey, headers);
        }
    }

    public class Exchange<T> : Exchange, IRabbitExchange<T>
    {
        #region Constructors

        public Exchange(string exchange, IRabbitPublisher publisher, ISerializer serializer)
            : base(exchange, publisher, serializer)
        {
        }

        #endregion Constructors

        #region IRabbitExchange<T> Members

        public void Publish(T message, string routingKey)
        {
            base.Publish(_serializer.Serialize(message), routingKey);
        }

        public void Publish(T message, string routingKey, IDictionary<string, object> headers)
        {
            base.Publish(_serializer.Serialize(message), routingKey, headers);
        }

        #endregion IRabbitExchange<T> Members
    }

    internal sealed class QueueResult<T> : IRabbitQueue<T>
    {
        private readonly IRabbitMqClientLogger _logger;
        private readonly QueueResult _openResult;

        #region Constructors

        public QueueResult(QueueResult openResult, IRabbitMqClientLogger logger)
        {
            _openResult = openResult;
            _logger = logger;
        }

        #endregion Constructors

        #region IRabbitQueue<T> Members

        IListeningConnection<T> IRabbitQueue<T>.Listen()
        {
            return new ListeningConnection<T>(_openResult.Listen(), _logger);
        }

        IListeningConnection<T> IRabbitQueue<T>.Listen(TimeSpan openTimeout)
        {
            return new ListeningConnection<T>(_openResult.Listen(openTimeout), _logger);
        }

        IListeningConnection<T> IRabbitQueue<T>.Listen(TimeSpan openTimeout, StreamTransform transform)
        {
            return new ListeningConnection<T>(_openResult.Listen(openTimeout, transform), _logger);
        }

        #endregion IRabbitQueue<T> Members
    }

    public class QueueResult : IRabbitQueue
    {
        private readonly string _exchange;
        private readonly object _listenPadlock;
        private readonly string _queueName;
        private readonly IQueueSubscriptionManager _queueSubscriptionManager;
        private readonly string _routingKey;
        private readonly IRabbitMqSubscriber _subscriberThread;

        #region Constructors

        public QueueResult(string exchange, string queueName, string routingKey, IQueueSubscriptionManager subscription, IRabbitMqSubscriber subscriberThread)
        {
            _exchange = exchange;
            _queueName = queueName;
            _routingKey = routingKey;
            _queueSubscriptionManager = subscription;
            _subscriberThread = subscriberThread;
            _listenPadlock = new object();
        }

        #endregion Constructors

        #region IRabbitQueue Members

        public IListeningConnection Listen()
        {
            return ListenInternal(TimeSpan.FromMinutes(1), null);
        }

        public IListeningConnection Listen(TimeSpan openTimeout)
        {
            return ListenInternal(openTimeout, null);
        }

        public IListeningConnection Listen(TimeSpan openTimeout, StreamTransform transform)
        {
            return ListenInternal(openTimeout, transform);
        }

        #endregion IRabbitQueue Members

        private IListeningConnection ListenInternal(TimeSpan openTimeout, StreamTransform transform)
        {
            var queueSubscription = _queueSubscriptionManager.GetSubscription(_exchange, _queueName, _routingKey);

            lock (_listenPadlock)
            {
                if (queueSubscription.ConsumingTask == null)
                    queueSubscription.ConsumingTask = Task.Factory.StartNew(() => _subscriberThread.Subscribe(queueSubscription));
                else return new ListeningConnection(queueSubscription, transform);
            }

            var opened = queueSubscription.OpenEvent.WaitOne(openTimeout);
            if (opened) return new ListeningConnection(queueSubscription, transform);
            throw new Exception(string.Format("Could not open listening channel within {0} to {1}", openTimeout, queueSubscription.SubscriptionData));
        }
    }
}