using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Myxomatosis.Connection;
using Myxomatosis.Connection.Exchange;
using Myxomatosis.Connection.Queue;
using Myxomatosis.Connection.Queue.Listen;
using Myxomatosis.Serialization;

namespace Myxomatosis.Api
{
    internal sealed class QueueResult<T> : IRabbitQueue<T>
    {
        private readonly QueueResult _openResult;
        private readonly ISerializer _serializer;

        #region Constructors

        public QueueResult(QueueResult openResult, ISerializer serializer)
        {
            _openResult = openResult;
            _serializer = serializer;
        }

        #endregion Constructors

        #region IRabbitQueue<T> Members

        IListeningConnection<T> IRabbitQueue<T>.Listen()
        {
            return new ListeningConnection<T>(_openResult.Listen(), _serializer);
        }

        IListeningConnection<T> IRabbitQueue<T>.Listen(TimeSpan openTimeout)
        {
            return new ListeningConnection<T>(_openResult.Listen(openTimeout), _serializer);
        }

        IListeningConnection<T> IRabbitQueue<T>.Listen(TimeSpan openTimeout, StreamTransform transform)
        {
            return new ListeningConnection<T>(_openResult.Listen(openTimeout, transform), _serializer);
        }

        public void Publish(T message)
        {
            _openResult.Publish(_serializer.Serialize(message));
        }

        public void Publish(T message, IDictionary<string, object> headers)
        {
            _openResult.Publish(_serializer.Serialize(message), headers.ToDictionary(kvp => kvp.Key, kvp => _serializer.Serialize(kvp.Value)));
        }

        #endregion IRabbitQueue<T> Members
    }

    public class QueueResult : IRabbitQueue
    {
        private readonly string _exchange;
        private readonly object _listenPadlock;
        private readonly IRabbitPublisher _publisher;
        private readonly string _queueName;
        private readonly IQueueSubscriptionManager _queueSubscriptionManager;
        private readonly IRabbitMqSubscriber _subscriberThread;

        #region Constructors

        public QueueResult(string exchange, string queueName, IQueueSubscriptionManager subscription, IRabbitPublisher publisher, IRabbitMqSubscriber subscriberThread)
        {
            _exchange = exchange;
            _queueName = queueName;
            _queueSubscriptionManager = subscription;
            _publisher = publisher;
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

        public void Publish(byte[] payload)
        {
            _publisher.Publish(payload, _exchange);
        }

        #endregion IRabbitQueue Members

        private IListeningConnection ListenInternal(TimeSpan openTimeout, StreamTransform transform)
        {
            var queueSubscription = _queueSubscriptionManager.GetSubscription(_exchange, _queueName);

            lock (_listenPadlock)
            {
                if (queueSubscription.ConsumingTask == null) queueSubscription.ConsumingTask = Task.Factory.StartNew(() => _subscriberThread.Subscribe(queueSubscription));
                else return new ListeningConnection(queueSubscription, transform);
            }

            var opened = queueSubscription.OpenEvent.WaitOne(openTimeout);
            if (opened) return new ListeningConnection(queueSubscription, transform);
            throw new Exception(string.Format("Could not open listening channel within {0} to {1}", openTimeout, queueSubscription.QueueName));
        }

        public void Publish(byte[] payload, IDictionary<string, byte[]> headers)
        {
            _publisher.Publish(payload, _exchange, headers);
        }
    }
}