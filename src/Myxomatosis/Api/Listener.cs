using Myxomatosis.Connection;
using Myxomatosis.Connection.Exchange;
using Myxomatosis.Connection.Queue;
using Myxomatosis.Connection.Queue.Listen;
using RabbitMQ.Client;
using System;

namespace Myxomatosis.Api
{
    public class ConnectionEntryPoint : IObservableConnection
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly IRabbitMqSubscriber _subscriber;
        private readonly QueueSubscriptionCache _subscriptionCache;

        #region Constructors

        public ConnectionEntryPoint(ConnectionFactory connectionFactory, IRabbitMqSubscriber subscriber)
        {
            _connectionFactory = connectionFactory;
            _subscriber = subscriber;
            _subscriptionCache = new QueueSubscriptionCache();
        }

        #endregion Constructors

        #region IObservableConnection Members

        public IQueueOpener Queue(string queueName)
        {
            return new QueueOpener(queueName, _subscriber, _subscriptionCache);
        }

        public IExchange Exchange(string exchange)
        {
            return new Exchange(_connectionFactory, exchange);
        }

        public IObservableConnection SetUp(Action<ITopologyBuilder> topologyConfig)
        {
            var topologyBuilder = new TopologyBuilder(_connectionFactory);
            topologyConfig(topologyBuilder);
            return this;
        }

        #endregion IObservableConnection Members
    }
}