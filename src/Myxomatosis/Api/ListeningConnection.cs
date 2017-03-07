using Myxomatosis.Connection;
using Myxomatosis.Connection.Message;
using Myxomatosis.Connection.Queue.Listen;
using Myxomatosis.Logging;
using System;
using System.Reactive.Linq;

namespace Myxomatosis.Api
{
    internal class ListeningConnection : IQueueConnection
    {
        private readonly ISubscriptionConfig _config;
        private readonly QueueSubscriptionToken _queueSubscription;
        private readonly QueueSubscriptionCache _subscriptionCache;

        #region Constructors

        public ListeningConnection(QueueSubscriptionToken queueSubscription, ISubscriptionConfig config, QueueSubscriptionCache subscriptionCache)
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
            get { return _queueSubscription.KeepListening; }
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
            var closed = _queueSubscription.ClosedEvent.WaitOne(_config.CloseTimeout);

            _subscriptionCache.Remove(_queueSubscription.QueueName);

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
