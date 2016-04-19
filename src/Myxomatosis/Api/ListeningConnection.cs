using Myxomatosis.Connection;
using Myxomatosis.Connection.Message;
using Myxomatosis.Connection.Queue.Listen;
using Myxomatosis.Logging;
using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Myxomatosis.Api
{
    public class QueueOpener : IQueueOpener
    {
        private readonly object _listenPadlock;
        private readonly string _queueName;
        private readonly IRabbitMqSubscriber _subscriberThread;
        private readonly QueueSubscriptionCache _subscriptionCache;

        #region Constructors

        public QueueOpener(string queueName, IRabbitMqSubscriber subscriberThread, QueueSubscriptionCache subscriptionCache)
        {
            _queueName = queueName;
            _subscriberThread = subscriberThread;
            _subscriptionCache = subscriptionCache;
            _listenPadlock = new object();
        }

        #endregion Constructors

        #region IQueueOpener Members

        public IQueueConnection Open(Action<ISubscriberConfigBuilder> configBuilder)
        {
            var config = GetConfig(configBuilder);
            var queueSubscription = _subscriptionCache.Create(_queueName, config.PrefetchCount, config.Args);

            lock (_listenPadlock)
            {
                if (queueSubscription.ConsumingTask == null)
                    queueSubscription.ConsumingTask = _subscriberThread.SubscribeAsync(queueSubscription);
                else return new ListeningConnection(queueSubscription, config, _subscriptionCache);
            }

            var opened = queueSubscription.OpenEvent.WaitOne(config.OpenTimeout);
            if (opened) return new ListeningConnection(queueSubscription, config, _subscriptionCache);
            throw new Exception(string.Format("Could not open listening channel within {0} to {1}", config.OpenTimeout, queueSubscription.SubscriptionData));
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

    public class ListeningConnection : IQueueConnection
    {
        private readonly ISubscriptionConfig _config;
        private readonly QueueSubscription _queueSubscription;
        private readonly QueueSubscriptionCache _subscriptionCache;

        #region Constructors

        public ListeningConnection(QueueSubscription queueSubscription, ISubscriptionConfig config, QueueSubscriptionCache subscriptionCache)
        {
            _queueSubscription = queueSubscription;
            _config = config;
            _subscriptionCache = subscriptionCache;
            Logger = new RabbitMqConsoleLogger();
        }

        #endregion Constructors

        public IRabbitMqClientLogger Logger { get; set; }

        #region IQueueConnection Members

        public bool IsOpen
        {
            get { return _queueSubscription.ConsumingTask.Status != TaskStatus.RanToCompletion; }
        }

        public IObservable<RabbitMessage> Stream()
        {
            return GetRawStream();
        }

        public IObservable<RabbitMessage<T>> Stream<T>()
        {
            var stream = GetRawStream().ToMessage<T>();
            return stream;
        }

        public void Dispose()
        {
            try
            {
                Close();
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion IQueueConnection Members

        private CloseConnectionResult Close()
        {
            _queueSubscription.KeepListening = false;
            var closed = _queueSubscription.ConsumingTask.Wait(_config.CloseTimeout);

            _subscriptionCache.Remove(_queueSubscription.SubscriptionData.Queue);

            return new CloseConnectionResult(closed);
        }

        private IObservable<RabbitMessage> GetRawStream()
        {
            /**
             * When the subscriber is disposed we want to dispose of the underlying connection
             * */
            var stream = Observable.Using(() => new ConnectionDisposer(this), cd => cd.MessageStream);
            return stream;
        }

        #region Nested type: ConnectionDisposer

        private class ConnectionDisposer : IDisposable
        {
            private readonly ListeningConnection _connection;

            #region Constructors

            public ConnectionDisposer(ListeningConnection connection)
            {
                _connection = connection;
                MessageStream = connection._queueSubscription.MessageSource;
            }

            #endregion Constructors

            public IObservable<RabbitMessage> MessageStream { get; private set; }

            #region IDisposable Members

            public void Dispose()
            {
                _connection.Close();
            }

            #endregion IDisposable Members
        }

        #endregion Nested type: ConnectionDisposer
    }
}