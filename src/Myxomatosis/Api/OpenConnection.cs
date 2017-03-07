using System;
using System.Threading.Tasks;
using Myxomatosis.Connection;
using Myxomatosis.Connection.Queue.Listen;

namespace Myxomatosis.Api
{
    /// <summary>
    /// Used to manage an open connection to RabbitMQ server, listenening on a particular queue
    /// </summary>
    internal abstract class OpenConnection : IOpenConnection
    {
        private readonly QueueSubscriptionToken _queueSubscription;

        #region Constructors

        protected OpenConnection(QueueSubscriptionToken queueSubscription)
        {
            _queueSubscription = queueSubscription;
        }

        #endregion Constructors

        #region IOpenConnection Members

        public bool IsOpen
        {
            get { return _queueSubscription.KeepListening; }
        }

        public CloseConnectionResult Close()
        {
            return CloseConnectionInternal(TimeSpan.FromMinutes(1));
        }

        public CloseConnectionResult Close(TimeSpan closeTimeout)
        {
            return CloseConnectionInternal(closeTimeout);
        }

        #endregion IOpenConnection Members

        private CloseConnectionResult CloseConnectionInternal(TimeSpan closeTimeout)
        {
            _queueSubscription.KeepListening = false;
            var closed = _queueSubscription.ClosedEvent.WaitOne(closeTimeout);
            return new CloseConnectionResult(closed);
        }
    }
}