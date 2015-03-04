using System;
using System.Reactive.Linq;
using Myxomatosis.Connection;
using Myxomatosis.Connection.Message;
using Myxomatosis.Connection.Queue;
using Myxomatosis.Connection.Queue.Listen;
using Myxomatosis.Serialization;

namespace Myxomatosis.Api
{
    internal class ListeningConnection<T> : IListeningConnection<T>
    {
        private readonly IListeningConnection _listeningConnection;
        private readonly ISerializer _serializer;

        #region Constructors

        public ListeningConnection(
            IListeningConnection listeningConnection,
            ISerializer serializer)
        {
            _listeningConnection = listeningConnection;
            _serializer = serializer;
        }

        #endregion Constructors

        #region IListeningConnection<T> Members

        public bool IsOpen
        {
            get { return _listeningConnection.IsOpen; }
        }

        public CloseConnectionResult Close()
        {
            return _listeningConnection.Close();
        }

        public CloseConnectionResult Close(TimeSpan closeTimeout)
        {
            return _listeningConnection.Close(closeTimeout);
        }

        public IObservable<RabbitMessage<T>> ToObservable()
        {
            return _listeningConnection.ToObservable().ToMessage<T>();
        }

        #endregion IListeningConnection<T> Members
    }

    public class ListeningConnection : OpenConnection, IListeningConnection
    {
        private readonly QueueSubscription _queueSubscription;
        private readonly StreamTransform _transform;

        #region Constructors

        public ListeningConnection(QueueSubscription queueSubscription, StreamTransform transform)
            : base(queueSubscription)
        {
            _queueSubscription = queueSubscription;
            _transform = transform ?? (m => m);
        }

        #endregion Constructors

        #region IListeningConnection Members

        public IObservable<RabbitMessage> ToObservable()
        {
            /**
             * When the subscriber is disposed we want to dispose of the underlying connection
             * */
            return Observable.Using(() => new ConnectionDisposer(this), cd => cd.MessageStream);
        }

        #endregion IListeningConnection Members

        #region Nested type: ConnectionDisposer

        private class ConnectionDisposer : IDisposable
        {
            private readonly ListeningConnection _connection;

            #region Constructors

            public ConnectionDisposer(ListeningConnection connection)
            {
                _connection = connection;
                MessageStream = connection._transform(connection._queueSubscription.MessageSource);
            }

            #endregion Constructors

            public IObservable<RabbitMessage> MessageStream { get; set; }

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