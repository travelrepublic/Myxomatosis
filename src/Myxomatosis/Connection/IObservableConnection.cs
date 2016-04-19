using System.Collections.Generic;
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
        IFanoutExchangeBuilder BoundToQueue(string queueName, Dictionary<string, object> args);

        IFanoutExchangeBuilder BoundToExchange(string exchangeName);
    }

    public interface ITopicExchangeBuilder
    {
        ITopicExchangeBuilder BoundToQueue(string queueName);

        ITopicExchangeBuilder BoundToQueue(string queueName, string routingKey, Dictionary<string, object> args);

        ITopicExchangeBuilder BoundToExchange(string exchangeName);

        ITopicExchangeBuilder BoundToExchange(string exchangeName, string routingKey);
    }

    internal class TopologyBuilder : ITopologyBuilder
    {
        private readonly ConnectionFactory _connectionFactory;

        #region Constructors

        public TopologyBuilder(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        #endregion Constructors

        #region ITopologyBuilder Members

        public IExchangeTypes Exchange(string exchangeName)
        {
            return new ExchangeTypes(exchangeName, _connectionFactory);
        }

        #endregion ITopologyBuilder Members

        #region Nested type: ExchangeBuilderBase

        private abstract class ExchangeBuilderBase
        {
            private readonly ExchangeType _exchangeType;
            protected readonly ConnectionFactory ConnectionFactory;
            protected readonly string ExchangeName;

            #region Constructors

            protected ExchangeBuilderBase(ConnectionFactory connectionFactory, string exchangeName, ExchangeType exchangeType)
            {
                ConnectionFactory = connectionFactory;
                ExchangeName = exchangeName;
                _exchangeType = exchangeType;

                CreateExchange();
            }

            #endregion Constructors

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

        #endregion Nested type: ExchangeBuilderBase

        #region Nested type: ExchangeTypes

        private class ExchangeTypes : IExchangeTypes
        {
            private readonly ConnectionFactory _connectionFactory;
            private readonly string _exchangeName;

            #region Constructors

            public ExchangeTypes(string exchangeName, ConnectionFactory connectionFactory)
            {
                _exchangeName = exchangeName;
                _connectionFactory = connectionFactory;
            }

            #endregion Constructors

            #region IExchangeTypes Members

            public IFanoutExchangeBuilder Fanout
            {
                get { return new FanoutExchangeBuidler(_connectionFactory, _exchangeName); }
            }

            public ITopicExchangeBuilder Topic
            {
                get { return new TopicExchangeBuilder(_connectionFactory, _exchangeName); }
            }

            #endregion IExchangeTypes Members
        }

        #endregion Nested type: ExchangeTypes

        #region Nested type: FanoutExchangeBuidler

        private class FanoutExchangeBuidler : ExchangeBuilderBase, IFanoutExchangeBuilder
        {
            #region Constructors

            public FanoutExchangeBuidler(ConnectionFactory connectionFactory, string exchangeName)
                : base(connectionFactory, exchangeName, ExchangeType.Fanout)
            {
            }

            #endregion Constructors

            #region IFanoutExchangeBuilder Members

            public IFanoutExchangeBuilder BoundToQueue(string queueName, Dictionary<string, object> args)
            {
                using (var connection = ConnectionFactory.CreateConnection())
                {
                    using (var model = connection.CreateModel())
                    {
                        model.QueueDeclare(queueName, true, false, false, args);
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

            #endregion IFanoutExchangeBuilder Members
        }

        #endregion Nested type: FanoutExchangeBuidler

        #region Nested type: TopicExchangeBuilder

        private class TopicExchangeBuilder : ExchangeBuilderBase, ITopicExchangeBuilder
        {
            #region Constructors

            public TopicExchangeBuilder(ConnectionFactory connectionFactory, string exchangeName)
                : base(connectionFactory, exchangeName, ExchangeType.Topic)
            {
            }

            #endregion Constructors

            #region ITopicExchangeBuilder Members

            public ITopicExchangeBuilder BoundToQueue(string queueName)
            {
                return BoundToQueue(queueName, string.Empty, null);
            }

            public ITopicExchangeBuilder BoundToQueue(string queueName, string routingKey, Dictionary<string, object> args)
            {
                using (var connection = ConnectionFactory.CreateConnection())
                {
                    using (var model = connection.CreateModel())
                    {
                        model.QueueDeclare(queueName, true, false, false, args);
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

            #endregion ITopicExchangeBuilder Members
        }

        #endregion Nested type: TopicExchangeBuilder
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

        ISubscriberConfigBuilder PrefetchCount(ushort prefetchCount);

        ISubscriberConfigBuilder ArgList(Dictionary<string, object> args);
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

        public ISubscriberConfigBuilder PrefetchCount(ushort prefetchCount)
        {
            _subscriptionConfig.PrefetchCount = prefetchCount;
            return this;
        }

        public ISubscriberConfigBuilder ArgList(Dictionary<string, object> args)
        {
            _subscriptionConfig.Args = args;
            return this;

        }

        #endregion ISubscriberConfigBuilder Members

        public ISubscriptionConfig GetConfig()
        {
            return _subscriptionConfig;
        }
    }
}