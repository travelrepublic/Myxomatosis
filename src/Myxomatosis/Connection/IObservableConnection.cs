using Myxomatosis.Connection.Message;
using Myxomatosis.Connection.Queue;
using RabbitMQ.Client;
using System;

namespace Myxomatosis.Connection
{
    public interface IObservableConnection
    {
        IQueueOpener Queue(string queueName);

        IExchange Exchange(string exchange);

        IObservableConnection SetUp(Action<ITopologyBuilder> topologyConfig);
    }

    public enum ExchangeType
    {
        Topic,
        Fanout
    }

    internal static class ExchangeTypeHelpers
    {
        public static string ToRabbitExchange(this ExchangeType exchangeType)
        {
            switch (exchangeType)
            {
                case ExchangeType.Fanout:
                    return "fanout";

                case ExchangeType.Topic:
                    return "topic";
            }

            throw new Exception("Unknown Exchange Type");
        }
    }

    public interface ITopologyBuilder
    {
        IExchangeTypes Exchange(string exchangeName);
    }

    public interface IExchangeTypes
    {
        IFanoutExchangeBuilder Fanout { get; }

        ITopicExchangeBuilder Topic { get; }
    }

    public interface IFanoutExchangeBuilder
    {
        IFanoutExchangeBuilder BoundToQueue(string queueName);

        IFanoutExchangeBuilder BoundToExchange(string exchangeName);
    }

    public interface ITopicExchangeBuilder
    {
        ITopicExchangeBuilder BoundToQueue(string queueName);

        ITopicExchangeBuilder BoundToQueue(string queueName, string routingKey);

        ITopicExchangeBuilder BoundToExchange(string exchangeName);

        ITopicExchangeBuilder BoundToExchange(string exchangeName, string routingKey);
    }

    internal class TopologyBuilder : ITopologyBuilder
    {
        private readonly ConnectionFactory _connectionFactory;

        public TopologyBuilder(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IExchangeTypes Exchange(string exchangeName)
        {
            return new ExchangeTypes(exchangeName, _connectionFactory);
        }

        private class ExchangeTypes : IExchangeTypes
        {
            private readonly string _exchangeName;
            private readonly ConnectionFactory _connectionFactory;

            public ExchangeTypes(string exchangeName, ConnectionFactory connectionFactory)
            {
                _exchangeName = exchangeName;
                this._connectionFactory = connectionFactory;
            }

            public IFanoutExchangeBuilder Fanout { get { return new FanoutExchangeBuidler(_connectionFactory, _exchangeName); } }

            public ITopicExchangeBuilder Topic { get { return new TopicExchangeBuilder(_connectionFactory, _exchangeName); } }
        }

        private abstract class ExchangeBuilderBase
        {
            protected readonly ConnectionFactory ConnectionFactory;
            protected readonly string ExchangeName;
            private readonly ExchangeType _exchangeType;

            protected ExchangeBuilderBase(ConnectionFactory connectionFactory, string exchangeName, ExchangeType exchangeType)
            {
                ConnectionFactory = connectionFactory;
                ExchangeName = exchangeName;
                _exchangeType = exchangeType;

                CreateExchange();
            }

            private void CreateExchange()
            {
                using (var connection = ConnectionFactory.CreateConnection())
                {
                    using (var model = connection.CreateModel())
                    {
                        model.ExchangeDeclare(ExchangeName, _exchangeType.ToRabbitExchange(), true, false, null);
                    }
                }
            }
        }

        private class FanoutExchangeBuidler : ExchangeBuilderBase, IFanoutExchangeBuilder
        {
            public FanoutExchangeBuidler(ConnectionFactory connectionFactory, string exchangeName)
                : base(connectionFactory, exchangeName, ExchangeType.Fanout)
            {
            }

            public IFanoutExchangeBuilder BoundToQueue(string queueName)
            {
                using (var connection = ConnectionFactory.CreateConnection())
                {
                    using (var model = connection.CreateModel())
                    {
                        model.QueueDeclare(queueName, true, false, false, null);
                        model.QueueBind(queueName, ExchangeName, string.Empty, null);
                    }
                }

                return this;
            }

            public IFanoutExchangeBuilder BoundToExchange(string exchangeName)
            {
                using (var connection = ConnectionFactory.CreateConnection())
                {
                    using (var model = connection.CreateModel())
                    {
                        model.ExchangeBind(exchangeName, ExchangeName, string.Empty);
                    }
                }

                return this;
            }
        }

        private class TopicExchangeBuilder : ExchangeBuilderBase, ITopicExchangeBuilder
        {
            public TopicExchangeBuilder(ConnectionFactory connectionFactory, string exchangeName)
                : base(connectionFactory, exchangeName, ExchangeType.Topic)
            {
            }

            public ITopicExchangeBuilder BoundToQueue(string queueName)
            {
                return BoundToQueue(queueName, string.Empty);
            }

            public ITopicExchangeBuilder BoundToQueue(string queueName, string routingKey)
            {
                using (var connection = ConnectionFactory.CreateConnection())
                {
                    using (var model = connection.CreateModel())
                    {
                        model.QueueDeclare(queueName, true, false, false, null);
                        model.QueueBind(queueName, ExchangeName, routingKey, null);
                    }
                }

                return this;
            }

            public ITopicExchangeBuilder BoundToExchange(string exchangeName)
            {
                return BoundToExchange(exchangeName, string.Empty);
            }

            public ITopicExchangeBuilder BoundToExchange(string exchangeName, string routingKey)
            {
                using (var connection = ConnectionFactory.CreateConnection())
                {
                    using (var model = connection.CreateModel())
                    {
                        model.ExchangeBind(exchangeName, ExchangeName, routingKey);
                    }
                }

                return this;
            }
        }
    }

    public interface IQueueOpener
    {
        IQueueConnection Open();

        IQueueConnection Open(Action<ISubscriberConfigBuilder> configBuilder);
    }

    public interface IQueueConnection : IDisposable
    {
        bool IsOpen { get; }

        IObservable<RabbitMessage> Stream();

        IObservable<RabbitMessage<T>> Stream<T>();
    }

    public interface ISubscriberConfigBuilder
    {
        ISubscriberConfigBuilder OpenTimeout(TimeSpan openTimeout);

        ISubscriberConfigBuilder CloseTimeout(TimeSpan openTimeout);
    }

    internal class SubscriberConfigBuilder : ISubscriberConfigBuilder
    {
        private readonly ISubscriptionConfig _subscriptionConfig;

        #region Constructors

        public SubscriberConfigBuilder(string queueName)
        {
            _subscriptionConfig = new DefaultSubscriptionConfig(queueName);
        }

        #endregion Constructors

        #region ISubscriberConfigBuilder Members

        public ISubscriberConfigBuilder OpenTimeout(TimeSpan openTimeout)
        {
            _subscriptionConfig.OpenTimeout = openTimeout;
            return this;
        }

        public ISubscriberConfigBuilder CloseTimeout(TimeSpan closeTimeout)
        {
            _subscriptionConfig.CloseTimeout = closeTimeout;
            return this;
        }

        #endregion ISubscriberConfigBuilder Members

        public ISubscriptionConfig GetConfig()
        {
            return _subscriptionConfig;
        }
    }
}