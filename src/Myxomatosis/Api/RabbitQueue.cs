using Myxomatosis.Connection;
using Myxomatosis.Connection.Message;
using Myxomatosis.Connection.Queue.Listen;
using System;
using System.Threading;

namespace Myxomatosis.Api
{
    /// <summary>
    /// Represents an instance of a RabbitMQ Queue
    /// </summary>
    internal class RabbitQueue : IQueue
    {
        private readonly string _queueName;
        private readonly RabbitMqQueueListener _queueListener;
        private readonly QueueSubscriptionCache _subscriptionCache;

        private Thread _consumingThread;

        #region Constructors

        public RabbitQueue(string queueName, RabbitMqQueueListener queueListener, QueueSubscriptionCache subscriptionCache)
        {
            _queueName = queueName;
            _queueListener = queueListener;
            _subscriptionCache = subscriptionCache;
            _consumingThread = null;
        }

        #endregion Constructors

        #region IQueueOpener Members

        public IQueueConnection Open(Action<ISubscriberConfigBuilder> configBuilder)
        {
            var config = GetConfig(configBuilder);
            var queueSubscription = _subscriptionCache.Create(_queueName, config.PrefetchCount, config.Args);

            // Only start new thread if one does not currently exist 
            Interlocked.CompareExchange(ref _consumingThread, new Thread(() => _queueListener.Listen(queueSubscription) ), null);
            if (_consumingThread.IsAlive)
                return new ListeningConnection(queueSubscription, config, _subscriptionCache);

            _consumingThread.Start();

            // Wait for connection to open
            var opened = queueSubscription.OpenEvent.WaitOne(config.OpenTimeout);
            if (opened) return new ListeningConnection(queueSubscription, config, _subscriptionCache);
            throw new Exception(string.Format("Could not open listening channel within {0} to {1}", config.OpenTimeout, queueSubscription));
        }

        public IQueueConnection Open()
        {
            return Open(_ => { });
        }

        #endregion IQueueOpener Members

        private ISubscriptionConfig GetConfig(Action<ISubscriberConfigBuilder> configBuilder)
        {
            var defaultConfig = new SubscriberConfigBuilder(_queueName);
            configBuilder(defaultConfig);
            return defaultConfig.GetConfig();
        }
    }
}