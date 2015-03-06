using Myxomatosis.Api;
using Myxomatosis.Configuration;
using Myxomatosis.Connection;
using Myxomatosis.Connection.Errors;
using Myxomatosis.Connection.Exchange;
using Myxomatosis.Connection.Queue.Listen;
using RabbitMQ.Client;
using System;

namespace Myxomatosis
{
    public static class ObservableConnectionFactory
    {
        public static IObservableConnection Create()
        {
            return Create(c => { });
        }

        public static IObservableConnection Create(Action<IConnectionConfigBuilder> config)
        {
            var configuration = new ConnectionConfigBuilder();
            config(configuration);
            var connectionConfig = configuration.Create();
            return Create(connectionConfig);
        }

        public static IObservableConnection Create(IConnectionConfig config)
        {
            var connectionFactory = new ConnectionFactory
            {
                HostName = config.HostName,
                Password = config.Password,
                UserName = config.UserName,
                VirtualHost = config.VirtualHost
            };
            var publisher = new RabbitMqPublisher(connectionFactory);
            var errorPublisher = new RabbitMqPublisher(connectionFactory);
            var errorHandler = new UnhandledErrorHandler(errorPublisher, config.Serializer);
            var logger = config.Logger;
            var subscriber = new RabbitMqSubscriber(connectionFactory, errorHandler, logger);
            var subscriptionManager = new SubscriptionManager();
            var listener = new Listener(subscriber, publisher, subscriptionManager, config.Serializer, logger);
            return listener;
        }
    }
}