using Myxomatosis.Api;
using Myxomatosis.Configuration;
using Myxomatosis.Connection;
using Myxomatosis.Connection.Errors;
using Myxomatosis.Connection.Exchange;
using Myxomatosis.Connection.Queue.Listen;
using Myxomatosis.Logging;
using RabbitMQ.Client;
using System;
using System.Linq;

namespace Myxomatosis
{
    public static class ObservableConnectionFactory
    {
        public static IObservableConnection Create()
        {
            return Create(c => { });
        }

        public static IObservableConnection Create(string connectionString, IRabbitMqClientLogger logger = null)
        {
            var keyValues =
                connectionString.Split(';').Select(s =>
                {
                    var items = s.Split('=');
                    return new { key = items[0], value = items[1] };
                }).ToDictionary(kvp => kvp.key, kvp => kvp.value);

            var config = new DefaultConfiguration();
            if (logger != null) config.Logger = logger;
            if (keyValues.ContainsKey("host")) config.HostName = keyValues["host"];
            if (keyValues.ContainsKey("username")) config.UserName = keyValues["username"];
            if (keyValues.ContainsKey("password")) config.Password = keyValues["password"];
            if (keyValues.ContainsKey("virtualhost")) config.VirtualHost = keyValues["virtualhost"];
            if (keyValues.ContainsKey("prefetchcount")) config.PrefetchCount = Convert.ToUInt16(keyValues["prefetchcount"]);
            return Create(config);
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
            var subscriber = new RabbitMqQueueListener(connectionFactory, errorHandler, logger);
            var listener = new ConnectionEntryPoint(connectionFactory, subscriber);
            return listener;
        }
    }
}