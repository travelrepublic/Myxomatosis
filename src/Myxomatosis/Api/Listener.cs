using Myxomatosis.Connection;
using Myxomatosis.Connection.Exchange;
using Myxomatosis.Connection.Queue;
using Myxomatosis.Connection.Queue.Listen;
using RabbitMQ.Client;
using System;

namespace Myxomatosis.Api
{
    /// <summary>
    /// Represents a single connection to a RabbitMQ Server
    /// </summary>
    internal class ConnectionEntryPoint : IObservableConnection
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly RabbitMqQueueListener _subscriber;
        private readonly QueueSubscriptionCache _subscriptionCache;

        #region Constructors

        public ConnectionEntryPoint(ConnectionFactory connectionFactory, RabbitMqQueueListener subscriber)
        {
            _connectionFactory = connectionFactory;
            _subscriber = subscriber;
            _subscriptionCache = new QueueSubscriptionCache();
        }

        #endregion Constructors

        #region IObservableConnection Members

        public IQueue Queue(string queueName)
        {
            return new RabbitQueue(queueName, _subscriber, _subscriptionCache);
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