using System.Collections.Concurrent;

namespace Myxomatosis.Connection.Queue.Listen
{
    public class SubscriptionManager : IQueueSubscriptionManager
    {
        private readonly ConcurrentDictionary<string, QueueSubscription> _subscriptions;

        #region Constructors

        public SubscriptionManager()
        {
            _subscriptions = new ConcurrentDictionary<string, QueueSubscription>();
        }

        #endregion Constructors

        #region IQueueSubscriptionManager Members

        public QueueSubscription GetSubscription(string exchange, string queueName)
        {
            return _subscriptions.GetOrAdd(Concatenate(exchange, queueName), new QueueSubscription(exchange, queueName));
        }

        #endregion IQueueSubscriptionManager Members

        private string Concatenate(params string[] keys)
        {
            return string.Join("::", keys);
        }
    }
}