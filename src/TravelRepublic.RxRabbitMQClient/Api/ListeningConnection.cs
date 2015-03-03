using System;
using System.Linq;
using System.Reactive.Linq;
using TravelRepublic.RxRabbitMQClient.Connection;
using TravelRepublic.RxRabbitMQClient.Connection.Message;
using TravelRepublic.RxRabbitMQClient.Connection.Queue;
using TravelRepublic.RxRabbitMQClient.Connection.Queue.Listen;
using TravelRepublic.RxRabbitMQClient.Serialization;

namespace TravelRepublic.RxRabbitMQClient.Api
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
            return _listeningConnection.ToObservable()
                .Select(m => new RabbitMessage<T>
                {
                    DeliveryTag = m.DeliveryTag,
                    Channel = m.Channel,
                    RawHeaders = m.RawHeaders,
                    Headers = m.RawHeaders.ToDictionary(i => i.Key, i => _serializer.Deserialize<object>(i.Value)),
                    RawMessage = m.RawMessage,
                    Message = _serializer.Deserialize<T>(m.RawMessage),
                    ErrorHandler = m.ErrorHandler
                });
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
            return _transform(_queueSubscription.MessageSource);
        }

        #endregion IListeningConnection Members
    }
}