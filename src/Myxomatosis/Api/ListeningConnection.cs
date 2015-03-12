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

        public QueueOpener(string queueName, IRabbitMqSubscriber subscriberThread)
        {
            _queueName = queueName;
            _subscriberThread = subscriberThread;
            _listenPadlock = new object();
        }

        public IQueueConnection Open(Action<ISubscriberConfigBuilder> configBuilder)
        {
            var queueSubscription = QueueSubscription.Create(_queueName);

            var config = GetConfig(configBuilder);

            lock (_listenPadlock)
            {
                if (queueSubscription.ConsumingTask == null)
                    queueSubscription.ConsumingTask = _subscriberThread.SubscribeAsync(queueSubscription);
                else return new ListeningConnection(queueSubscription, config);
            }

            var opened = queueSubscription.OpenEvent.WaitOne(config.OpenTimeout);
            if (opened) return new ListeningConnection(queueSubscription, config);
            throw new Exception(string.Format("Could not open listening channel within {0} to {1}", config.OpenTimeout, queueSubscription.SubscriptionData));
        }

        public IQueueConnection Open()
        {
            return Open(_ => { });
        }

        private ISubscriptionConfig GetConfig(Action<ISubscriberConfigBuilder> configBuilder)
        {
            SubscriberConfigBuilder defaultConfig = new SubscriberConfigBuilder(_queueName);
            configBuilder(defaultConfig);
            return defaultConfig.GetConfig();
        }
    }

    public class ListeningConnection : IQueueConnection
    {
        private readonly ISubscriptionConfig _config;

        private readonly QueueSubscription _queueSubscription;

        #region Constructors

        public ListeningConnection(QueueSubscription queueSubscription, ISubscriptionConfig config)
        {
            _queueSubscription = queueSubscription;
            _config = config;
            Logger = new RabbitMqConsoleLogger();
        }

        #endregion Constructors

        public IRabbitMqClientLogger Logger { get; set; }

        #region IQueueConnection Members

        public bool IsOpen
        {
            get { return _queueSubscription.ConsumingTask.Status != TaskStatus.RanToCompletion; }
        }

        private CloseConnectionResult Close()
        {
            _queueSubscription.KeepListening = false;
            var closed = _queueSubscription.ConsumingTask.Wait(_config.CloseTimeout);
            QueueSubscription.Remove(_queueSubscription.SubscriptionData.Queue);
            return new CloseConnectionResult(closed);
        }

        public IObservable<RabbitMessage> Stream()
        {
            return GetRawStream();
        }

        public IObservable<RabbitMessage<T>> Stream<T>()
        {
            var stream = GetRawStream().ToMessage<T>();
            //            stream.Subscribe(m => { }, e => { Logger.LogError("Myxomatosis exception: ", e); });
            return stream;
        }

        #endregion IQueueConnection Members

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
    }
}