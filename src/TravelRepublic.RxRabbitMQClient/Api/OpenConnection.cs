using System;
using System.Threading.Tasks;
using TravelRepublic.RxRabbitMQClient.Connection;
using TravelRepublic.RxRabbitMQClient.Connection.Queue.Listen;

namespace TravelRepublic.RxRabbitMQClient.Api
{
    public abstract class OpenConnection : IOpenConnection
    {
        private readonly QueueSubscription _queueSubscription;

        #region Constructors

        protected OpenConnection(QueueSubscription queueSubscription)
        {
            _queueSubscription = queueSubscription;
        }

        #endregion Constructors

        #region IOpenConnection Members

        public bool IsOpen
        {
            get { return _queueSubscription.ConsumingTask.Status == TaskStatus.RanToCompletion; }
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
            var closed = _queueSubscription.ConsumingTask.Wait(closeTimeout);
            return new CloseConnectionResult(closed);
        }
    }
}